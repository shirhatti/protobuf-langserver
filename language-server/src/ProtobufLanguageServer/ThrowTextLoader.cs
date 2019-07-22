using Microsoft.CodeAnalysis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProtobufLanguageServer
{
    public class ThrowTextLoader : TextLoader
    {
        public override Task<TextAndVersion> LoadTextAndVersionAsync(Workspace workspace, DocumentId documentId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
