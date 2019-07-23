using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protogen
{
    public class Node
    {
        public Node Parent { get; set; }
        public NodeInfo Info { get; set; }
        public List<Node> Children { get; } = new List<Node>();

        public Node GetChildNodeAt(int line, int col)
        {
            return Children
                .SingleOrDefault((n => (n.Info.StartLine < line || (n.Info.StartLine == line && n.Info.StartCol <= col)) && ( line < n.Info.EndLine || (line == n.Info.EndLine && col <= n.Info.EndCol))))
                ?.GetChildNodeAt(line, col)
                ?? this;
        }
    }
}
