using OmniSharp.Extensions.JsonRpc;

namespace ProtobufLanguageServer
{
    [Parallel, Method("proto/removeDocument")]
    public interface IProtoRemoveDocumentHandler : IJsonRpcRequestHandler<RemoveDocumentParams>
    {

    }
}