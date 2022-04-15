// See https://aka.ms/new-console-template for more information
using NLog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

string logFile = "log.txt";
string configFile = "cloudflareConfig.json";
bool proxied = true;
ILogger logger = SetupLogging(logFile);
string publicIP = GetPublicIP().Result;

Config config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile));

if (config == null)
    throw new Exception($"Config in {configFile} could not be loaded");

logger.Info($"Running PUT request to update the Cloudflare DNS record for {config.DomainName}");
Task<HttpResponseMessage> updateResult = UpdateDNS(config.APIToken, config.ZoneId, config.DNSRecordId, config.DomainName, publicIP, proxied);
logger.Info(updateResult.Result);

ILogger SetupLogging(string logFile)
{
    var config = new NLog.Config.LoggingConfiguration();

    // Targets where to log to: File and Console
    var logTarget = new NLog.Targets.FileTarget("logfile") { FileName = logFile }; 
    config.AddRule(LogLevel.Debug, LogLevel.Error, logTarget);

    // Apply config           
    NLog.LogManager.Configuration = config;
    return NLog.LogManager.Setup().GetCurrentClassLogger();
}

async Task<HttpResponseMessage> UpdateDNS(string apiToken, string zoneId, string dnsRecordId, string domainName, string publicIP, bool proxied)
{
    string apiUrl = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{dnsRecordId}";
    var updateRequest = new { type = "A", name = domainName, content = publicIP,ttl = 1, proxied = proxied};
    string jsonBody = JsonSerializer.Serialize(updateRequest);
    var contentData = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiToken);
        client.DefaultRequestHeaders
             .Accept
             .Add(new MediaTypeWithQualityHeaderValue("application/json"));


        var response = await client.PutAsync(apiUrl, contentData);
        return response;
    }
}

async Task<string> GetPublicIP()
{
    //NOTE: this is just an open api which return your public IP
    string apiUrl = "https://api.ipify.org";

    using (var client = new HttpClient())
    {
        var response = await client.GetStringAsync(apiUrl);

        if (response == null)
        {
            throw new Exception($"Could not get public IP from API {apiUrl}");
        }

        return response;
    }
}

AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
{
    logger.Error(eventArgs.Exception.ToString());
};
