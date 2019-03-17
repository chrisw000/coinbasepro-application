# coinbasepro-application
dotnet hosted services to handle data from the coinbasepro-sharp api

<h2>Getting Started</h2>

<i>Generate your key at https://pro.coinbase.com/profile/api</i>

- Navigate to the CoinbasePro.ConsoleExample folder
- Create a new file; appsettings.Development.json *(appsettings.Development.json is excluded from git)*
- Populate your apiKey, apiSecret, passPhrase **keep these secret!** 
- Provide a folder location for the csv output
```json
{
  "csvPath": "c:/csv-data/",
  "apiKey":  "",
  "apiSecret":  "",
  "passPhrase":  ""
}
```
- Populate a /Properties/launchSettings.json file
```json
{
  "profiles": {
    "CoinbasePro.ConsoleExample": {
      "commandName": "Project",
      "environmentVariables": {
        "ENVIRONMENT": "Development",
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

- Run it
```
dotnet run coinbasepro.consoleexample
```

<h2>Output</h2>
CoinbasePro.ConsoleExample will batch up a series of API calls and download 1 years worth of candle data for the following:

- Btc-Usd, Hour 1 candles
- Eth-Usd, Hour 1 candles
- Eth-Eur, Minutes 15 candles

Saving the data into into csv files together with csv meta data to allow it to pickup only new data on subsequent runs.

The data is pulled in batches, so there can be a pause of upto 1 minute between batch runs.
