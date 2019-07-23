using OmniSharp.Extensions.JsonRpc;

namespace ProtobufLanguageServer
{
    [Parallel, Method("proto/addDocument")]
    internal interface IProtoAddDocumentHandler : IJsonRpcRequestHandler<AddDocumentParams>
    {
    }
}