using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using domain.shared.services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace tests
{
    [TestClass]
    public class PriceServiceTests
    {
        private PriceService _priceService;

        [TestInitialize]
        public void Setup()
        {
            _priceService = new PriceService();
        }

        [TestMethod]
        public void GetNewAverageEntryPrice_WithValidInput_ReturnsCorrectAverage()
        {
            // Arrange
            decimal oldQuantity = 100m;
            decimal additionalQuantity = 50m;
            decimal oldAverageEntryPrice = 10m;
            decimal newEntryPrice = 15m;

            // Act
            var result = _priceService.GetNewAverageEntryPrice(oldQuantity, additionalQuantity, oldAverageEntryPrice, newEntryPrice);

            // Assert
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(11.67m, Math.Round(result.IfFail(0m), 2));
        }

        [TestMethod]
        public void GetNewAverageEntryPrice_WithZeroAdditionalQuantity_ReturnsError()
        {
            // Arrange
            decimal oldQuantity = 100m;
            decimal additionalQuantity = 0m; // Invalid additional Quantity
            decimal oldAverageEntryPrice = 10m;
            decimal newEntryPrice = 15m;

            // Act
            var result = _priceService.GetNewAverageEntryPrice(oldQuantity, additionalQuantity, oldAverageEntryPrice, newEntryPrice);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Additional Quantity cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void GetNewAverageEntryPrice_WithZeroNewEntryPrice_ReturnsError()
        {
            // Arrange
            decimal oldQuantity = 100m;
            decimal additionalQuantity = 50m;
            decimal oldAverageEntryPrice = 10m;
            decimal newEntryPrice = 0m; // Invalid new entry price

            // Act
            var result = _priceService.GetNewAverageEntryPrice(oldQuantity, additionalQuantity, oldAverageEntryPrice, newEntryPrice);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("New EntryPrice cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void GetNewAverageEntryPrice_WithNegativeAdditionalQuantity_ReturnsError()
        {
            // Arrange
            decimal oldQuantity = 100m;
            decimal additionalQuantity = -50m; // Invalid negative additional Quantity
            decimal oldAverageEntryPrice = 10m;
            decimal newEntryPrice = 15m;

            // Act
            var result = _priceService.GetNewAverageEntryPrice(oldQuantity, additionalQuantity, oldAverageEntryPrice, newEntryPrice);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Additional Quantity cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void GetNewAverageEntryPrice_WithNegativeNewEntryPrice_ReturnsError()
        {
            // Arrange
            decimal oldQuantity = 100m;
            decimal additionalQuantity = 50m;
            decimal oldAverageEntryPrice = 10m;
            decimal newEntryPrice = -15m; // Invalid negative new entry price

            // Act
            var result = _priceService.GetNewAverageEntryPrice(oldQuantity, additionalQuantity, oldAverageEntryPrice, newEntryPrice);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("New EntryPrice cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void GetNewAverageEntryPrice_WithZeroOldQuantity_ReturnsCorrectAverage()
        {
            // Arrange
            decimal oldQuantity = 0m; // Zero old Quantity
            decimal additionalQuantity = 50m;
            decimal oldAverageEntryPrice = 10m; // Should not affect result
            decimal newEntryPrice = 15m;

            // Act
            var result = _priceService.GetNewAverageEntryPrice(oldQuantity, additionalQuantity, oldAverageEntryPrice, newEntryPrice);

            // Assert
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(15m, result.IfFail(0m)); // Should be equal to newEntryPrice
        }
    }
}

