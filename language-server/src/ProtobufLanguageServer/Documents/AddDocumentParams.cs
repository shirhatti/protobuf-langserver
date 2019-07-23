using OmniSharp.Extensions.Embedded.MediatR;

namespace ProtobufLanguageServer
{
    public class AddDocumentParams : IRequest
    {
        public string FilePath { get; set; }
    }
}