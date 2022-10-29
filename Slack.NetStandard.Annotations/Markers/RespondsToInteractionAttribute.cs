using Slack.NetStandard.Interaction;

namespace Slack.NetStandard.Annotations.Markers
{
    public class RespondsToInteractionAttribute:Attribute
    {
        public RespondsToInteractionAttribute(Type type)
        {
            
        }

        public RespondsToInteractionAttribute(Type type, string identifier)
        {

        }
    }
}
