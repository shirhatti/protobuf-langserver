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
using System.Collections;
using System.IO;
using System.Text;

namespace Protogen
{
    class Info
    {
        public int StartLine { get; set; }
        public int StartCol { get; set; }
        public int EndLine { get; set; }
        public int EndCol { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public string File { get; set; }

        public void ResolveContent(string[] lines)
        {
            if (StartLine == EndLine)
            {
                Content = lines[StartLine].Substring(StartCol, EndCol - StartCol);
            }
            else
            {
                var builder = new StringBuilder();
                builder.Append(lines[StartLine].Substring(StartCol));
                for (int i = StartLine + 1; i < EndLine; i++)
                {
                    builder.Append(lines[i]);
                }
                builder.Append(lines[EndLine].Substring(0, EndCol));
                Content = builder.ToString();
            }
        }
    }

    public class Program
    {
        private static List<Type> DescriptorProtos = new List<Type>
        {
            typeof(FileDescriptorProto),
            typeof(ServiceDescriptorProto),
            typeof(EnumDescriptorProto),
            typeof(FileDescriptorProto),
            typeof(FieldDescriptorProto),
        };

        private static string ResolveType(IList<int> pathList)
        {
            if (pathList.Count == 0)
            {
                return "root";
            }

            var resolvedMessageDescriptor = FileDescriptorProto.Descriptor;
            var resolvedTypeName= "";

            foreach (var path in pathList)
            {
                if (path == 0)
                {
                    continue;
                }

                // new in proto3
                if (path == 12)
                {
                    return "syntax";
                }

                var field = resolvedMessageDescriptor.FindFieldByNumber(path);

                if (field == null)
                {
                    continue;
                }

                resolvedTypeName = field.Name;
                var matchingDescriptorProto = DescriptorProtos.SingleOrDefault(t => t.Name.StartsWith(resolvedTypeName, StringComparison.OrdinalIgnoreCase));

                if (matchingDescriptorProto == null)
                {
                    continue;
                }

                var messageDescriptor = matchingDescriptorProto.GetProperty("Descriptor").GetValue(null);
                resolvedMessageDescriptor = (MessageDescriptor)messageDescriptor;
            }

            return resolvedTypeName;
        }

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
                var lines = File.ReadAllLines("D:\\protobuf-langserver\\protoc\\greet.proto");
                var infoList = new List<Info>();

                Debugger.Launch();
                foreach (var file in request.ProtoFileList)
                {
                    foreach (var location in file.SourceCodeInfo.LocationList)
                    {
                        var info = new Info
                        {
                            StartLine = location.SpanList[0],
                            StartCol = location.SpanList[1],
                            EndLine = location.SpanCount == 3 ? location.SpanList[0] : location.SpanList[2],
                            EndCol = location.SpanCount == 3 ? location.SpanList[2] : location.SpanList[3],
                            File = file.Name,
                            Type = ResolveType(location.PathList)
                        };

                        info.ResolveContent(lines);

                        infoList.Add(info);
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
