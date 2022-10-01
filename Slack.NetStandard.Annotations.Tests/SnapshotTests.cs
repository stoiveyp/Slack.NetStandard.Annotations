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
    }
}