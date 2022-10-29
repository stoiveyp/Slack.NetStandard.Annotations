using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;

namespace Slack.NetStandard.Annotations
{
    internal static class MarkerExtensions
    {
        public static bool IsSlashCommandMarker(this AttributeSyntax? markerInfo)
        {
            return markerInfo != null && markerInfo.MarkerName() == nameof(RespondsToSlashCommandAttribute).NameOnly();
        }

        public static bool IsEventMarker(this AttributeSyntax? markerInfo)
        {
            return markerInfo != null && markerInfo.MarkerName() == nameof(RespondsToEventAttribute).NameOnly();
        }

        public static bool IsInteractionMarker(this AttributeSyntax? markerInfo)
        {
            return markerInfo != null && markerInfo.MarkerName() == nameof(RespondsToInteractionAttribute).NameOnly();
        }
    }
}
