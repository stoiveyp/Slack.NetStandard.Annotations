namespace Slack.NetStandard.Annotations.Markers
{
    public class RespondsToInteractionAttribute:Attribute
    {
        public RespondsToInteractionAttribute(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; set; }
    }
}
