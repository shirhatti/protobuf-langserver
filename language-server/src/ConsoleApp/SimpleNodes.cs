using Microsoft.CodeAnalysis.Text;
using System.Linq;
using static Google.ProtocolBuffers.DescriptorProtos.SourceCodeInfo.Types;

namespace Protogen
{
    public class RootNode : Node
    {
        public RootNode(Location location, SourceText text) : base(location, text) { }
    }

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

    public class MessageNode : Node
    {
        public MessageNode(Location location, SourceText text) : base(location, text) { }

        public NameNode NameNode => Children.SingleOrDefault(c => c is NameNode) as NameNode;
    }

    public class ServiceNode : Node
    {
        public ServiceNode(Location location, SourceText text) : base(location, text) { }
    }

    public class MethodNode : Node
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
