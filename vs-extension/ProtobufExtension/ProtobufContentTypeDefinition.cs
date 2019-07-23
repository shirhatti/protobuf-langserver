using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace ProtobufExtension
{
    public class FooContentDefinition
    {
        [Export]
        [Name("protobuf")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition ProtobufContentTypeDefinition;

        [Export]
        [FileExtension(".proto")]
        [ContentType("protobuf")]
        internal static FileExtensionToContentTypeDefinition ProtobufFileExtensionDefinition;
    }
}
