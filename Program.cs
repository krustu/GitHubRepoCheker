using GitHubRepoCheker.ClassJsonSettnigs;
using GitHubRepoCheker.GithubRequest;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;

class Program
{
    static async Task Main()
    {
        Environment.SetEnvironmentVariable("GitHubSettings__MaxAttempts", "2");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        
        GitHubSettings Presettings = config
            .GetSection("GitHubSettings")
            .Get<GitHubSettings>() 
            ?? throw new InvalidOperationException("Section 'GitHubSettings' is missing");


     

        // Validation System before running program


        ValidationResults result = SettingsValidator.SettingsMistakes(Presettings);
        ValidationHandler handler = new(result);
        var settings = handler.ValidCompatibility(Presettings);
        if (settings == null) return;

        

        //created Client 
        using HttpClient Client = new(); 
        Client.Timeout = TimeSpan.FromSeconds(settings.TimeOutSeconds); // auto cancellation for client, if respondse took more than 10second canellation will send stop
        Client.DefaultRequestHeaders.Add("User-Agent",$"{settings.UserAgent.Name} / {settings.UserAgent.Version}"); //custom headers 

        var github = new GitHubClient(Client , settings); // pass the object inside class GitHubClient 
        // created object github to work and use github methods in Main.


        using CancellationTokenSource AutoMod = // auto cancelltion 
            new CancellationTokenSource(TimeSpan.FromSeconds(settings.TotalTimeoutSeconds));
        using CancellationTokenSource ManuallyMod = new CancellationTokenSource(); //cancellation manually
        using var cts =
            CancellationTokenSource.CreateLinkedTokenSource(AutoMod.Token, ManuallyMod.Token); // combine two cancellation into one


        //Check token
        if (!string.IsNullOrWhiteSpace(settings.Token))
        {
            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", settings.Token);
            Console.WriteLine("token found");
        }
        else
        {
            Console.WriteLine("No token found");
        }
        Console.ReadKey();
        _ = Task.Run(() =>
        {
            Console.WriteLine("press any key to Cancel immediately");
            Console.ReadKey(true);
            ManuallyMod.Cancel(); // controle manual cancellation with press latter to stop any time
        });


         

      //  Console.WriteLine(JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        var repos = settings.Repos;

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
                    await Task.Delay(settings.DelayBetweenRequestsMs, cts.Token); // our delay 
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
