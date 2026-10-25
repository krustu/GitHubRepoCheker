using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Net;
using GitHubRepoCheker.GithubRequest;
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
