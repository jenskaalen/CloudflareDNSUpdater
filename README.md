# CloudflareDNSUpdater
Updates the A record of your chosen domain with your current public IP


### Setup
A file named cloudflareConfig.json must be added to the directory, with the following values:
```
{
  "ZoneId": "xxxxxxxxxxxx",
  "APIToken": "xxxxxxxxxxxx",
  "DNSRecordId": "xxxxxxxxxxxx",
  "DomainName": "YourDomain.com"
}
```

ZoneId can be found via the dashboard or API. DNS record ID must be fetched via the API. 
Token with edit permissions for DNS must be created.


Why make this? My IP changes frequently🤷‍♂️
