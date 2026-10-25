using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GitHubRepoCheker.ClassJsonSettnigs
{
    public static class SettingsValidator
    {
        public static List<string> CheckCompatibility(GitHubSettings settings)
        {
            List<string> errors = new();
            
            try
            {
                if (settings.TimeOutSeconds < 10)
                    throw new Exception("TimeOutSeconds - cannot be less than 10 second");

                else if (settings.UserAgent.Name == null || settings.UserAgent.Name == "")
                    throw new ArgumentNullException("UserAgent is empty !");
                else if (settings.UserAgent.Version == null || settings.UserAgent.Version == "")
                    throw new ArgumentNullException("Version is empty !");

                else if (settings.TotalTimeoutSeconds > 50)
                    throw new Exception("TotalTimeoutSeconds should start with 50s and more");

                else if (settings.MaxAttempts != 4)
                    throw new Exception("MaxAttempts only 4 second");

                else if (settings.DelayBetweenRequestsMs > 1000)
                    throw new Exception("DelayBetweenRequestsMs cannot be less than 1000ms (1sec)");

                else if (settings.BackoffBase <= 1)
                    throw new Exception("BackoffBase cannot be less than 1");

                else if (settings.Repos.Count < 2)
                    throw new Exception("You have less then 2 repos");
                for(int a = 0;a < settings.Repos.Count; a++)
                {
                        try
                        {
                            if (settings.Repos[a] == null || settings.Repos[a] == "")
                                throw new Exception($"Problem with: {a} - repository");
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex.Message);
                            continue;
                        }
          
                }

            
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            catch (OverflowException ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errors.Add(ex.Message);
            }
            return errors;
        }
    }
}
/*using System;
using System.Collections.Generic;

namespace GitHubRepoCheker.ClassJsonSettnigs
{
    public class ValidationResult
    {
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();

        public bool IsValid => Errors.Count == 0;
    }

    public static class SettingsValidator
    {
        public static ValidationResult CheckCompatibility(GitHubSettings settings)
        {
            ValidationResult result = new();

            if (settings == null)
            {
                result.Errors.Add("Settings object is null.");
                return result;
            }

            // Errors

            if (settings.TimeOutSeconds < 1)
                result.Errors.Add("TimeOutSeconds must be at least 1.");

            if (settings.MaxAttempts < 1)
                result.Errors.Add("MaxAttempts must be at least 1.");

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

            // Warnings

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
}*/