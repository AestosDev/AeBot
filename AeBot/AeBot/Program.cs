using System.Text.RegularExpressions;

class Program
{
    // Konfigurationen direkt im Code
    static string WebhookUrl = "";
    static string Language = "DE"; // Oder "DE"

    static async Task Main(string[] args)
    {
        // Startnachricht an Discord senden
        await SendDiscordMessage("`AeBot has started.`");

        // Status regelmäßig prüfen
        string previousStatus = "";
        while (true)
        {
            string currentStatus = await CheckServerStatus();

            if (currentStatus != previousStatus)
            {
                await SendDiscordMessage(currentStatus); 
                previousStatus = currentStatus;
            }

            await Task.Delay(5 * 60 * 1000);
        }
    }

    static async Task<string> CheckServerStatus()
    {
        try
        {
            var client = new HttpClient();
            string url = "https://status.epicgames.com";
            string htmlContent = await client.GetStringAsync(url);

            string pattern = @"<span class=""status-indicator"">(.*?)</span>";
            var match = Regex.Match(htmlContent, pattern);

            if (match.Success)
            {
                string statusText = match.Groups[1].Value;

                if (statusText.Contains("Under Maintenance", StringComparison.OrdinalIgnoreCase))
                {
                    return LanguageManager.GetString(Language, "ServerOffline");
                }
                else if (statusText.Contains("Online", StringComparison.OrdinalIgnoreCase))
                {
                    return LanguageManager.GetString(Language, "ServerOnline"); 
                }
            }

            return LanguageManager.GetString(Language, "ServerOffline");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error checking server status: " + ex.Message);
            return LanguageManager.GetString(Language, "ServerOffline"); 
        }
    }

    static async Task SendDiscordMessage(string message)
    {
        try
        {
            var json = "{ \"content\": \"" + message + "\" }";
            using (var client = new HttpClient())
            {
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(WebhookUrl, content);

                // Überprüfe den Antwortstatus
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Message successfully sent to Discord.");
                }
                else
                {
                    Console.WriteLine("Error sending message to Discord. Status Code: " + response.StatusCode);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response body: " + responseBody); 
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message to Discord: " + ex.Message);
        }
    }
}

public static class LanguageManager
{
    private static readonly Dictionary<string, Dictionary<string, string>> LanguageStrings = new()
    {
        { "EN", new Dictionary<string, string>
            {
                { "ServerOnline", "Server is Online" },
                { "ServerOffline", "-# **[SERVER STATUS:](https://.com)** `OFFLINE / UNDER MAINTENANCE`" },
            }
        },
        { "DE", new Dictionary<string, string>
            {
                { "ServerOnline", "-# **[SERVER STATUS:](https://.com)** `ONLINE`" },
                { "ServerOffline", "-# **[SERVER STATUS:](https://.com)** `OFFLINE / WARTUNGSARBEITEN`" },
            }
        }
    };

    public static string GetString(string language, string key)
    {
        if (LanguageStrings.ContainsKey(language) && LanguageStrings[language].ContainsKey(key))
        {
            return LanguageStrings[language][key];
        }
        return "Translation not found"; 
    }
}
