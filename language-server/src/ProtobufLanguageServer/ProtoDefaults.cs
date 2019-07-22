using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ProtobufLanguageServer
{
    public static class ProtoDefaults
    {
        public static DocumentSelector Selector { get; } = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.proto"
            });
    }
}