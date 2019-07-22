using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Compiler.PluginProto;
using Google.ProtocolBuffers.DescriptorProtos;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                //var jobj = new JObject();

                Debugger.Launch();
                var protoDescriptorProto = request.GetProtoFile(0);
                foreach (var item in request.ProtoFileList)
                {
                    //var sourceCodeInfoJson = JsonConvert.SerializeObject(item.SourceCodeInfo, Formatting.None, new JsonSerializerSettings()
                    //{
                    //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    //});
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
