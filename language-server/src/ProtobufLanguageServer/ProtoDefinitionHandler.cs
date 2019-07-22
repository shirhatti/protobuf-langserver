
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace ProtobufLanguageServer
{
    public class ProtoDefinitionEndpoint : IDefinitionHandler
    {
        private readonly ILanguageServer _router;
        private readonly ForegroundThreadManager _threadManager;

        private DefinitionCapability _capability;

        public ProtoDefinitionEndpoint(ForegroundThreadManager threadManager, ILanguageServer router)
        {
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

        public Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Go to definition request at line: " + (request.Position.Line + 1),
            });

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

            return Task.FromResult(locations);
        }

        public void SetCapability(DefinitionCapability capability)
        {
            _capability = capability;
        }
    }
}