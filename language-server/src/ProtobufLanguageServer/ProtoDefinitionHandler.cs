
using System;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ProtobufLanguageServer.Documents;

namespace ProtobufLanguageServer
{
    public class ProtoDefinitionEndpoint : IDefinitionHandler
    {
        private readonly ILanguageServer _router;
        private readonly ForegroundThreadManager _threadManager;
        private readonly WorkspaceSnapshotManager _snapshotManager;

        private DefinitionCapability _capability;

        public ProtoDefinitionEndpoint(ForegroundThreadManager threadManager, ILanguageServer router, WorkspaceSnapshotManager snapshotManager)
        {
            _snapshotManager = snapshotManager;
            _threadManager = threadManager;
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        public TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions()
            {
                DocumentSelector = ProtoDefaults.Selector,
            };
        }

        public async Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Go to definition request at line: " + (request.Position.Line + 1),
            });

            _threadManager.AssertBackgroundThread();

            var document = await Task.Factory.StartNew(
                () => {
                    _snapshotManager.TryResolveDocument(request.TextDocument.Uri.AbsolutePath, out var doc);
                    return doc;
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                _threadManager.ForegroundScheduler);

            var syntaxTree = await Task.Factory.StartNew(
                async () => await document.GetSyntaxTreeAsync(),
                CancellationToken.None,
                TaskCreationOptions.None,
                _threadManager.BackgroundScheduler);

            // TODO: Do something useful with this syntax tree.

            var location1 = new LocationOrLocationLink(new Location()
            {
                Range = new Range(new Position(0, 0), new Position(0, 5)),
                Uri = request.TextDocument.Uri,
            });

            var location2 = new LocationOrLocationLink(new Location()
            {
                Range = new Range(new Position(0, 7), new Position(0, 11)),
                Uri = request.TextDocument.Uri,
            });

            var locations = new LocationOrLocationLinks(location1, location2);

            return locations;
        }

        public void SetCapability(DefinitionCapability capability)
        {
            _capability = capability;
        }
    }
}