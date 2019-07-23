using Google.ProtocolBuffers.DescriptorProtos;
using Microsoft.CodeAnalysis.Text;
using Protogen;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class Program
    {
        [DllImport("libminiprotoc.dll")]
        public static extern bool generate(IntPtr file, Int64 fileSize, IntPtr descriptorProtoPtr, out Int64 descriptorSize);
        
        public static async Task Main()
        {
            var stringInput = await File.ReadAllTextAsync("greet.proto");
            var text = SourceText.From(stringInput);
            var ast = GetSyntaxTree("greet.proto", text);
        }

        public static Node GetSyntaxTree(string path, SourceText text)
        {
            var chars = new char[text.Length];
            text.CopyTo(0, chars, 0, text.Length);
            var resolvedEncoding = text.Encoding ?? Encoding.UTF8;
            var inputFile = resolvedEncoding.GetBytes(chars);
            var infoList = new List<NodeInfo>();
            var lines = text.Lines;
            Node ast = null;
            using (var inputSlab = new MemoryPoolSlab(inputFile))
            using (var outputSlab = new MemoryPoolSlab(new byte[1000000]))
            {

                var parseStatus = generate(inputSlab.NativePointer, inputFile.Length, outputSlab.NativePointer, out var descriptorSize);
                var byteArray = new byte[descriptorSize];
                Marshal.Copy(outputSlab.NativePointer, byteArray, 0, (int)descriptorSize);
                var file = FileDescriptorProto.ParseFrom(byteArray);

                foreach (var location in file.SourceCodeInfo.LocationList)
                {
                    var info = new NodeInfo
                    {
                        StartLine = location.SpanList[0],
                        StartCol = location.SpanList[1],
                        EndLine = location.SpanCount == 3 ? location.SpanList[0] : location.SpanList[2],
                        EndCol = location.SpanCount == 3 ? location.SpanList[2] : location.SpanList[3],
                        File = file.Name
                    };

                    info.ResolvePath(location.PathList);

                    if (info.Type == "label")
                    {
                        // There's a bug in protobuf where labels in version 3 isn't parsed correctly
                        info.EndLine = info.StartLine;
                        info.EndCol = lines[info.StartLine].ToString().Length - 1;
                    }

                    info.ResolveContent(lines);


                    infoList.Add(info);
                }

                foreach (var info in infoList.OrderByDescending(i => i.Content.Length))
                {

                    if (ast == null)
                    {
                        ast = new Node { Info = info };
                    }
                    else
                    {
                        var node = ast.GetChildNodeAt(info.StartLine, info.StartCol);
                        node.Children.Add(new Node
                        {
                            Info = info,
                            Parent = node
                        });
                    }
                }
            }
            return ast;
        }
    }
}
