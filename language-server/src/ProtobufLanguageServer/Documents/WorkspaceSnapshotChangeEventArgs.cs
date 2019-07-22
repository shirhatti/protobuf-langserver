using System;

namespace ProtobufLanguageServer.Documents
{
    public class WorkspaceSnapshotChangeEventArgs : EventArgs
    {
        public WorkspaceSnapshotChangeEventArgs(WorkspaceSnapshot older, WorkspaceSnapshot newer, ProjectChangeKind kind)
        {
            if (older == null && newer == null)
            {
                throw new ArgumentException("Both projects cannot be null.");
            }

            Older = older;
            Newer = newer;
            Kind = kind;
        }

        public WorkspaceSnapshotChangeEventArgs(WorkspaceSnapshot older, WorkspaceSnapshot newer, string documentFilePath, ProjectChangeKind kind)
        {
            if (older == null && newer == null)
            {
                throw new ArgumentException("Both projects cannot be null.");
            }

            Older = older;
            Newer = newer;
            DocumentFilePath = documentFilePath;
            Kind = kind;
        }

        public WorkspaceSnapshot Older { get; }

        public WorkspaceSnapshot Newer { get; }

        public string DocumentFilePath { get; }

        public ProjectChangeKind Kind { get; }
    } 
}