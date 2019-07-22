using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace ProtobufLanguageServer.Documents
{
    public class WorkspaceSnapshotManager
    {
        public WorkspaceSnapshotManager(ForegroundThreadManager threadManager)
        {
            _threadManager = threadManager;
        }

        public event EventHandler<WorkspaceSnapshotChangeEventArgs> Changed;

        private ForegroundThreadManager _threadManager;

        private HashSet<string> _openFiles = new HashSet<string>(FilePathComparer.Instance);

        public bool IsDocumentOpen(string documentFilePath)
        {
            return _openFiles.Contains(documentFilePath);
        }

        public WorkspaceSnapshot WorkspaceSnapshot {
            get {
                _threadManager.AssertForegroundThread();
                if(_workspaceSnapshot == null)
                {
                    _workspaceSnapshot = new WorkspaceSnapshot(VersionStamp.Default, ImmutableDictionary<string, DocumentSnapshot>.Empty);
                }
                return _workspaceSnapshot;
            }
            set{
                _threadManager.AssertForegroundThread();
                _workspaceSnapshot = value;
            }
        }

        private WorkspaceSnapshot _workspaceSnapshot;

        public bool TryResolveDocument(string documentFilePath, out DocumentSnapshot document)
        {
            return _workspaceSnapshot.Documents.TryGetValue(documentFilePath, out document);
        }

        public void DocumentAdded(string documentFilePath, TextLoader textLoader)
        {
            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            if (textLoader == null)
            {
                throw new ArgumentNullException(nameof(textLoader));
            }

            _threadManager.AssertForegroundThread();

            var oldWorkspaceSnapshot = WorkspaceSnapshot;

            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            var docSnapshot = new DocumentSnapshot(documentFilePath, 0, VersionStamp.Default, textLoader);
            var newDocs = WorkspaceSnapshot.Documents.Add(documentFilePath, docSnapshot);
            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);

            NotifyListeners(new WorkspaceSnapshotChangeEventArgs(oldWorkspaceSnapshot, WorkspaceSnapshot, ProjectChangeKind.DocumentAdded));
            NotifyListeners(new WorkspaceSnapshotChangeEventArgs(oldWorkspaceSnapshot, WorkspaceSnapshot, ProjectChangeKind.DocumentChanged));
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

            _threadManager.AssertForegroundThread();

            var oldWorkspaceSnapshot = WorkspaceSnapshot;

            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            Debug.Assert(WorkspaceSnapshot.Documents.ContainsKey(documentFilePath));
            var docVersion = WorkspaceSnapshot.Documents[documentFilePath].DocumentVersion.GetNewerVersion();
            var docSnapshot = new DocumentSnapshot(documentFilePath, textVersion, docVersion, sourceText);
            var newDocs = WorkspaceSnapshot.Documents.SetItem(documentFilePath, docSnapshot);

            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);

            NotifyListeners(new WorkspaceSnapshotChangeEventArgs(oldWorkspaceSnapshot, WorkspaceSnapshot, ProjectChangeKind.DocumentChanged));
        }

        public void DocumentRemoved(string documentFilePath)
        {
            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            _threadManager.AssertForegroundThread();

            var oldWorkspaceSnapshot = WorkspaceSnapshot;
            var newDocs = WorkspaceSnapshot.Documents.Remove(documentFilePath);
            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);

            NotifyListeners(new WorkspaceSnapshotChangeEventArgs(oldWorkspaceSnapshot, WorkspaceSnapshot, ProjectChangeKind.DocumentRemoved));
            NotifyListeners(new WorkspaceSnapshotChangeEventArgs(oldWorkspaceSnapshot, WorkspaceSnapshot, ProjectChangeKind.DocumentChanged));
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

            _threadManager.AssertForegroundThread();

            var oldWorkspaceSnapshot = WorkspaceSnapshot;

            _openFiles.Add(documentFilePath);
            var stamp = VersionStamp.Default;
            var docSnapshot = new DocumentSnapshot(documentFilePath, textVersion, stamp, sourceText);
            var newDocs = WorkspaceSnapshot.Documents.SetItem(documentFilePath, docSnapshot);
            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);
        
            NotifyListeners(new WorkspaceSnapshotChangeEventArgs(oldWorkspaceSnapshot, WorkspaceSnapshot, documentFilePath, ProjectChangeKind.DocumentChanged));
        }

        public void DocumentClosed(string documentFilePath, TextLoader textLoader)
        {
            _threadManager.AssertForegroundThread();
            _openFiles.Remove(documentFilePath);

            var oldWorkspaceSnapshot = WorkspaceSnapshot;
            var oldDocSnapshot = WorkspaceSnapshot.Documents[documentFilePath];
            var newDocSnapshot = new DocumentSnapshot(documentFilePath, -1, oldDocSnapshot.DocumentVersion.GetNewerVersion(), textLoader);
            var newDocs = WorkspaceSnapshot.Documents.SetItem(documentFilePath, newDocSnapshot);
            var newVersion = WorkspaceSnapshot.Version.GetNewerVersion();
            WorkspaceSnapshot = new WorkspaceSnapshot(newVersion, newDocs);

            NotifyListeners(new WorkspaceSnapshotChangeEventArgs(oldWorkspaceSnapshot, WorkspaceSnapshot, documentFilePath, ProjectChangeKind.DocumentChanged));
        }

        protected virtual void NotifyListeners(WorkspaceSnapshotChangeEventArgs e)
        {
            _threadManager.AssertForegroundThread();

            var handler = Changed;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}