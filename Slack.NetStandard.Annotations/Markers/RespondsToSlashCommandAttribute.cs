using Slack.NetStandard.Interaction;

namespace Slack.NetStandard.Annotations.Markers;

public class RespondsToSlashCommandAttribute : Attribute
{
    public RespondsToSlashCommandAttribute(string command)
    {
        
    }
}