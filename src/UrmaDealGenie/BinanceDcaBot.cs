using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using XCommas.Net.Objects;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using Binance.Net.Enums;
using Microsoft.Extensions.Logging;
using CryptoExchange.Net.Logging;
using Binance.Net.Objects.Spot.SpotData;

namespace UrmaDealGenie
{
  public class DcaBotConfig
  {
    public DcaBotConfig()
    {
      this.Id = Guid.NewGuid().ToString();
    }
    public string Id { get; }
    public string Name { get; set; }
    public string Pair { get; set; }
    public decimal BuyOrder { get; set; }
    public decimal SafetyOrder { get; set; }
    public decimal DeviationPercentage { get; set; }
    public int MaxSafetyOrders { get; set; }
    public decimal VolumeScaling { get; set; }
    public decimal DeviationScaling { get; set; }
    public decimal TakeProfitPercentage { get; set; }
    public bool StartASAP { get; set; }
  }
  /// <summary>
  ///  The main AWS Lambda Function class and handler
  /// </summary>
  public class BinanceDcaBot
  {
    private BinanceClient client;
    public BinanceDcaBot()
    {
      // Binance TestNet API key
      var apiKey = "YOUR_API_KEY_HERE";
      var secret = "YOUR_3COMMAS_SECRET_HERE";
      client = new BinanceClient(new BinanceClientOptions()
      {
        ApiCredentials = new ApiCredentials(apiKey, secret),
        BaseAddress = BinanceApiAddresses.TestNet.RestClientAddress,
        LogLevel = LogLevel.Debug,
        LogWriters = new List<ILogger> { new ConsoleLogger() }
      });
    }

    public async Task Go()
    {
      // TODO store state in DB, so that if all orders are deleted in Binance, they can be recreated (not MVP)
      // loop through each bot deal that should be running
      DcaBotConfig bot = GetBot();

      var openOrders = GetOpenOrders().Result;

      // TODO each buy and sell order has a unique ID in binance, Used for matching
      var dealId = $"{bot.Name}-{bot.Pair}-0";
      
      // check bots to see if any new new deals need creating - open new BO
      var deal = openOrders.FirstOrDefault(x => x.ClientOrderId == dealId);
      if (deal == null)
      {
        await StartNewDeal(bot);

        openOrders = GetOpenOrders().Result;

        // check deals to see if any new SOs need creating
        // if SO has gone (can it be checked to see if SO completed, or callback?) then 
        // place SO buy vol x VolumeScaling @ current price - (current price x DeviationPercentage / 100) 
        // cancel TP sell
        // place TP sell vol @ current price + (current price x TakeProfitPercentage / 100)

        // check if deal has sold all volume
        // cancel TP sell
      }
      else
      {
        // edit deal
      }

    }

    private DcaBotConfig GetBot()
    {
      return new DcaBotConfig()
      {
        Name = "Urma",
        Pair = "BTCUSDT",
        BuyOrder = 0.001m,
        SafetyOrder = 0.001m,
        DeviationPercentage = 1.87m,
        MaxSafetyOrders = 7,
        VolumeScaling = 1.4m,
        DeviationScaling = 1.5m,
        TakeProfitPercentage = 1.5m,
        StartASAP = true,
      };      
    }

    private string MakeDealId(DcaBotConfig bot, OrderSide orderSide, int currentSafetyOrder = 0)
    {
      return $"{bot.Name}-{bot.Pair}-{orderSide}-{currentSafetyOrder}";
    }
    private async Task StartNewDeal(DcaBotConfig bot)
    {
      // place initial Base Order with specified volume at current price
      var callResult = await client.Spot.Order.PlaceOrderAsync(bot.Pair, OrderSide.Buy, OrderType.Market, 
        quantity: bot.BuyOrder, 
        newClientOrderId: MakeDealId(bot, OrderSide.Buy));
        // TODO: Prefer limit orders, but need current price and handle maker=taker rejections
       
      // place Take Profit sell order with specified vol at Take Profit price
      var currentPrice = GetCurrentPrice(bot.Pair).Result;
      var takeProfitPrice = Decimal.Round(currentPrice + (currentPrice * bot.TakeProfitPercentage / 100), 2);
      callResult = await client.Spot.Order.PlaceOrderAsync(bot.Pair, OrderSide.Sell, OrderType.LimitMaker, 
        quantity: bot.BuyOrder, 
        price: takeProfitPrice,
        newClientOrderId: MakeDealId(bot, OrderSide.Sell));

      // place SO buy vol x VolumeScaling @ current price - (current price x DeviationPercentage / 100)
      // SIMPLE WORKAROUND - place all SOs up front, i.e. 1 to MaxSafetyOrders

    }

    private async Task<decimal> GetCurrentPrice(string pair)
    {
      var callResult = await client.Spot.Market.GetPriceAsync(pair);
      return callResult.Data.Price;
    }

    private async Task<IEnumerable<BinanceOrder>> GetOpenOrders()
    {
      var callResult = await client.Spot.Order.GetOpenOrdersAsync();
      if (callResult.Success)
      {
        foreach (BinanceOrder order in callResult.Data)
        {
          Console.WriteLine($"{order.Symbol} - {order.Side} {order.Type} order, price {order.Price}, vol {order.Quantity}");
        }
      }
      else
      {
        // error
      }
      return callResult.Data;
    }
  }
}