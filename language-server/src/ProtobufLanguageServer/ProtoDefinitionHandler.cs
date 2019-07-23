
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ProtobufLanguageServer.Documents;
using Protogen;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

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
                () =>
                {
                    _snapshotManager.TryResolveDocument(request.TextDocument.Uri.AbsolutePath, out var doc);
                    return doc;
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                _threadManager.ForegroundScheduler);

            var syntaxTree = await document.GetSyntaxTreeAsync();

            var child = syntaxTree.Root.GetChildNodeAt((int)request.Position.Line, (int)request.Position.Character);

            if (child.Parent.Info.Type == "method" && (child.Info.Type == "input_type" || child.Info.Type == "output_type"))
            {
                var declaringTypeNode = FindDeclaringTypeNode(syntaxTree.Root, child.Info.Content);
                if (declaringTypeNode != null)
                {
                    var declaringTypeNodeLocation = new LocationOrLocationLink(
                        new Location()
                        {
                            Range = new Range(
                                new Position(declaringTypeNode.Info.StartLine, declaringTypeNode.Info.StartCol),
                                new Position(declaringTypeNode.Info.EndLine, declaringTypeNode.Info.EndCol)),
                            Uri = request.TextDocument.Uri,
                        });
                    var locations = new LocationOrLocationLinks(declaringTypeNodeLocation);
                    return locations;
                }
            }

            var emptyLocations = new LocationOrLocationLinks();
            return emptyLocations;
        }

        private Node FindDeclaringTypeNode(Node node, string content)
        {
            if (node.Info.Type == "message_type")
            {
                var delcaringMessageType = node.Children.FirstOrDefault(c => c.Info.Type == "name" && c.Info.Content == content);

                if (delcaringMessageType != null)
                {
                    // Found
                    return delcaringMessageType;
                }
            }

            for (var i = 0; i < node.Children.Count; i++)
            {
                var foundNode = FindDeclaringTypeNode(node.Children[i], content);

                if (foundNode != null)
                {
                    // Found
                    return foundNode;
                }
            }

            return null;
        }

        public void SetCapability(DefinitionCapability capability)
        {
            _capability = capability;
        }
    }
}