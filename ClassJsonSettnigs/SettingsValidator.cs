using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace GitHubRepoCheker.ClassJsonSettnigs
{
    public class ValidationHandler
    {
        private ValiationResults _result;
        public ValidationHandler( ValiationResults result)
        {
            _result = result;
        }
        //CheckCompatibility
        //GitHubSettings
       public GitHubSettings? ValidCompatibility(GitHubSettings settings)
       {
            bool CanWorkpath = HandleValidation();
            if (CanWorkpath)
                return settings;
            else
            {
                return null;
            }

        }
        private bool HandleValidation()
        {
            if (_result.Errors.Count == 0 && _result.Warnings.Count == 0)
                return true;
            else if (_result.Errors.Count() <= 1)
                return false;
            else
            {
                Console.WriteLine("Several warning were detected in the Json; do you want to continue?");
                Console.WriteLine("Yes - 2 ; No - 1");
                bool answer = InputChecker();
                if(answer == true)
                {
                    _result.ShowWarnings();
                    Console.ReadKey();
                    return true;
                }
                else
                {
                    _result.ShowErrors();
                    Console.ReadKey();
                    return false;
                }

            }
        }
        private bool InputChecker()
        {
            int result;
           
            while (true)
            { 
                if(int.TryParse(Console.ReadLine(), out 
                    result ) && result >= 0 && result <= 3)
                {
                    break;
                }
                Console.WriteLine("Write again 1 or 2");
            }
            if (result == 2)
                return true;

            else if (result == 1)
                return false;
            else
            {
                return false;
            }
        }
  
    }
    public class ValiationResults
    {
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();
        public void ShowWarnings()
        {
            int index = 0;
            foreach(var a in Warnings)
            {
                Console.WriteLine($"{index} - WARNING: {a}");
                index++;
            }
        }
        public void ShowErrors()
        {
            int index = 0;
            foreach (var a in Errors)
            {
                Console.WriteLine($"{index} - ERROR: {a}");
                index++;
            }
        }
    }
    public static class SettingsValidator
    {
        public static ValiationResults SettingsMistakes(GitHubSettings settings)
        {
            ValiationResults result = new();
            
            if(settings == null)
            {
                result.Errors.Add("Settings object is null.");
            }
           
            // Error part
            if (settings?.TimeOutSeconds < 1)
                result.Errors.Add("TimeOutSeconds must be at least 1.");

            if (settings?.MaxAttempts < 1)
                result.Errors.Add("MaxAttempts must be at least 1.");

            if (settings?.DelayBetweenRequestsMs < 0)
                result.Errors.Add("DelayBetweenRequestsMs cannot be negative.");

            if (settings?.Repos == null || settings.Repos.Count == 0)
            {
                result.Errors.Add("Repos cannot be empty.");
            }
            else
            {
                for (int i = 0; i < settings.Repos.Count; i++)
                {
                    string repo = settings.Repos[i];

                    if (string.IsNullOrWhiteSpace(repo))
                    {
                        result.Errors.Add(
                            $"Repos[{i}] is empty.");
                        continue;
                    }

                    string[] parts = repo.Split('/');

                    if (parts.Length != 2 ||
                        string.IsNullOrWhiteSpace(parts[0]) ||
                        string.IsNullOrWhiteSpace(parts[1]))
                    {
                        result.Errors.Add(
                            $"Repos[{i}] has invalid format: '{repo}'. Expected owner/name.");
                    }
                }
            }
            if (settings?.TotalTimeoutSeconds < settings?.TimeOutSeconds)
            {
                result.Warnings.Add(
                    "TotalTimeoutSeconds is less than TimeOutSeconds.");
            }

            if (settings?.Repos != null && settings.Repos.Count > 1)
            {
                double totalDelaySeconds =
                    (settings.Repos.Count - 1) *
                    settings.DelayBetweenRequestsMs / 1000.0;

                if (totalDelaySeconds > settings.TotalTimeoutSeconds)
                {
                    result.Warnings.Add(
                        "Delays between repositories exceed TotalTimeoutSeconds. " +
                        "Some repositories may not be checked.");
                }

            }
            return result;
        } 
    }
}
