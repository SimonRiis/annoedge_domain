using System;
using Microsoft.VisualBasic;
using domain.order;
using domain.shared.enums;
using domain.product;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using domain.shared;
namespace test.order;

[TestClass]
public class OrderTests
{

// [TestInitialize]
//         public void Setup()
//         {
//             _bookService = new BookService();
//         }

        [TestMethod]
        public void VerifyTradeAdded()
        {
            var trade = new Trade()
            {
                Direction = Direction.LONG,
                Id = "1",
                ExternalId = "1",
                OrderId = "orderid",
                Quantity = 100m,
                Price = 10m,
                TotalPriceInUSD = 1000m,
                ExchangeTimestamp = DateTimeOffset.UtcNow
            };

            var order = new Order(new Product{ Id = Guid.NewGuid(), Symbol = new Symbol{ Id = Guid.NewGuid(), Name = "BTCUSD" }, 
                ProductInfo = new ProductInfo{ AssetType = "Crypto", BaseCurrency = "BTC", 
                QuoteCurrency = "USD", SettlementCurrency = "USD", TradePrecision = 1, 
                TickSize = 0, MarginRequirement = 0, ExpirationDate = None, LastUpdated = DateTimeOffset.UtcNow }},
                new domain.exchange.Exchange{ Id = Guid.NewGuid(), Name = "KRAKEN", DefaultBookId = Guid.NewGuid() },
                "orderid",
                "1",
                Status.PENDING,
                100m,
                10m,
                Direction.LONG,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow);

            order.AddTrade(trade);

            Assert.AreEqual(1, order.Trades.Count);
            Assert.AreEqual(trade, order.Trades.First());
        }

        [TestMethod]
        public void VerifyFillAddedStatusChangePartiallyFilled()
        {
            var trade = new Trade()
            {
                Direction = Direction.LONG,
                Id = "1",
                ExternalId = "1",
                OrderId = "orderid",
                Quantity = 100m,
                Price = 10m,
                TotalPriceInUSD = 1000m,
                ExchangeTimestamp = DateTimeOffset.UtcNow
            };

            var order = new Order(new Product{ Id = Guid.NewGuid(), Symbol = new Symbol{ Id = Guid.NewGuid(), Name = "BTCUSD" }, 
                ProductInfo = new ProductInfo{ AssetType = "Crypto", BaseCurrency = "BTC", 
                QuoteCurrency = "USD", SettlementCurrency = "USD", TradePrecision = 1, 
                TickSize = 0, MarginRequirement = 0, ExpirationDate = None, LastUpdated = DateTimeOffset.UtcNow }},
                new domain.exchange.Exchange{ Id = Guid.NewGuid(), Name = "KRAKEN", DefaultBookId = Guid.NewGuid() },
                "orderid",
                "1",
                Status.PENDING,
                100m,
                10m,
                Direction.LONG,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow);

            order.AddTrade(trade);

            Assert.AreEqual(1, order.Trades.Count);
            Assert.AreEqual(trade, order.Trades.First());
        }
        

}
