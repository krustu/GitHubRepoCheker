using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubRepoCheker.ClassJsonSettnigs
{
    public class GitHubSettings
    {
        public int TimeOutSeconds { get; set; }
        public UserAgentSettings UserAgent { get; set; } = new();
        public int TotalTimeoutSeconds { get; set; }
        public List<string> Repos { get; set; } = new();
        public int MaxAttempts { get; set; }
        public int DelayBetweenRequestsMs { get; set; }
        public double BackoffBase { get; set; }
    }
    public class UserAgentSettings
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
    }
   
}
