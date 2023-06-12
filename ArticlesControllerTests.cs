using MDigital.Controllers;
using MDigital.Models;
using MDigital;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MDigitalTests
{
    [TestFixture]
    internal class ArticlesControllerTests
    {
        private ArticlesController _controller;
        private MDigitalDBContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MDigitalDBContext>()
                .UseInMemoryDatabase(databaseName: "test_db")
                .Options;

            _context = new MDigitalDBContext(options);
            _controller = new ArticlesController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetArticles_ReturnsOkResult()
        {
            await SeedData();

            var result = await _controller.GetArticles();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<IEnumerable<Article>>(okResult.Value);
            var articles = okResult.Value as IEnumerable<Article>;
            Assert.AreEqual(2, articles.Count());
        }

        [Test]
        public async Task GetArticles_WhenNoArticles_ReturnsEmptyResult()
        {
            var result = await _controller.GetArticles();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<IEnumerable<Article>>(okResult.Value);
            var articles = okResult.Value as IEnumerable<Article>;
            Assert.IsEmpty(articles);
        }

        [Test]
        public async Task GetArticle_WithValidId_ReturnsOkResult()
        {
            await SeedData();

            var result = await _controller.GetArticle(1);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<Article>(okResult.Value);
            var article = okResult.Value as Article;
            Assert.AreEqual(1, article.Id);
        }

        [Test]
        public async Task GetArticle_WithInvalidId_ReturnsNotFoundResult()
        {
            var result = await _controller.GetArticle(1);

            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task CreateArticle_WithValidData_ReturnsCreatedResult()
        {
            var article = new Article
            {
                Id = 3,
                Name = "New Article",
                Price = 9.99m,
                Quantity = 10
            };

            var result = await _controller.CreateArticle(article);

            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdAtResult = result.Result as CreatedAtActionResult;
            Assert.IsInstanceOf<Article>(createdAtResult.Value);
            var createdArticle = createdAtResult.Value as Article;
            Assert.AreEqual(3, createdArticle.Id);
        }

        [Test]
        public async Task CreateArticle_WithInvalidData_ReturnsBadRequestResult()
        {
            var article = new Article
            {
                Id = 1, // Invalid: Duplicate ID
                Name = "New Article",
                Price = 9.99m,
                Quantity = 10
            };

            var result = await _controller.CreateArticle(article);

            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task UpdateArticle_WithValidData_ReturnsNoContentResult()
        {
            await SeedData();
            var article = new Article
            {
                Id = 2,
                Name = "Updated Article",
                Price = 19.99m,
                Quantity = 5
            };

            var result = await _controller.UpdateArticle(2, article);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task UpdateArticle_WithInvalidId_ReturnsBadRequestResult()
        {
            var article = new Article
            {
                Id = 2,
                Name = "Updated Article",
                Price = 19.99m,
                Quantity = 5
            };

            var result = await _controller.UpdateArticle(2, article);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task UpdateArticle_WithNonexistentId_ReturnsNotFoundResult()
        {
            var article = new Article
            {
                Id = 3,
                Name = "Updated Article",
                Price = 19.99m,
                Quantity = 5
            };

            var result = await _controller.UpdateArticle(3, article);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task DeleteArticle_WithValidId_ReturnsNoContentResult()
        {
            await SeedData();

            var result = await _controller.DeleteArticle(1);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteArticle_WithInvalidId_ReturnsNotFoundResult()
        {
            var result = await _controller.DeleteArticle(1);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        private async Task SeedData()
        {
            var articles = new List<Article>
            {
                new Article { Id = 1, Name = "Article 1", Price = 10.99m, Quantity = 20 },
                new Article { Id = 2, Name = "Article 2", Price = 20.99m, Quantity = 30 }
            };

            _context.Articles.AddRange(articles);
            await _context.SaveChangesAsync();
        }
    }
}
