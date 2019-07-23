using Google.ProtocolBuffers.DescriptorProtos;
using Google.ProtocolBuffers.Descriptors;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protogen
{
    public class NodeInfo
    {
        public int StartLine { get; set; }
        public int StartCol { get; set; }
        public int EndLine { get; set; }
        public int EndCol { get; set; }
        public string Type { get; set; } = "root";
        public string Content { get; set; } // Temp for debugging
        public string File { get; set; }
        public string Links { get; set; } = "";  // Temp for debugging

        public void ResolveContent(TextLineCollection lines)
        {
            if (StartLine == EndLine)
            {
                Content = lines[StartLine].ToString().Substring(StartCol, EndCol - StartCol);
            }
            else
            {
                var builder = new StringBuilder();
                builder.Append(lines[StartLine].ToString().Substring(StartCol));
                for (int i = StartLine + 1; i < EndLine; i++)
                {
                    builder.Append(lines[i]);
                }
                builder.Append(lines[EndLine].ToString().Substring(0, EndCol));
                Content = builder.ToString();
            }
        }

        private static Dictionary<string, Type> DescriptorProtos = new Dictionary<string, Type>
        {
            { "type", typeof(FileDescriptorProto) }, // This doesn't look right. Should remove
            { "service", typeof(ServiceDescriptorProto) },
            { "enum", typeof(EnumDescriptorProto) },
            { "field", typeof(FieldDescriptorProto) },
            { "method", typeof(MethodDescriptorProto) },
            { "message_type", typeof(DescriptorProto) },
            { "package", null },
            { "syntax", null },
            { "name", null },
            { "input_type", null },
            { "output_type", null },
            { "label", null },
            { "number", null },
        };

        public void ResolvePath(IList<int> pathList)
        {
            if (pathList.Count == 0)
            {
                return;
            }

            var resolvedMessageDescriptor = FileDescriptorProto.Descriptor;
            var resolvedTypeName = "";

            foreach (var path in pathList)
            {
                Links += $" => {path}";

                if (path == 0)
                {
                    continue;
                }

                // new in proto3
                if (path == 12)
                {
                    Type = "syntax";
                    return;
                }

                var field = resolvedMessageDescriptor.FindFieldByNumber(path);

                if (field == null)
                {
                    continue;
                }

                resolvedTypeName = field.Name;
                var matchingDescriptorProto = DescriptorProtos[resolvedTypeName];

                if (matchingDescriptorProto == null)
                {
                    continue;
                }

                var messageDescriptor = matchingDescriptorProto.GetProperty("Descriptor").GetValue(null);
                resolvedMessageDescriptor = (MessageDescriptor)messageDescriptor;
            }

            Type = resolvedTypeName;
        }
    }
}
