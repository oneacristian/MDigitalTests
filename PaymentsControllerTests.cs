using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using MDigital;
using MDigital.Controllers;
using MDigital.Models;

namespace MDigitalTests
{
    [TestFixture]
    public class PaymentsControllerTests
    {
        private PaymentsController _controller;
        private DbContextOptions<MDigitalDBContext> _options;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<MDigitalDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new MDigitalDBContext(_options))
            {
                context.Database.EnsureCreated();

                var payments = new List<Payment>
                {
                    new Payment { Id = 1, Date = DateTime.Now.AddDays(-1), Amount = 100 },
                    new Payment { Id = 2, Date = DateTime.Now, Amount = 200 },
                    new Payment { Id = 3, Date = DateTime.Now.AddDays(1), Amount = 300 }
                };

                context.Payments.AddRange(payments);
                context.SaveChanges();
            }

            var dbContext = new MDigitalDBContext(_options);
            _controller = new PaymentsController(dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = new MDigitalDBContext(_options))
            {
                context.Database.EnsureDeleted();
            }
        }

        [Test]
        public async Task GetPayments_ReturnsOkResult()
        {
            var result = await _controller.GetPayments();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task GetPayments_ReturnsAllPayments()
        {
            var result = await _controller.GetPayments();
            var okResult = result.Result as OkObjectResult;
            var payments = okResult.Value as IEnumerable<Payment>;

            Assert.AreEqual(3, payments.Count());
        }

        [Test]
        public async Task GetPayment_WithValidId_ReturnsOkResult()
        {
            var paymentId = 1;

            var result = await _controller.GetPayment(paymentId);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task GetPayment_WithValidId_ReturnsCorrectPayment()
        {
            var paymentId = 1;

            var result = await _controller.GetPayment(paymentId);
            var okResult = result.Result as OkObjectResult;
            var payment = okResult.Value as Payment;

            Assert.AreEqual(paymentId, payment.Id);
        }

        [Test]
        public async Task GetPayment_WithInvalidId_ReturnsNotFoundResult()
        {
            var paymentId = 10;

            var result = await _controller.GetPayment(paymentId);

            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task CreatePayment_WithValidPayment_ReturnsCreatedAtActionResult()
        {
            var paymentDto = new CreatePaymentDto
            {
                Date = DateTime.Now,
                Amount = 400
            };

            var result = await _controller.CreatePayment(paymentDto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
        }

        [Test]
        public async Task CreatePayment_WithInvalidPayment_ReturnsBadRequestResult()
        {
            var paymentDto = new CreatePaymentDto
            {
                Date = DateTime.Now.AddDays(1),
                Amount = -100
            };

            var result = await _controller.CreatePayment(paymentDto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task UpdatePayment_WithValidPayment_ReturnsNoContentResult()
        {
            var paymentDto = new UpdatePaymentDto
            {
                Id = 2,
                Date = DateTime.Now,
                Amount = 250
            };

            var result = await _controller.UpdatePayment(paymentDto.Id, paymentDto);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task UpdatePayment_WithInvalidId_ReturnsBadRequestResult()
        {
            var paymentDto = new UpdatePaymentDto
            {
                Id = 10,
                Date = DateTime.Now,
                Amount = 250
            };

            var result = await _controller.UpdatePayment(paymentDto.Id, paymentDto);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task UpdatePayment_WithInvalidPayment_ReturnsBadRequestResult()
        {
            var paymentDto = new UpdatePaymentDto
            {
                Id = 2,
                Date = DateTime.Now.AddDays(1),
                Amount = -100
            };

            var result = await _controller.UpdatePayment(paymentDto.Id, paymentDto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeletePayment_WithValidId_ReturnsNoContentResult()
        {
            var paymentId = 3;

            var result = await _controller.DeletePayment(paymentId);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeletePayment_WithInvalidId_ReturnsNotFoundResult()
        {
            var paymentId = 10;

            var result = await _controller.DeletePayment(paymentId);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }
}
