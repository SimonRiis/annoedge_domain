using domain.order;
using domain.position;
using domain.services.position;
using domain.shared.enums;
using domain.shared;
using domain.shared.services;
using static LanguageExt.Prelude;


namespace test
{
    [TestClass]
    public class PositionServiceTests
    {
        private PositionService _positionService; // Assume this is the class that contains UpdatePosition
        private Position _samplePosition;

        [TestInitialize]
        public void SetUp()
        {
            _positionService = new PositionService(new PositionLegService(new PriceService(), new QuantityService()));
            _samplePosition = new Position(Guid.NewGuid(), PositionStatus.PENDING, DateTimeOffset.UtcNow, 
                new PositionMandate { Id = Guid.NewGuid(), MaxPositionSize = 200, StoplossPct = -4, TakeProfitPct = 8}, new PositionLegService(new PriceService(), new QuantityService()));
        }

        private Order CreateSampleOrder(Direction direction, Guid orderId, Guid productId, Guid exchangeId, IEnumerable<Fill> fills = null)
        {
            var symbolId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductInfo = new ProductInfo { AssetType = "", BaseCurrency = "", LastUpdated = DateTimeOffset.UtcNow, MinOrderQtySize = 1, QuoteCurrency = "",
                    SettlementCurrency = "", ExpirationDate = None, MarginRequirement = None, TickSize = None },
                Symbol = new Symbol { Name = "BTCUSD", Id = symbolId },
            };
            var exchange = new Exchange { Name = "KRAKEN", Id = exchangeId };
            var order =  new Order(product, exchange, new ExchangeMapId { ExternalId = Guid.NewGuid().ToString(), InternalId = orderId }, 10m, 100m, direction, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

            if (fills != null)
            {
                foreach (var fill in fills)
                {
                    order.AddFill(fill);
                }
            }

            return order;


            //{
            //    Id = new ExchangeMapId(exchangeId, productId),
            //    Exchange = new Exchange { Id = exchangeId, Name = "Test Exchange" },
            //    Product = new Product { Id = productId, Symbol = new Symbol("Test Symbol"), ProductInfo = new ProductInfo() },
            //    Quantity = 10,
            //    Price = 100,
            //    Status = OrderStatus.Pending,
            //    LastUpdated = DateTimeOffset.UtcNow,
            //    ExchangeTimestamp = DateTimeOffset.UtcNow,
            //    Direction = direction,
            //    Fills = fills?.ToList() ?? new List<Fill>()
            //};
        }

        private Fill CreateSampleFill(Direction direction, decimal quantity, decimal price, Guid orderId)
        {
            return new Fill
            {
                Id = new ExchangeMapId { ExternalId = Guid.NewGuid().ToString(), InternalId = Guid.NewGuid() },
                OrderId = new ExchangeMapId { ExternalId = Guid.NewGuid().ToString(), InternalId = orderId },
                Direction = direction,
                Quantity = quantity,
                Price = price,
                ExchangeTimestamp = DateTimeOffset.UtcNow
            };
        }

        [TestMethod]
        public void UpdatePosition_WithEmptyOrders_ReturnsSuccessWithNoChanges()
        {
            var result = _positionService.UpdatePosition(_samplePosition, Enumerable.Empty<Order>());
            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(0, result.Match(Succ: pos => pos.Legs.Count, Fail: _ => -1));
        }

        [TestMethod]
        public void UpdatePosition_WithSingleOrderNoFills_AddsLegWithoutQuantity()
        {
            var order = CreateSampleOrder(Direction.LONG, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var result = _positionService.UpdatePosition(_samplePosition, new List<Order> { order });

            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(1, result.Match(Succ: pos => pos.Legs.Count, Fail: _ => -1));
            Assert.AreEqual(0, result.Match(Succ: pos => pos.Legs.First().Quantity, Fail: _ => -1));
        }

        [TestMethod]
        public void UpdatePosition_WithMultipleOrdersSameExchangeProduct_CombinesOrdersIntoSingleLeg()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var exchangeId = Guid.NewGuid();
            var order1 = CreateSampleOrder(Direction.LONG, orderId, productId, exchangeId, new List<Fill> { CreateSampleFill(Direction.LONG, 5, 100, orderId) });
            var order2 = CreateSampleOrder(Direction.LONG, orderId, productId, exchangeId, new List<Fill> { CreateSampleFill(Direction.LONG, 5, 100, orderId) });

            var result = _positionService.UpdatePosition(_samplePosition, new List<Order> { order1, order2 });

            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(1, result.Match(Succ: pos => pos.Legs.Count, Fail: _ => -1));
            Assert.AreEqual(10, result.Match(Succ: pos => pos.Legs.First().Quantity, Fail: _ => -1));
        }

        [TestMethod]
        public void UpdatePosition_WithOrdersDifferentExchanges_CreatesSeparateLegs()
        {
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var exchangeId = Guid.NewGuid();
            var order1 = CreateSampleOrder(Direction.LONG, orderId1, Guid.NewGuid(), exchangeId, new List<Fill> { CreateSampleFill(Direction.LONG, 5, 100, orderId1) });
            var order2 = CreateSampleOrder(Direction.LONG, orderId2, Guid.NewGuid(), exchangeId, new List<Fill> { CreateSampleFill(Direction.LONG, 5, 100, orderId2) });

            var result = _positionService.UpdatePosition(_samplePosition, new List<Order> { order1, order2 });

            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(2, result.Match(Succ: pos => pos.Legs.Count, Fail: _ => -1));
        }

        [TestMethod]
        public void UpdatePosition_WithPartialFills_UpdatesLegQuantityAccordingly()
        {
            var orderId = Guid.NewGuid();
            var fills = new List<Fill>
            {
                CreateSampleFill(Direction.LONG, 3, 100, orderId),
                CreateSampleFill(Direction.LONG, 2, 100, orderId)
            };
            var order = CreateSampleOrder(Direction.LONG, orderId, Guid.NewGuid(), Guid.NewGuid(), fills);

            var result = _positionService.UpdatePosition(_samplePosition, new List<Order> { order });

            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(1, result.Match(Succ: pos => pos.Legs.Count, Fail: _ => -1));
            Assert.AreEqual(5, result.Match(Succ: pos => pos.Legs.First().Quantity, Fail: _ => -1));
        }

        [TestMethod]
        public void UpdatePosition_WithDirectionChangeFromLongToShort_DecreasesLegQuantity()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var exchangeId = Guid.NewGuid();
            var order1 = CreateSampleOrder(Direction.LONG, orderId, productId, exchangeId, new List<Fill> { CreateSampleFill(Direction.LONG, 10, 100, orderId) });
            var order2 = CreateSampleOrder(Direction.SHORT, orderId, productId, exchangeId, new List<Fill> { CreateSampleFill(Direction.SHORT, 5, 100, orderId) });
            var result = _positionService.UpdatePosition(_samplePosition, new List<Order> { order1, order2 });

            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(1, result.Match(Succ: pos => pos.Legs.Count, Fail: _ => -1));
            Assert.AreEqual(5, result.Match(Succ: pos => pos.Legs.First().Quantity, Fail: _ => -1));
        }

        [TestMethod]
        public void UpdatePosition_WithInvalidDirectionInFill_ReturnsError()
        {
            var orderId1 = Guid.NewGuid();
            var invalidFill = CreateSampleFill((Direction)999, 5, 100, orderId1);
            var order = CreateSampleOrder(Direction.LONG, orderId1, Guid.NewGuid(), Guid.NewGuid(), new List<Fill> { invalidFill });

            var result = _positionService.UpdatePosition(_samplePosition, new List<Order> { order });

            Assert.IsTrue(result.IsFail);
            Assert.AreEqual("Invalid direction", result.Match(Fail: err => err.Message, Succ: _ => ""));
        }

        [TestMethod]
        public void UpdatePosition_WithMultipleLegsAcrossExchangesAndProducts_CreatesSeparateLegsForEach()
        {
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var exchangeId = Guid.NewGuid();
            var order1 = CreateSampleOrder(Direction.LONG, orderId1, Guid.NewGuid(), exchangeId, new List<Fill> { CreateSampleFill(Direction.LONG, 5, 100, orderId1) });
            var order2 = CreateSampleOrder(Direction.LONG, orderId2, Guid.NewGuid(), exchangeId, new List<Fill> { CreateSampleFill(Direction.LONG, 10, 100, orderId2) });

            var result = _positionService.UpdatePosition(_samplePosition, new List<Order> { order1, order2 });

            Assert.IsTrue(result.IsSucc);
            Assert.AreEqual(2, result.Match(Succ: pos => pos.Legs.Count, Fail: _ => -1));
        }

        [TestMethod]
        public void UpdatePosition_WithAllFailedOrders_ReturnsCombinedErrorMessages()
        {
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var exchangeId = Guid.NewGuid();
            var order1 = CreateSampleOrder(Direction.LONG, orderId1, Guid.NewGuid(), exchangeId,  new List<Fill> { CreateSampleFill(Direction.LONG, -5, 100, orderId1) });
            var order2 = CreateSampleOrder(Direction.SHORT, orderId2, Guid.NewGuid(), exchangeId, new List<Fill> { CreateSampleFill(Direction.SHORT, -5, 100, orderId2) });

            var result = _positionService.UpdatePosition(_samplePosition, new List<Order> { order1, order2 });

            Assert.IsTrue(result.IsFail);
            Assert.IsTrue(result.Match(Fail: err => err.Message, Succ: _ => "").Contains("Invalid quantity"));
        }


    }
}
