using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Compiler.PluginProto;
using Google.ProtocolBuffers.DescriptorProtos;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
            try
            {
                using var input = Console.OpenStandardInput();
                request = CodeGeneratorRequest.ParseFrom(input, extensionRegistry);
            }
            catch (Exception e)
            {
                response.Error += e.ToString();
            }

            return 0;
        }
    }
}
