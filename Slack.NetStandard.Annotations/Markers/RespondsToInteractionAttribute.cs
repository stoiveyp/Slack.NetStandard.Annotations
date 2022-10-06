using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slack.NetStandard.Annotations.Markers
{
    public class RespondsToInteractionAttribute:Attribute
    {
        public RespondsToInteractionAttribute(){}

        public RespondsToInteractionAttribute(string callbackID)
        {
            
        }
    }
}
