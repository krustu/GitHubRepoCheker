using GitHubRepoCheker.ClassJsonSettnigs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GitHubRepoCheker.GithubRequest
{
    public class GitHubClient
    {
        private readonly HttpClient _httpClient; // craete one main client for all and pass it in others parts
        private readonly GitHubSettings _settings;
        public GitHubClient(HttpClient a , GitHubSettings b)
        {
            _httpClient = a;
            _settings = b;
        }
        public async Task<string> GetRepositoryAsync(string owner, string repo, CancellationToken ct)
        {
            using HttpResponseMessage response =
                await GetWithRetryAsync($"https://api.github.com/repos/{owner}/{repo}", ct);

            if (response.IsSuccessStatusCode)
            {
                Task<string> Asnw = response.Content.ReadAsStringAsync(ct); // need only for write json.Length

                return await Asnw; //return string because  we used await 
            }
            else
            {
                throw new HttpRequestException(DescribeError(response.StatusCode, repo));
            }

        }
        private async Task<HttpResponseMessage> GetWithRetryAsync(string url, CancellationToken ct)
        {
            int Maxattempts = _settings.MaxAttempts; // out attempts 
            for (int attempts = 1; ; attempts++)
            {
                try
                {
                    HttpResponseMessage response =
                        await _httpClient.GetAsync(url, ct);

                    if (!IsTransient(response) || attempts == Maxattempts)
                    {
                        return response;
                    }


                    Console.WriteLine($"attempts {attempts}: {(int)response.StatusCode}, repeat");
                    response.Dispose();
                }
                catch (HttpRequestException) when (attempts < Maxattempts)
                {
                    Console.WriteLine($"attempts {attempts}: response failed, repeat");

                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested && attempts < Maxattempts)
                {
                    Console.WriteLine($"attempts {attempts}: timeout, repeat ");
                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested)
                {
                    throw new HttpRequestException("Request timed out after all attempts");
                }
                TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(_settings.BackoffBase, attempts - 1));
                await Task.Delay(delay, ct);
            }
        }
        private static bool IsTransient(HttpResponseMessage response) // second small part of method GEtWithRetryAsync 
        {
            var answer = (int)response.StatusCode;
            if (answer >= 500)
                return true;
            if (answer == 408 || answer == 429)
                return true;
            else
            {
                return false;
            }

        }
        private static string DescribeError(HttpStatusCode code, string repoName) // whti method need only to see exaclty the problem and use only inside this class as second 
        {
            switch (code)
            {
                case HttpStatusCode.NotFound:
                    return $"repository {repoName} not found!";
                case HttpStatusCode.Forbidden:
                    return "access denied or Github Api rate limit exceed. ";
                case HttpStatusCode.RequestTimeout:
                    return "The Request timed out.";
                case HttpStatusCode.TooManyRequests:
                    return "Too many requests. Try again later";

                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.GatewayTimeout:
                    return "GitHub server is temporarily unavailable.";

                default:
                    if ((int)code >= 500)
                        return "GitHub server error.";
                    return $"HTTP error {(int)code}({code}).";

            }

        }
    }
}
