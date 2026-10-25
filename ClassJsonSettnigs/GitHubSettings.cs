using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubRepoCheker.ClassJsonSettnigs
{
    public class ModulGithub
    {
        private readonly GitHubSettings _Gitsettings;
        private readonly UserAgentSettings _UserAgent;
        public ModulGithub(IOptions<GitHubSettings> options, IOptions<UserAgentSettings> Agent)
        {
            _Gitsettings = options.Value;
            _UserAgent = Agent.Value;
        }

        public async Task RunAsync()
        {

        }
    }
    public class GitHubSettings
    {
        public int TimeOut { get; set; }
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
        public double Version { get; set; }
    }
    public static class RepoData
    {
        public static List<string> repos = new() // collection with all urls
    {
        "krustu/intermediate-CSpractice",
        "krustu/CSharp-GitPracrice",
        "krustu/RealEstateAggregator",
        "krustu/RPG-manager",
        "krustu/Zabka-Catalog-LINQ"
    };
    }
}
/*
  "GitHubSettings": {
    "TimeOut": 10,
    "UserAgent": {
      "Name": "KrustuSHN",
      "Version": 1.0
    },
    "TotalTimeoutSeconds": 60,
    "Repos": [
      "krustu/intermediate-CSpractice",
      "krustu/CSharp-GitPracrice",
      "krustu/RealEstateAggregator",
      "krustu/RPG-manager",
      "krustu/Zabka-Catalog-LINQ"
    ],
    "MaxAttempts": 4,
    "DelayBetweenRequestsMs": 1000,
    "BackoffBase": 2.0
  }
} 
*/