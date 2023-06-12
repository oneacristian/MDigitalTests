using MDigital.Controllers;
using MDigital.Models;
using MDigital;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDigitalTests
{
    [TestFixture]
    internal class TransactionsControllerTests
    {
        private TransactionsController _controller;
        private MDigitalDBContext _context;

        [SetUp]
        public void Setup()
        {
            // In-memory database for testing
            var options = new DbContextOptionsBuilder<MDigitalDBContext>()
                .UseInMemoryDatabase(databaseName: "test_db")
                .Options;
            _context = new MDigitalDBContext(options);
            _controller = new TransactionsController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetTransactions_ReturnsOkResult()
        {
            await SeedData();

            var result = await _controller.GetTransactions();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<IEnumerable<Transaction>>(okResult.Value);
            var transactions = okResult.Value as IEnumerable<Transaction>;
            Assert.AreEqual(2, transactions.Count());
        }

        [Test]
        public async Task GetTransactions_WhenNoTransactions_ReturnsEmptyResult()
        {
            var result = await _controller.GetTransactions();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<IEnumerable<Transaction>>(okResult.Value);
            var transactions = okResult.Value as IEnumerable<Transaction>;
            Assert.IsEmpty(transactions);
        }

        [Test]
        public async Task GetTransaction_WithValidId_ReturnsOkResult()
        {
            await SeedData();

            var result = await _controller.GetTransaction(1);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<Transaction>(okResult.Value);
            var transaction = okResult.Value as Transaction;
            Assert.AreEqual(1, transaction.Id);
        }

        [Test]
        public async Task GetTransaction_WithInvalidId_ReturnsNotFoundResult()
        {
            var result = await _controller.GetTransaction(1);

            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task CreateTransaction_WithValidData_ReturnsCreatedResult()
        {
            var dto = new CreateTransactionDto
            {
                PaymentId = 1,
                CustomerId = 1,
                Articles = new List<ArticleDto>
                {
                    new ArticleDto { ArticleId = 1, ArticleQty = 2 }
                }
            };

            var result = await _controller.CreateTransaction(dto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdAtResult = result.Result as CreatedAtActionResult;
            Assert.IsInstanceOf<Transaction>(createdAtResult.Value);
            var transaction = createdAtResult.Value as Transaction;
            Assert.AreEqual(1, transaction.Id);
        }

        [Test]
        public async Task CreateTransaction_WithInvalidData_ReturnsBadRequestResult()
        {
            var dto = new CreateTransactionDto
            {
                PaymentId = 1,
                CustomerId = 1,
                Articles = null // Invalid: Articles list is null
            };

            var result = await _controller.CreateTransaction(dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task UpdateTransaction_WithValidData_ReturnsNoContentResult()
        {
            await SeedData();
            var dto = new UpdateTransactionDto
            {
                PaymentId = 2,
                CustomerId = 2,
                Articles = new List<ArticleDto>
                {
                    new ArticleDto { ArticleId = 2, ArticleQty = 3 }
                }
            };

            var result = await _controller.UpdateTransaction(1, dto);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task UpdateTransaction_WithInvalidId_ReturnsNotFoundResult()
        {
            var dto = new UpdateTransactionDto
            {
                PaymentId = 2,
                CustomerId = 2,
                Articles = new List<ArticleDto>
                {
                    new ArticleDto { ArticleId = 2, ArticleQty = 3 }
                }
            };

            var result = await _controller.UpdateTransaction(1, dto);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task DeleteTransaction_WithValidId_ReturnsNoContentResult()
        {
            await SeedData();

            var result = await _controller.DeleteTransaction(1);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteTransaction_WithInvalidId_ReturnsNotFoundResult()
        {
            var result = await _controller.DeleteTransaction(1);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        private async Task SeedData()
        {
            var payment = new Payment { Id = 1, Amount = 100 };
            var customer = new Customer { Id = 1, Name = "John Doe" };
            var transaction = new Transaction { Id = 1, PaymentId = 1, CustomerId = 1 };

            _context.Payments.Add(payment);
            _context.Customers.Add(customer);
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();
        }
    }
}

