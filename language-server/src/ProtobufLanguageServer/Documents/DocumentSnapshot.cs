using System.Threading.Tasks;
using System;
using Microsoft.CodeAnalysis.Text;
using ProtobufLanguageServer.Syntax;

namespace ProtobufLanguageServer.Documents
{
    public class DocumentSnapshot
    {
        public DocumentSnapshot(
            string path,
            long version,
            Func<Task<SourceText>> loader
        )
        {
            Path = path;
            Version = version;
            _loader = loader;
        }

        public DocumentSnapshot(string path, long version, SourceText text)
        {
            path = Path;
            version = Version;
            _text = text;
        }

        public string Path {get;}
        public long Version {get;}

        private Func<Task<SourceText>> _loader;
        private SourceText _text = null;

        public async Task<SourceText> GetTextAsync()
        {
            if(_text == null)
            {
                return await _loader();
            }
            else{
                return _text;
            }
        }

        public Task<SyntaxTree> GetSyntaxTreeAsync()
        {
            throw new NotImplementedException();
        }
    }
}