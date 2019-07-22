using Google.ProtocolBuffers.DescriptorProtos;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleApp
{
    class Info
    {
        public int StartLine { get; set; }
        public int StartCol { get; set; }
        public int EndLine { get; set; }
        public int EndCol { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
    }
    class Program
    {
        [DllImport("libminiprotoc.dll")]
        public static extern bool generate(IntPtr filePtr, IntPtr descriptorProtoPtr);
        static void Main(string[] args)
        {
            string text = File.ReadAllText("greet.proto");
            using (var inputSlab = new MemoryPoolSlab(Encoding.ASCII.GetBytes(text)))
            using (var outputSlab = new MemoryPoolSlab(new byte[1000000]))
            {
                generate(inputSlab.NativePointer, outputSlab.NativePointer);
                var infoList = new List<Info>();
                var file = FileDescriptorProto.ParseFrom(outputSlab.Array);
                foreach (var location in file.SourceCodeInfo.LocationList)
                {
                    infoList.Add(new Info
                    {
                        StartLine = location.SpanList[0],
                        StartCol = location.SpanList[1],
                        EndLine = location.SpanCount == 3 ? location.SpanList[0] : location.SpanList[2],
                        EndCol = location.SpanCount == 3 ? location.SpanList[2] : location.SpanList[3],
                        File = file.Name,
                        //Type = location.DescriptorForType.,
                        Name = file.Name,
                    });
                }
            }
            Console.WriteLine("Hello World!");
        }
    }
}
