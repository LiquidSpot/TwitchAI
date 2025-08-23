using System;

namespace TwitchAI.Application.Dto.Response.Activity
{
    public class DashboardActivityResponseDto
    {
        public int[] OnlineSamples { get; set; } = Array.Empty<int>();
        public string[] Labels { get; set; } = Array.Empty<string>();
        public string[] AiCommands { get; set; } = Array.Empty<string>();
        public int[] AiCommandCounts { get; set; } = Array.Empty<int>();
    }
}


