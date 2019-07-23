
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ProtobufLanguageServer.Documents;

namespace ProtobufLanguageServer
{
    public class ProtoCompletionEndpoint : ICompletionHandler, ICompletionResolveHandler
    {
        private readonly ILanguageServer _router;
        private readonly ForegroundThreadManager _threadManager;
        private readonly WorkspaceSnapshotManager _snapshotManager;

        private CompletionCapability _capability;

        public ProtoCompletionEndpoint(ForegroundThreadManager threadManager, ILanguageServer router, WorkspaceSnapshotManager snapshotManager)
        {
            _threadManager = threadManager;
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _snapshotManager = snapshotManager;
        }

        public bool CanResolve(CompletionItem value)
        {
            return true;
        }

        public CompletionRegistrationOptions GetRegistrationOptions()
        {
            return new CompletionRegistrationOptions()
            {
                DocumentSelector = ProtoDefaults.Selector,
                ResolveProvider = true,
                TriggerCharacters = new Container<string>(
                    "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                    "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
                    "{", ".", ",", " ", "(", ";"),
            };
        }

        public async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Proto file completion list request at line: " + (request.Position.Line + 1),
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

            // Provide your completions here
            var item1 = new CompletionItem()
            {
                Label = "Sample completion item 1",
                InsertText = "SampleInsertText1"
            };
            var item2 = new CompletionItem()
            {
                Label = "Sample completion item 2",
                InsertText = "SampleInsertText2"
            };
            var completionList = new CompletionList(item1, item2);
            
            return Task.FromResult(completionList);
        }

        public Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
        {
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Proto file completion item request: " + request.Label,
            });

            //TODO: In the future this should probably be a bit more inteligent,
            // this is assuming we loaded all the data for a completion item on the innitial completion list.
            return Task.FromResult(request);
        }

        public void SetCapability(CompletionCapability capability)
        {
            _capability = capability;
        }
    }
}