using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Net;
class Program
{
    static async Task Main()
    {
        using HttpClient Client = new(); //created Client 
        Client.Timeout = TimeSpan.FromSeconds(10); // auto cancellation for client, if respondse took more than 10second canellation will send stop
        Client.DefaultRequestHeaders.Add("User-Agent", "KrustuSHN / 1.0"); //custom headers 

        var github = new GitHubClient(Client); // pass the object inside class GitHubClient 
        // created object github to work and use github methods in Main.

        using CancellationTokenSource AutoMod = // auto cancelltion 
            new CancellationTokenSource(TimeSpan.FromSeconds(60));
        using CancellationTokenSource ManuallyMod = new CancellationTokenSource(); //cancellation manually
        using var cts =
            CancellationTokenSource.CreateLinkedTokenSource(AutoMod.Token, ManuallyMod.Token); // combine two cancellation into one

        List<string> repos = new() // collection with all urls
    {
        "krustu/intermediate-CSpractice",
        "krustu/CSharp-GitPracrice",
        "krustu/RealEstateAggregator",
        "krustu/RPG-manager",
        "krustu/Zabka-Catalog-LINQ"
    };

        _ = Task.Run(() =>
        {
            Console.WriteLine("press any key to Cancel immediately");
            Console.ReadKey(true);
            ManuallyMod.Cancel(); // controle manual cancellation with press latter to stop any time
        });

        try
        {
            for (int i = 0; i < repos.Count; i++) // Our main cycle to repeat as much as we have urls(repo)
            {
                string[] parts = repos[i].Split('/');  // cutting string into several slice
                try
                {
                    //here we use ubject github as client and use his method that we made in clas GIthubClient 

                    string json = await github.GetRepositoryAsync(parts[0], parts[1], cts.Token); // 

                    Console.WriteLine($"{i + 1}. {repos[i]}: OK, {json.Length} chars");
                }
                catch (HttpRequestException ex) // handle mistakes 
                {
                    Console.WriteLine($"{i + 1}. {repos[i]}: {ex.Message}"); // conctuction need only for to see mistakes output
                }

                if (i < repos.Count - 1)
                    await Task.Delay(1000, cts.Token); // our delay 
            }
        }
        catch (OperationCanceledException) //catch to identify which cancellation will send notification about stopping
        {
            if (ManuallyMod.IsCancellationRequested)
                Console.WriteLine("Operation was canceled by Manually");
            else if (AutoMod.IsCancellationRequested)
                Console.WriteLine("Operation was canceled by Timeout");
        }
    }
}
public class GitHubClient
{
    private readonly HttpClient _httpClient; // craete one main client for all and pass it in others parts
    public GitHubClient(HttpClient a)
    {
        _httpClient = a;
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
        int Maxattempts = 4; // out attempts 
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
            TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, attempts - 1));
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