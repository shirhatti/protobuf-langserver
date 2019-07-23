using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.CodeAnalysis.Text;
using ProtobufLanguageServer.Syntax;
using Microsoft.CodeAnalysis;

namespace ProtobufLanguageServer.Documents
{
    public class DocumentSnapshot
    {
        public DocumentSnapshot(
            string path,
            long textVersion,
            Microsoft.CodeAnalysis.VersionStamp stamp,
            TextLoader loader)
        {
            Path = path;
            TextVersion = textVersion;
            DocumentVersion = stamp;
            _loader = loader;
        }

        public DocumentSnapshot(
            string path,
            long textVersion,
            Microsoft.CodeAnalysis.VersionStamp stamp,
            SourceText text)
        {
            Path = path;
            TextVersion = textVersion;
            _text = text;
            DocumentVersion = stamp;
        }

        public string Path {get;}
        public long TextVersion {get;}
        public Microsoft.CodeAnalysis.VersionStamp DocumentVersion {get;} 

        private TextLoader _loader;
        private SourceText _text = null;

        public Task<SourceText> GetTextAsync()
        {
            if(_text == null)
            {
                throw new NotImplementedException();
                //return (await _loader.LoadTextAndVersionAsync(Workspace, null, CancellationToken.None));
            }
            else{
                return Task.FromResult(_text);
            }
        }

        public Task<ProtobufLanguageServer.Syntax.SyntaxTree> GetSyntaxTreeAsync()
        {
            var syntaxTree = Syntax.SyntaxTree.Create(Path, _text);

            return Task.FromResult(syntaxTree);
        }
    }
}