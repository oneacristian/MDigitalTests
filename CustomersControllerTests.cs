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
    internal class CustomersControllerTests
    {
        private CustomersController _controller;
        private MDigitalDBContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MDigitalDBContext>()
                .UseInMemoryDatabase(databaseName: "test_db")
                .Options;
            _context = new MDigitalDBContext(options);
            _controller = new CustomersController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetCustomers_ReturnsOkResult()
        {
            await SeedData();

            var result = await _controller.GetCustomers();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<IEnumerable<Customer>>(okResult.Value);
            var customers = okResult.Value as IEnumerable<Customer>;
            Assert.AreEqual(2, customers.Count());
        }

        [Test]
        public async Task GetCustomers_WhenNoCustomers_ReturnsEmptyResult()
        {
            var result = await _controller.GetCustomers();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<IEnumerable<Customer>>(okResult.Value);
            var customers = okResult.Value as IEnumerable<Customer>;
            Assert.IsEmpty(customers);
        }

        [Test]
        public async Task GetCustomer_WithValidId_ReturnsOkResult()
        {
            await SeedData();

            var result = await _controller.GetCustomer(1);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<Customer>(okResult.Value);
            var customer = okResult.Value as Customer;
            Assert.AreEqual(1, customer.Id);
        }

        [Test]
        public async Task GetCustomer_WithInvalidId_ReturnsNotFoundResult()
        {
            var result = await _controller.GetCustomer(1);

            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task CreateCustomer_WithValidData_ReturnsCreatedResult()
        {
            var customer = new Customer
            {
                Id = 3,
                Name = "New Customer"
            };

            var result = await _controller.CreateCustomer(customer);

            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdAtResult = result.Result as CreatedAtActionResult;
            Assert.IsInstanceOf<Customer>(createdAtResult.Value);
            var createdCustomer = createdAtResult.Value as Customer;
            Assert.AreEqual(3, createdCustomer.Id);
        }

        [Test]
        public async Task CreateCustomer_WithInvalidData_ReturnsBadRequestResult()
        {
            var customer = new Customer
            {
                Id = 1, // Invalid: Duplicate ID
                Name = "New Customer"
            };

            var result = await _controller.CreateCustomer(customer);

            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task UpdateCustomer_WithValidData_ReturnsNoContentResult()
        {
            await SeedData();
            var customer = new Customer
            {
                Id = 1,
                Name = "Updated Customer"
            };

            var result = await _controller.UpdateCustomer(1, customer);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task UpdateCustomer_WithInvalidId_ReturnsBadRequestResult()
        {
            var customer = new Customer
            {
                Id = 2,
                Name = "Updated Customer"
            };

            var result = await _controller.UpdateCustomer(2, customer);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task UpdateCustomer_WithNonexistentId_ReturnsNotFoundResult()
        {
            var customer = new Customer
            {
                Id = 3,
                Name = "Updated Customer"
            };

            var result = await _controller.UpdateCustomer(3, customer);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task DeleteCustomer_WithValidId_ReturnsNoContentResult()
        {
            await SeedData();

            var result = await _controller.DeleteCustomer(1);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteCustomer_WithInvalidId_ReturnsNotFoundResult()
        {
            var result = await _controller.DeleteCustomer(1);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        private async Task SeedData()
        {
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Customer 1" },
                new Customer { Id = 2, Name = "Customer 2" }
            };

            _context.Customers.AddRange(customers);
            await _context.SaveChangesAsync();
        }
    }
}
