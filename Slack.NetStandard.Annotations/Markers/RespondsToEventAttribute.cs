using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slack.NetStandard.Annotations.Markers
{
    public class RespondsToEventAttribute:Attribute
    {
        public RespondsToEventAttribute(Type eventType)
        {
            
        }
    }
}
