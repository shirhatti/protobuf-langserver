using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Compiler.PluginProto;
using Google.ProtocolBuffers.DescriptorProtos;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Google.ProtocolBuffers.Descriptors;

namespace Protogen
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

    public class Program
    {
        public static int Main(string[] args)
        {
            DescriptorProtoFile.Descriptor.ToString();
            ExtensionRegistry extensionRegistry = ExtensionRegistry.CreateInstance();
            CSharpOptions.RegisterAllExtensions(extensionRegistry);

            CodeGeneratorRequest request;
            var response = new CodeGeneratorResponse.Builder();
            try
            {
                using var input = Console.OpenStandardInput();
                request = CodeGeneratorRequest.ParseFrom(input, extensionRegistry);
                //var jobj = new JObject();
                var infoList = new List<Info>();

                Debugger.Launch();
                foreach (var file in request.ProtoFileList)
                {
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
            }
            catch (Exception e)
            {
                response.Error += e.ToString();
            }

            return 0;
        }
    }
}
