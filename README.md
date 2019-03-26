# coinbasepro-application
dotnet hosted services to handle data from the coinbasepro-sharp api

[![Build status](https://ci.appveyor.com/api/projects/status/n062lhwlltgthrt4?svg=true)](https://ci.appveyor.com/project/chrisw000/coinbasepro-application)

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

The currency and candle periods are defined in the example `ConsoleHost.cs` file
```
// Setup the markets to pull data for
services.AddTransient<ICandleMonitorFeedProvider>(sp => new CsvCandleMonitorFeed(new List<CandleMonitorFeeds>()
    {
        // There is a bug in GDAX.Api.ClientLibrary that causes endless loop calling REST service
        // when the amount of data on GDAX is less than what is trying to be pulled
        // I've submitted a buxfix - which will be in the 1.0.28 Nuget version
        // for now just pull currencies with at least a year of data
        new CandleMonitorFeeds(ProductType.BtcUsd, CandleGranularity.Hour1),
        new CandleMonitorFeeds(ProductType.EthUsd, CandleGranularity.Hour1),
        new CandleMonitorFeeds(ProductType.EthEur, CandleGranularity.Minutes15)
    })
```

<h2>RabbitMq</h2>
More docs to follow, check out the example RabbitMq producer and consumer examples.

Requires running a RabbitMq server, configuring user/password and endpoints for both the producer and consumer.
Then TimeSeries / candle data can be transmitted across RabbitMq

TODO: 
More docs, and how to register the ICandleProducer, ICandleConsumer implementations into the `ConsoleExample` app.
