using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ProtobufLanguageServer.Documents
{
    public class ProjectSnapshotManager
    {
        public ProjectSnapshotManager(WorkspaceSnapshot workspace)
        {
            WorkspaceSnapshot = workspace;
        }

        public event EventHandler<WorkspaceSnapshotChangeEventArgs> Changed;

        private HashSet<string> _openFiles = new HashSet<string>();

        public bool IsDocumentOpen(string documentFilePath)
        {
            return _openFiles.Contains(documentFilePath);
        }

        public Workspace Workspace {get; set;}

        public WorkspaceSnapshot WorkspaceSnapshot {get; set;}

        public async Task DocumentAdded(string documentFilePath, TextLoader textLoader)
        {
            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            if (textLoader == null)
            {
                throw new ArgumentNullException(nameof(textLoader));
            }

            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            var textAndVersion = await textLoader.LoadTextAndVersionAsync(Workspace, null, CancellationToken.None);
            var docSnapshot = new DocumentSnapshot(documentFilePath, 0, VersionStamp.Default, textAndVersion.Text);
            var newDocs = WorkspaceSnapshot.Documents.Add(documentFilePath, docSnapshot);
            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);
        }

        public void DocumentChanged(string documentFilePath, SourceText sourceText, long textVersion)
        {
            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            if (sourceText == null)
            {
                throw new ArgumentNullException(nameof(sourceText));
            }

            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            Debug.Assert(WorkspaceSnapshot.Documents.ContainsKey(documentFilePath));
            var docVersion = WorkspaceSnapshot.Documents[documentFilePath].DocumentVersion.GetNewerVersion();
            var docSnapshot = new DocumentSnapshot(documentFilePath, textVersion, docVersion, sourceText);
            var newDocs = WorkspaceSnapshot.Documents.SetItem(documentFilePath, docSnapshot);

            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);
        }

        public void DocumentRemoved(string documentFilePath)
        {
            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            var newDocs = WorkspaceSnapshot.Documents.Remove(documentFilePath);
            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);
        }

        public void DocumentOpened(string documentFilePath, long textVersion, SourceText sourceText)
        {
            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            if (sourceText == null)
            {
                throw new ArgumentNullException(nameof(sourceText));
            }

            _openFiles.Add(documentFilePath);
            var stamp = VersionStamp.Default;
            var docSnapshot = new DocumentSnapshot(documentFilePath, textVersion, stamp, sourceText);
            var newDocs = WorkspaceSnapshot.Documents.SetItem(documentFilePath, docSnapshot);
            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);
        }

        public async Task DocumentClosed(string documentFilePath, long textVersion, TextLoader textLoader)
        {
            var textAndVersion = await textLoader.LoadTextAndVersionAsync(Workspace, null, CancellationToken.None);
            _openFiles.Remove(documentFilePath);
            
            var docSnapshot = new DocumentSnapshot(documentFilePath, textVersion, textAndVersion.Version, textLoader);
            var newDocs = WorkspaceSnapshot.Documents.SetItem(documentFilePath, docSnapshot);
            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);
        }
    }
}