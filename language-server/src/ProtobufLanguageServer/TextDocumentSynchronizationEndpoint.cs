using Microsoft.CodeAnalysis.Text;
using OmniSharp.Extensions.Embedded.MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using ProtobufLanguageServer.Documents;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ILanguageServer = OmniSharp.Extensions.LanguageServer.Server.ILanguageServer;

namespace ProtobufLanguageServer
{
    public class TextDocumentSynchronizationEndpoint : ITextDocumentSyncHandler
    {
        private readonly ILanguageServer _router;
        private readonly WorkspaceSnapshotManager _snapshotManager;
        private readonly ForegroundThreadManager _threadManager;

        public TextDocumentSynchronizationEndpoint(
            ILanguageServer router,
            WorkspaceSnapshotManager snapshotManager,
            ForegroundThreadManager threadManager)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _snapshotManager = snapshotManager;
            _threadManager = threadManager;
        }

        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Incremental;

        public async Task<Unit> Handle(DidChangeTextDocumentParams notification, CancellationToken token)
        {
            _threadManager.AssertBackgroundThread();
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Proto file changed: " + notification.TextDocument.Uri.AbsolutePath,
            });

            var document = await Task.Factory.StartNew(() => 
            {
                _snapshotManager.TryResolveDocument(notification.TextDocument.Uri.AbsolutePath, out var documentSnapshot);
                return documentSnapshot;
            });

            var sourceText = await document.GetTextAsync();
            sourceText = ApplyContentChanges(notification.ContentChanges, sourceText);

            await Task.Factory.StartNew(
                () => _snapshotManager.DocumentChanged(notification.TextDocument.Uri.AbsolutePath, sourceText, notification.TextDocument.Version),
                CancellationToken.None,
                TaskCreationOptions.None,
                _threadManager.ForegroundScheduler);
            return Unit.Value;
        }

        public async Task<Unit> Handle(DidOpenTextDocumentParams notification, CancellationToken token)
        {
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Proto file opened: " + notification.TextDocument.Uri.AbsolutePath,
            });
            _threadManager.AssertBackgroundThread();

            var sourceText = SourceText.From(notification.TextDocument.Text);

            await Task.Factory.StartNew(
                () => _snapshotManager.DocumentOpened(notification.TextDocument.Uri.AbsolutePath, notification.TextDocument.Version, sourceText),
                CancellationToken.None,
                TaskCreationOptions.None,
                _threadManager.ForegroundScheduler);

            return Unit.Value;
        }

        public async Task<Unit> Handle(DidCloseTextDocumentParams notification, CancellationToken token)
        {
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Proto file closed: " + notification.TextDocument.Uri.AbsolutePath,
            });
            _threadManager.AssertBackgroundThread();

            await Task.Factory.StartNew(
                () => _snapshotManager.DocumentClosed(notification.TextDocument.Uri.AbsolutePath, new ThrowTextLoader()),
                CancellationToken.None,
                TaskCreationOptions.None,
                _threadManager.ForegroundScheduler);

            return Unit.Value;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams notification, CancellationToken token)
        {
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Proto file saved: " + notification.TextDocument.Uri.AbsolutePath,
            });

            return Unit.Task;
        }

        public void SetCapability(SynchronizationCapability capability)
        {
        }

        TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions()
            {
                DocumentSelector = ProtoDefaults.Selector,
                SyncKind = Change
            };
        }

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions()
            {
                DocumentSelector = ProtoDefaults.Selector,
            };
        }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentSaveRegistrationOptions()
            {
                DocumentSelector = ProtoDefaults.Selector,
                IncludeText = true
            };
        }
        public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
        {
            return new TextDocumentAttributes(uri, "proto");
        }

        internal SourceText ApplyContentChanges(IEnumerable<TextDocumentContentChangeEvent> contentChanges, SourceText sourceText)
        {
            foreach (var change in contentChanges)
            {
                var linePosition = new LinePosition((int)change.Range.Start.Line, (int)change.Range.Start.Character);
                var position = sourceText.Lines.GetPosition(linePosition);
                var textSpan = new TextSpan(position, change.RangeLength);
                var textChange = new TextChange(textSpan, change.Text);

                _router.Window.LogInfo("Applying " + textChange);

                // If there happens to be multiple text changes we generate a new source text for each one. Due to the
                // differences in VSCode and Roslyn's representation we can't pass in all changes simultaneously because
                // ordering may differ.
                sourceText = sourceText.WithChanges(textChange);
            }

            return sourceText;
        }
    }
}
