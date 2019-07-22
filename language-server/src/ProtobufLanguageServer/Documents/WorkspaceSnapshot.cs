using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ProtobufLanguageServer.Documents
{
    public class WorkspaceSnapshot
    {
        public WorkspaceSnapshot(VersionStamp version, ImmutableDictionary<string, DocumentSnapshot> documents)
        {
            Version = version;
            Documents = documents;
        }

        public VersionStamp Version {get;}

        public ImmutableDictionary<string, DocumentSnapshot> Documents {get;}
    }
}