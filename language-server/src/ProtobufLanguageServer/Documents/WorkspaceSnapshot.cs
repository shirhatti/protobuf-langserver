using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace ProtobufLanguageServer.Documents
{
    public class WorkspaceSnapshot
    {
        public WorkspaceSnapshot(VersionStamp version, IReadOnlyList<DocumentSnapshot> documents)
        {
            Version = version;
            Documents = documents;
        }

        public VersionStamp Version {get;}

        public IReadOnlyList<DocumentSnapshot> Documents {get;}
    }
}