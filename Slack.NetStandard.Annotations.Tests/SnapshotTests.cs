namespace Slack.NetStandard.Annotations.Tests
{
    [UsesVerify]
    public class SnapshotTests
    {
        [Fact]
        public Task RespondsToEvent()
        {
            var sampleCode = System.IO.File.ReadAllText("Examples/EventHandler.cs");
            return Utility.Verify(sampleCode);
        }

        [Fact]
        public Task RespondsToSlashCommand()
        {
            var sampleCode = System.IO.File.ReadAllText("Examples/SlashCommand.cs");
            return Utility.Verify(sampleCode);
        }

        [Fact]
        public Task InteractionPayload()
        {
            var sampleCode = System.IO.File.ReadAllText("Examples/InteractionPayload.cs");
            return Utility.Verify(sampleCode);
        }
    }
}