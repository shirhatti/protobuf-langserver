using Microsoft.CodeAnalysis.Text;
using System.Linq;
using static Google.ProtocolBuffers.DescriptorProtos.SourceCodeInfo.Types;

namespace Protogen
{
    public class NamedNode : Node
    {
        private NameNode _name;

        public NamedNode(Location location, SourceText text) : base(location, text) { }

        public NameNode NameNode
        {
            get
            {
                if (_name == null)
                {
                    _name = (Children.SingleOrDefault(c => c is NameNode) as NameNode);
                }

                return _name;
            }
        }

        public string Name => NameNode.Name;
    }
}
