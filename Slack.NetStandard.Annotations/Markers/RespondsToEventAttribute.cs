using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slack.NetStandard.Annotations.Markers
{
    public class RespondsToEventAttribute<T> where T : Slack.NetStandard.EventsApi.Event
    {
        public Func<T,bool>? Validation { get; }

        public RespondsToEventAttribute(Func<T, bool> validation)
        {
            Validation = validation;
        }
    }

    public class RespondsToEventAttribute:Attribute
    {
        public RespondsToEventAttribute(){}
    }
}
