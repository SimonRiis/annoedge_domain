using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using domain.shared.services;
using domain.algobook;
using domain.exchangebook;
using domain.globalbook;
using LanguageExt;
using static LanguageExt.Prelude;
using domain.book;
using domain.shared;
using BookService = domain.services.book.BookService;

namespace test
{
    [TestClass]
    public class BookServiceTests
    {
        private BookService _bookService;

        [TestInitialize]
        public void Setup()
        {
            _bookService = new BookService();
        }

        [TestMethod]
        public void VerifyBooksForExchange_WithMatchingQuantitys_ReturnsSuccess()
        {
            // Arrange
            var exchangeId = Guid.NewGuid();
            var symbolId = Guid.NewGuid();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductInfo = new ProductInfo { AssetType = "", BaseCurrency = "", LastUpdated = DateTimeOffset.UtcNow, MinOrderQtySize = 1, QuoteCurrency = "", SettlementCurrency = "", ExpirationDate = None, MarginRequirement = None, TickSize = None },
                Symbol = new Symbol { Name = "BTCUSD", Id = symbolId },
            };

            var bookEntry = new BookEntry
            {
                Id = Guid.NewGuid(),
                AverageEntryPrice = 1m,
                LastUpdated = DateTimeOffset.UtcNow,
                Product = product,
                Quantity = 100m
            };

            var book1 = new Book(new BookEntryService(new PriceService(), new QuantityService())) { Id = Guid.Empty, Name = "" };

            book1.AddBookEntry(bookEntry);

            var exchange = new Exchange { Name = "KRAKEN", Id = Guid.NewGuid(), DefaultBookId = Guid.NewGuid() };

            var exchangeBooks = new List<ExchangeBook>
            {
                new ExchangeBook
                {
                    Name = "Kraken book",
                    Exchange = exchange,
                    Id = Guid.NewGuid(),
                    Book = book1
                }
            };

            var algoBooks = new List<AlgoBook>
            {
                new AlgoBook
                {
                    Id = Guid.NewGuid(),
                    Algo = new Algo { Name = "Algo1", Id = Guid.NewGuid(), Exchange = exchange},
                    Book = book1
                }
            };

            // Act
            var result = _bookService.VerifyBooksForExchange(exchangeId, exchangeBooks, algoBooks);

            // Assert
            Assert.IsTrue(result.IsSucc);
        }

        [TestMethod]
        public void VerifyBooksForExchange_WithImbalance_ReturnsError()
        {
            // Arrange
            var exchangeId = Guid.NewGuid();
            var symbolId = Guid.NewGuid();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductInfo = new ProductInfo { AssetType = "", BaseCurrency = "", LastUpdated = DateTimeOffset.UtcNow, MinOrderQtySize = 1, QuoteCurrency = "", SettlementCurrency = "", ExpirationDate = None, MarginRequirement = None, TickSize = None },
                Symbol = new Symbol { Name = "BTCUSD", Id = symbolId },
            };


            var bookEntry = new BookEntry
            {
                Id = Guid.NewGuid(),
                AverageEntryPrice = 1m,
                LastUpdated = DateTimeOffset.UtcNow,
                Product = product,
                Quantity = 100m
            };

            var bookEntry2 = new BookEntry
            {
                Id = Guid.NewGuid(),
                AverageEntryPrice = 1m,
                LastUpdated = DateTimeOffset.UtcNow,
                Product = product,
                Quantity = 50m
            };

            var book1 = new Book(new BookEntryService(new PriceService(), new QuantityService())) { Id = Guid.Empty, Name = "" };

            book1.AddBookEntry(bookEntry);


            var book2 = new Book(new BookEntryService(new PriceService(), new QuantityService())) { Id = Guid.NewGuid(), Name = "" };
            book2.AddBookEntry(bookEntry2);

            var exchange = new Exchange { Name = "KRAKEN", Id = exchangeId, DefaultBookId = Guid.NewGuid() };

            var exchangeBooks = new List<ExchangeBook>
            {
                new ExchangeBook
                {
                    Name = "Kraken book",
                    Exchange = exchange,
                    Id = Guid.NewGuid(),
                    Book = book1
                }
            };

            var algoBooks = new List<AlgoBook>
            {
                 new AlgoBook
                {
                    Id = Guid.NewGuid(),
                    Algo = new Algo { Name = "Algo1", Id = Guid.NewGuid(), Exchange = exchange},
                    Book = book2
                }
            };

            // Act
            var result = _bookService.VerifyBooksForExchange(exchangeId, exchangeBooks, algoBooks);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual($"IMBALANCE for Exchange id: {exchangeId} symbol id: {symbolId}", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void VerifyGlobalBook_WithMatchingQuantitys_ReturnsSuccess()
        {
            // Arrange
            var symbolId = Guid.NewGuid();
            var exchangeId = Guid.NewGuid();
        var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductInfo = new ProductInfo { AssetType = "", BaseCurrency = "", LastUpdated = DateTimeOffset.UtcNow, MinOrderQtySize = 1, QuoteCurrency = "", SettlementCurrency = "", ExpirationDate = None, MarginRequirement = None, TickSize = None },
                Symbol = new Symbol { Name = "BTCUSD", Id = symbolId },
            };

            var bookEntry = new BookEntry
            {
                Id = Guid.NewGuid(),
                AverageEntryPrice = 1m,
                LastUpdated = DateTimeOffset.UtcNow,
                Product = product,
                Quantity = 100m
            };

            var book1 = new Book(new BookEntryService(new PriceService(), new QuantityService())) { Id = Guid.NewGuid(), Name = "" };

            book1.AddBookEntry(bookEntry);


            var globalBook = new GlobalBook
            {
                Id = Guid.NewGuid(),
                Book = book1
            };

            var exchange = new Exchange { Name = "KRAKEN", Id = Guid.NewGuid(), DefaultBookId = Guid.NewGuid() };

            var exchangeBooks = new List<ExchangeBook>
            {
                new ExchangeBook
                {
                    Name = "Kraken book",
                    Exchange = exchange,
                    Id = Guid.NewGuid(),
                    Book = book1
                }
            };
            var algoBooks = new List<AlgoBook>(); // No algoBooks needed for this test

            // Act
            var result = _bookService.VerifyGlobalBook(globalBook, exchangeBooks, algoBooks);

            // Assert
            Assert.IsTrue(result.IsSucc);
        }

        [TestMethod]
        public void VerifyGlobalBook_WithImbalance_ReturnsError()
        {
            // Arrange
            var symbolId = Guid.NewGuid();
            var exchangeId = Guid.NewGuid();
        var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductInfo = new ProductInfo { AssetType = "", BaseCurrency = "", LastUpdated = DateTimeOffset.UtcNow, MinOrderQtySize = 1, QuoteCurrency = "", SettlementCurrency = "", ExpirationDate = None, MarginRequirement = None, TickSize = None },
                Symbol = new Symbol { Name = "BTCUSD", Id = symbolId },
            };

            var bookEntry = new BookEntry
            {
                Id = Guid.NewGuid(),
                AverageEntryPrice = 1m,
                LastUpdated = DateTimeOffset.UtcNow,
                   Product = product,
                Quantity = 100m
            };

            var book1 = new Book(new BookEntryService(new PriceService(), new QuantityService())) {Id = Guid.Empty, Name = "" };

            book1.AddBookEntry(bookEntry);

            var bookEntry2 = new BookEntry
            {
                Id = Guid.NewGuid(),
                AverageEntryPrice = 1m,
                LastUpdated = DateTimeOffset.UtcNow,
                 Product = product,
                Quantity = 50m
            };

            var book2 = new Book(new BookEntryService(new PriceService(), new QuantityService())) {Id = Guid.NewGuid(), Name = "" };
            book2.AddBookEntry(bookEntry2);


            var globalBook = new GlobalBook
            {
                Id = Guid.NewGuid(),
                Book = book1
            };

            var exchange = new Exchange { Name = "KRAKEN", Id = exchangeId, DefaultBookId = Guid.NewGuid() };

            var exchangeBooks = new List<ExchangeBook>
            {
                new ExchangeBook
                {
                    Name = "Kraken book",
                    Exchange = exchange,
                    Id = Guid.NewGuid(),
                    Book = book2
                }
            };

            var algoBooks = new List<AlgoBook>(); // No algoBooks needed for this test

            // Act
            var result = _bookService.VerifyGlobalBook(globalBook, exchangeBooks, algoBooks);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual($"IMBALANCE for symbol id: {symbolId}", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }
    }
}
