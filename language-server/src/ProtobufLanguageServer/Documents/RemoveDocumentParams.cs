using OmniSharp.Extensions.Embedded.MediatR;

namespace ProtobufLanguageServer
{
    public class RemoveDocumentParams : IRequest
    {
        public string FilePath { get; set; }
    }
}