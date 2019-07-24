using Microsoft.CodeAnalysis.Text;
using System.Linq;
using static Google.ProtocolBuffers.DescriptorProtos.SourceCodeInfo.Types;

namespace Protogen
{

    public class SyntaxNode : Node
    {
        public SyntaxNode(Location location, SourceText text) : base(location, text) { }
    }

    public class NumberNode : Node
    {
        public NumberNode(Location location, SourceText text) : base(location, text) { }
    }

    public class LabelNode : Node
    {
        public LabelNode(Location location, SourceText text) : base(location, text) { }
    }

    public class InputNode : Node
    {
        public InputNode(Location location, SourceText text) : base(location, text) { }
    }

    public class OutputNode : Node
    {
        public OutputNode(Location location, SourceText text) : base(location, text) { }
    }

    public class NameNode : Node
    {
        public NameNode(Location location, SourceText text) : base(location, text) { }

        public string Name => Content;
    }

    public class PackageNode : Node
    {
        public PackageNode(Location location, SourceText text) : base(location, text) { }
    }

    public class MessageNode : NamedNode
    {
        public MessageNode(Location location, SourceText text) : base(location, text) { }
    }

    public class ServiceNode : NamedNode
    {
        public ServiceNode(Location location, SourceText text) : base(location, text) { }
    }

    public class MethodNode : NamedNode
    {
        public MethodNode(Location location, SourceText text) : base(location, text) { }
    }

    public class FieldNode : Node
    {
        public FieldNode(Location location, SourceText text) : base(location, text) { }
    }

    public class EnumNode : Node
    {
        public EnumNode(Location location, SourceText text) : base(location, text) { }
    }

    public class TypeNode : Node
    {
        public TypeNode(Location location, SourceText text) : base(location, text) { }
    }
}
