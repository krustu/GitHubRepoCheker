
using System;
using System.Collections.Generic;

namespace GitHubRepoCheker.ClassJsonSettnigs
{
    public class ValidationHandler
    {
        private readonly ValidationResults _result;

        public ValidationHandler(ValidationResults result)
        {
            _result = result;
        }

        public GitHubSettings? ValidCompatibility(GitHubSettings settings)
        {
            bool canWork = HandleValidation();

            if (canWork)
                return settings;
            else
                return null;
        }

        private bool HandleValidation()
        {
            // Critical errors: do not allow the program to continue.
            if (_result.Errors.Count > 0)
            {
                _result.ShowErrors();
                Console.WriteLine("Configuration contains errors. Program cannot continue.");
                Console.ReadKey();
                return false;
            }
            // No errors and no warnings.
            else if (_result.Warnings.Count == 0)
            {
                return true;
            }
            // Warnings only: the user decides whether to continue.
            else
            {
                _result.ShowWarnings();

                Console.WriteLine("Warnings were detected in the configuration. Do you want to continue?");
                Console.WriteLine("Yes - 2 ; No - 1");

                bool answer = InputChecker();

                if (answer == true)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Program cancelled by user.");
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
                Console.Write("Enter 1 or 2: ");

                if (int.TryParse(Console.ReadLine(), out result)
                    && (result == 1 || result == 2))
                {
                    break;
                }

                Console.WriteLine("Invalid input. Write 1 or 2.");
            }

            if (result == 2)
                return true;
            else
                return false;
        }
    }

    public class ValidationResults
    {
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();

        public void ShowWarnings()
        {
            int index = 0;

            foreach (var warning in Warnings)
            {
                Console.WriteLine($"{index} - WARNING: {warning}");
                index++;
            }
        }

        public void ShowErrors()
        {
            int index = 0;

            foreach (var error in Errors)
            {
                Console.WriteLine($"{index} - ERROR: {error}");
                index++;
            }
        }
    }

    public static class SettingsValidator
    {
        public static ValidationResults SettingsMistakes(GitHubSettings settings)
        {
            ValidationResults result = new();

            if (settings == null)
            {
                result.Errors.Add("Settings object is null.");
                return result;
            }

            // Errors

            if (settings.TimeOutSeconds < 1)
                result.Errors.Add("TimeOutSeconds must be at least 1.");

         

            if (settings.DelayBetweenRequestsMs < 0)
                result.Errors.Add("DelayBetweenRequestsMs cannot be negative.");

            if (settings.Repos == null || settings.Repos.Count == 0)
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
                        result.Errors.Add($"Repos[{i}] is empty.");
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

            // Warnings
              if (settings.MaxAttempts < 1)
                  result.Warnings.Add("MaxAttempts must be at least 1.");

            if (settings.TotalTimeoutSeconds < settings.TimeOutSeconds)
            {
                result.Warnings.Add(
                    "TotalTimeoutSeconds is less than TimeOutSeconds.");
            }

            if (settings.Repos != null && settings.Repos.Count > 1)
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
