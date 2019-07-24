using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using static Google.ProtocolBuffers.DescriptorProtos.SourceCodeInfo.Types;

namespace Protogen
{
    public class RootNode : Node
    {
        private List<ServiceNode> _services;
        private List<MessageNode> _messages;

        public List<ServiceNode> Services
        {
            get
            {
                if (_services == null)
                {
                    _services = Children.Where(c => c is ServiceNode).Select(c => c as ServiceNode).ToList();
                }
                return _services;
            }
        }

        public List<MessageNode> Messages
        {
            get
            {
                if (_messages == null)
                {
                    _messages = Children.Where(c => c is MessageNode).Select(c => c as MessageNode).ToList();
                }
                return _messages;
            }
        }

        public RootNode(Location location, SourceText text) : base(location, text) { }


    }
}
