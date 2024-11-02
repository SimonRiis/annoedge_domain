using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using domain.shared.services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace test
{
    [TestClass]
    public class VolumeServiceTests
    {
        private VolumeService _volumeService;

        [TestInitialize]
        public void Setup()
        {
            _volumeService = new VolumeService();
        }

        // Tests for DecreaseVolume method

        [TestMethod]
        public void DecreaseVolume_WithValidInput_ReturnsNewVolume()
        {
            // Arrange
            decimal currentVolume = 100m;
            decimal volumeReduction = 30m;

            // Act
            var result = _volumeService.DecreaseVolume(currentVolume, volumeReduction);

            // Assert
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(70m, result.IfFail(0m));
        }

        [TestMethod]
        public void DecreaseVolume_WithZeroVolumeReduction_ReturnsError()
        {
            // Arrange
            decimal currentVolume = 100m;
            decimal volumeReduction = 0m; // Invalid volume reduction

            // Act
            var result = _volumeService.DecreaseVolume(currentVolume, volumeReduction);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Decrease volume cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void DecreaseVolume_WithVolumeReductionExceedingCurrentVolume_ReturnsError()
        {
            // Arrange
            decimal currentVolume = 100m;
            decimal volumeReduction = 150m; // Exceeds current volume

            // Act
            var result = _volumeService.DecreaseVolume(currentVolume, volumeReduction);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Decrease volume cannot exceed current position volume.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void DecreaseVolume_WithExactVolumeReduction_ReturnsZero()
        {
            // Arrange
            decimal currentVolume = 100m;
            decimal volumeReduction = 100m; // Matches current volume exactly

            // Act
            var result = _volumeService.DecreaseVolume(currentVolume, volumeReduction);

            // Assert
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(0m, result.IfFail(0m));
        }

        // Tests for IncreaseVolume method

        [TestMethod]
        public void IncreaseVolume_WithValidInput_ReturnsNewVolume()
        {
            // Arrange
            decimal currentVolume = 100m;
            decimal additionalVolume = 50m;

            // Act
            var result = _volumeService.IncreaseVolume(currentVolume, additionalVolume);

            // Assert
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(150m, result.IfFail(0m));
        }

        [TestMethod]
        public void IncreaseVolume_WithZeroAdditionalVolume_ReturnsError()
        {
            // Arrange
            decimal currentVolume = 100m;
            decimal additionalVolume = 0m; // Invalid additional volume

            // Act
            var result = _volumeService.IncreaseVolume(currentVolume, additionalVolume);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Additional volume cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void IncreaseVolume_WithNegativeAdditionalVolume_ReturnsError()
        {
            // Arrange
            decimal currentVolume = 100m;
            decimal additionalVolume = -50m; // Invalid negative additional volume

            // Act
            var result = _volumeService.IncreaseVolume(currentVolume, additionalVolume);

            // Assert
            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Additional volume cannot be zero.", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }
    }
}
