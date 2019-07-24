using Microsoft.CodeAnalysis.Text;
using System.Linq;
using static Google.ProtocolBuffers.DescriptorProtos.SourceCodeInfo.Types;

namespace Protogen
{
    public class NamedNode : Node
    {
        private string _name;

        public NamedNode(Location location, SourceText text) : base(location, text) { }

        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = (Children.SingleOrDefault(c => c is NameNode) as NameNode)?.Name ?? string.Empty;
                }

                return _name;
            }
        }
    }
}
