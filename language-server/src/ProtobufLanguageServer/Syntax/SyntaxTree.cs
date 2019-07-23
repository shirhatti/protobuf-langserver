using Microsoft.CodeAnalysis.Text;
using Protogen;

namespace ProtobufLanguageServer.Syntax
{
    public class SyntaxTree
    {
        private SyntaxTree(Node root)
        {
            Root = root;
        }
        
        public Node Root { get; }

        public static SyntaxTree Create(string path, SourceText text)
        {
            var root = ConsoleApp.Program.GetSyntaxTree(path, text);
            return new SyntaxTree(root);
        }
    }
}