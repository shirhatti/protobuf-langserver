using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Compiler.PluginProto;
using Google.ProtocolBuffers.DescriptorProtos;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Google.ProtocolBuffers.Descriptors;
using System.IO;
using System.Text;

namespace Protogen
{

    public class Program
    {

        public static int Main(string[] args)
        {
            DescriptorProtoFile.Descriptor.ToString();
            ExtensionRegistry extensionRegistry = ExtensionRegistry.CreateInstance();
            CSharpOptions.RegisterAllExtensions(extensionRegistry);

            CodeGeneratorRequest request;
            var response = new CodeGeneratorResponse.Builder();
            var infoList = new List<NodeInfo>();
            try
            {
                using var input = Console.OpenStandardInput();
                request = CodeGeneratorRequest.ParseFrom(input, extensionRegistry);
                var lines = File.ReadAllLines("C:\\gh\\protobuf-langserver\\protoc\\greet.proto");

                Debugger.Launch();
                foreach (var file in request.ProtoFileList)
                {
                    Node ast = null;

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
                        info.ResolveContent(lines);

                        if (info.Type == "label")
                        {
                            // There's a bug in protobuf where labels in version 3 isn't parsed correctly
                            info.EndLine = info.StartLine;
                            info.EndCol = lines[info.StartLine].Length - 1;
                        }

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
            }
            catch (Exception e)
            {
                response.Error += e.ToString();
            }

                return 0;
        }
    }
}
