using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using domain.shared.services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace test
{
    [TestClass]
    public class QuantityServiceTests
    {
        private QuantityService _QuantityService;

        [TestInitialize]
        public void Setup()
        {
            _QuantityService = new QuantityService();
        }

        // Tests for DecreaseQuantity method

        [TestMethod]
        public void DecreaseQuantity_WithValidInput_ReturnsNewQuantity()
        {
            // Arrange
            decimal currentQuantity = 100m;
            decimal QuantityReduction = 30m;

            // Act
            var result = _QuantityService.DecreaseQuantity(currentQuantity, QuantityReduction);

            // Assert
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(70m, result.IfFail(0m));
        }

        [TestMethod]
        public void DecreaseQuantity_WithZeroQuantityReduction_ReturnsError()
        {
            // Arrange
            decimal currentQuantity = 100m;
            decimal QuantityReduction = 0m; // Invalid Quantity reduction

            // Act
            var result = _QuantityService.DecreaseQuantity(currentQuantity, QuantityReduction);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Decrease Quantity cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void DecreaseQuantity_WithQuantityReductionExceedingCurrentQuantity_ReturnsError()
        {
            // Arrange
            decimal currentQuantity = 100m;
            decimal QuantityReduction = 150m; // Exceeds current Quantity

            // Act
            var result = _QuantityService.DecreaseQuantity(currentQuantity, QuantityReduction);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Decrease Quantity cannot exceed current position Quantity.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void DecreaseQuantity_WithExactQuantityReduction_ReturnsZero()
        {
            // Arrange
            decimal currentQuantity = 100m;
            decimal QuantityReduction = 100m; // Matches current Quantity exactly

            // Act
            var result = _QuantityService.DecreaseQuantity(currentQuantity, QuantityReduction);

            // Assert
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(0m, result.IfFail(0m));
        }

        // Tests for IncreaseQuantity method

        [TestMethod]
        public void IncreaseQuantity_WithValidInput_ReturnsNewQuantity()
        {
            // Arrange
            decimal currentQuantity = 100m;
            decimal additionalQuantity = 50m;

            // Act
            var result = _QuantityService.IncreaseQuantity(currentQuantity, additionalQuantity);

            // Assert
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(150m, result.IfFail(0m));
        }

        [TestMethod]
        public void IncreaseQuantity_WithZeroAdditionalQuantity_ReturnsError()
        {
            // Arrange
            decimal currentQuantity = 100m;
            decimal additionalQuantity = 0m; // Invalid additional Quantity

            // Act
            var result = _QuantityService.IncreaseQuantity(currentQuantity, additionalQuantity);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Additional Quantity cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void IncreaseQuantity_WithNegativeAdditionalQuantity_ReturnsError()
        {
            // Arrange
            decimal currentQuantity = 100m;
            decimal additionalQuantity = -50m; // Invalid negative additional Quantity

            // Act
            var result = _QuantityService.IncreaseQuantity(currentQuantity, additionalQuantity);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Additional Quantity cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }
    }
}
