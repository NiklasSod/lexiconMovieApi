using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using MovieApi.Controllers;
using MovieApi.DTOs.Genre;
using MovieApi.Models;

namespace MovieApi.Tests.Controllers;

/// <summary>
/// Unit tests for GenreController using Moq to mock IMovieApiContext.
/// Tests cover CreateGenre, GetGenreById, and DeleteGenreById actions.
/// </summary>
public class GenreControllerTests
{
    /// <summary>
    /// Helper that creates a Mock of DbSet<Genre> backed by an in-memory List<Genre>.
    /// Supports all EF Core async LINQ methods (AnyAsync, FirstOrDefaultAsync, etc.)
    /// and DbSet methods (Add, Remove, FindAsync).
    /// </summary>
    private static Mock<DbSet<Genre>> CreateMockGenreDbSet(List<Genre> data)
    {
        var queryable = data.AsQueryable();

        var mockSet = new Mock<DbSet<Genre>>();

        // --- IQueryable setup (required for async LINQ extension methods) ---
        mockSet.As<IQueryable<Genre>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<Genre>(queryable.Provider));
        mockSet.As<IQueryable<Genre>>().Setup(m => m.Expression)
            .Returns(queryable.Expression);
        mockSet.As<IQueryable<Genre>>().Setup(m => m.ElementType)
            .Returns(queryable.ElementType);
        mockSet.As<IQueryable<Genre>>().Setup(m => m.GetEnumerator())
            .Returns(queryable.GetEnumerator());
        mockSet.As<IAsyncEnumerable<Genre>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Genre>(queryable.GetEnumerator()));

        // --- Add: adds to the backing list ---
        mockSet.Setup(m => m.Add(It.IsAny<Genre>()))
            .Callback<Genre>(data.Add)
            .Returns<Genre>(g => null!); // EF returns the added entity via an internal tracker

        // --- Remove: removes from the backing list ---
        mockSet.Setup(m => m.Remove(It.IsAny<Genre>()))
            .Callback<Genre>(g => data.Remove(g))
            .Returns<Genre>(g => null!);

        // --- FindAsync: looks up by primary key (Id) ---
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((object[] ids) => data.FirstOrDefault(g => g.Id == (int)ids[0]));

        return mockSet;
    }

    // ─────────────────────────────────────────────────────────────
    // CreateGenre Tests
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateGenre_WithNewName_ReturnsCreatedAtAction()
    {
        // Arrange
        var data = new List<Genre>
        {
            new Genre { Id = 1, Name = "Action" }
        };
        var mockSet = CreateMockGenreDbSet(data);

        var mockContext = new Mock<IMovieApiContext>();
        mockContext.Setup(c => c.Genres).Returns(mockSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var controller = new GenreController(mockContext.Object);

        var dto = new GenreCreateDto { Name = "Comedy" };

        // Act
        var result = await controller.CreateGenre(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var genre = Assert.IsType<Genre>(createdResult.Value);
        Assert.Equal("Comedy", genre.Name);
        Assert.Equal(2, data.Count); // original + new
    }

    [Fact]
    public async Task CreateGenre_WithExistingName_ReturnsBadRequest()
    {
        // Arrange
        var data = new List<Genre>
        {
            new Genre { Id = 1, Name = "Action" }
        };
        var mockSet = CreateMockGenreDbSet(data);

        var mockContext = new Mock<IMovieApiContext>();
        mockContext.Setup(c => c.Genres).Returns(mockSet.Object);

        var controller = new GenreController(mockContext.Object);

        var dto = new GenreCreateDto { Name = "Action" }; // same name, different case should still trigger

        // Act
        var result = await controller.CreateGenre(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Genre already exists.", badRequestResult.Value);
        Assert.Single(data); // nothing added
    }

    [Fact]
    public async Task CreateGenre_WithExistingNameDifferentCase_ReturnsBadRequest()
    {
        // Arrange
        var data = new List<Genre>
        {
            new Genre { Id = 1, Name = "Action" }
        };
        var mockSet = CreateMockGenreDbSet(data);

        var mockContext = new Mock<IMovieApiContext>();
        mockContext.Setup(c => c.Genres).Returns(mockSet.Object);

        var controller = new GenreController(mockContext.Object);

        var dto = new GenreCreateDto { Name = "action" }; // lowercase

        // Act
        var result = await controller.CreateGenre(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Genre already exists.", badRequestResult.Value);
    }

    // ─────────────────────────────────────────────────────────────
    // GetGenreById Tests
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetGenreById_WithValidId_ReturnsGenre()
    {
        // Arrange
        var data = new List<Genre>
        {
            new Genre { Id = 1, Name = "Action" },
            new Genre { Id = 2, Name = "Comedy" }
        };
        var mockSet = CreateMockGenreDbSet(data);

        var mockContext = new Mock<IMovieApiContext>();
        mockContext.Setup(c => c.Genres).Returns(mockSet.Object);

        var controller = new GenreController(mockContext.Object);

        // Act
        var result = await controller.GetGenreById(2);

        // Assert
        var genre = Assert.IsType<Genre>(result.Value);
        Assert.Equal(2, genre.Id);
        Assert.Equal("Comedy", genre.Name);
    }

    [Fact]
    public async Task GetGenreById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var data = new List<Genre>
        {
            new Genre { Id = 1, Name = "Action" }
        };
        var mockSet = CreateMockGenreDbSet(data);

        var mockContext = new Mock<IMovieApiContext>();
        mockContext.Setup(c => c.Genres).Returns(mockSet.Object);

        var controller = new GenreController(mockContext.Object);

        // Act
        var result = await controller.GetGenreById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ─────────────────────────────────────────────────────────────
    // DeleteGenreById Tests
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteGenreById_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var data = new List<Genre>
        {
            new Genre { Id = 1, Name = "Action" },
            new Genre { Id = 2, Name = "Comedy" }
        };
        var mockSet = CreateMockGenreDbSet(data);

        var mockContext = new Mock<IMovieApiContext>();
        mockContext.Setup(c => c.Genres).Returns(mockSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var controller = new GenreController(mockContext.Object);

        // Act
        var result = await controller.DeleteGenreById(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Single(data); // removed Action, only Comedy left
        Assert.DoesNotContain(data, g => g.Id == 1);
    }

    [Fact]
    public async Task DeleteGenreById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var data = new List<Genre>
        {
            new Genre { Id = 1, Name = "Action" }
        };
        var mockSet = CreateMockGenreDbSet(data);

        var mockContext = new Mock<IMovieApiContext>();
        mockContext.Setup(c => c.Genres).Returns(mockSet.Object);

        var controller = new GenreController(mockContext.Object);

        // Act
        var result = await controller.DeleteGenreById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        Assert.Single(data); // nothing removed
    }
}

// ═══════════════════════════════════════════════════════════════
// EF Core async query provider helpers for mocking DbSet<T>
// These adapt IQueryable<T> so that async EF Core LINQ methods
// (AnyAsync, FirstOrDefaultAsync, ToListAsync, etc.) work with Moq.
// ═══════════════════════════════════════════════════════════════

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable) { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
        => new ValueTask<bool>(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return default;
    }
}

internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncEnumerable<T>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression)
        => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression)
        => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: [typeof(Expression)])
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(_inner, [expression]);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(null, [executionResult])!;
    }
}
