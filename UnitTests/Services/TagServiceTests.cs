using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using Deadliner.Domain.ViewModel.Tags;
using Deadliner.Service.Implementations;
using Moq;

namespace UnitTests.Services;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();
    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();
    private readonly TagService _sut;

    public TagServiceTests()
    {
        _sut = new TagService(_tagRepositoryMock.Object, _taskRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTagNotFound_ReturnsNotFound()
    {
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag?)null);

        var result = await _sut.GetByIdAsync(userId: 1, tagId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTagBelongsToAnotherUser_ReturnsForbidden()
    {
        var tag = new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 2 };
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        var result = await _sut.GetByIdAsync(userId: 1, tagId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Forbidden, result.StatusCode);
        Assert.Equal("Нет доступа к этому тегу", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTagExists_ReturnsTagWithTaskCount()
    {
        var tag = new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);
        _taskRepositoryMock.Setup(r => r.GetByTagIdAsync(1)).ReturnsAsync(new[]
        {
            new UserTask { Id = 10, TagId = 1, Title = "Task 1" },
            new UserTask { Id = 11, TagId = 1, Title = "Task 2" },
        });

        var result = await _sut.GetByIdAsync(userId: 1, tagId: 1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Work", result.Data.Name);
        Assert.Equal(2, result.Data.ActiveTaskCount);
    }

    [Fact]
    public async Task CreateAsync_WhenTagAlreadyExists_ReturnsConflict()
    {
        _tagRepositoryMock.Setup(r => r.ExistsByNameAsync("Work", 1)).ReturnsAsync(true);

        var result = await _sut.CreateAsync(1, new TagCreateViewModel
        {
            Name = "Work",
            ColorHex = "#FF0000",
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        _tagRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Tag>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsValid_ReturnsCreated()
    {
        _tagRepositoryMock.Setup(r => r.ExistsByNameAsync("Work", 1)).ReturnsAsync(false);

        var result = await _sut.CreateAsync(1, new TagCreateViewModel
        {
            Name = "Work",
            ColorHex = "#FF0000",
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.Created, result.StatusCode);
        Assert.True(result.Data);
        _tagRepositoryMock.Verify(r => r.CreateAsync(It.Is<Tag>(t =>
            t.Name == "Work" &&
            t.ColorHex == "#FF0000" &&
            t.UserId == 1)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTagNotFound_ReturnsNotFound()
    {
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag?)null);

        var result = await _sut.DeleteAsync(userId: 1, tagId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_WhenTagBelongsToAnotherUser_ReturnsConflict()
    {
        var tag = new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 2 };
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        var result = await _sut.DeleteAsync(userId: 1, tagId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("Вы не можете удалить чужой тег", result.Description);
    }

    [Fact]
    public async Task DeleteAsync_WhenTagExists_ReturnsOk()
    {
        var tag = new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        var result = await _sut.DeleteAsync(userId: 1, tagId: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        _tagRepositoryMock.Verify(r => r.DeleteAsync(tag), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenUserHasNoTags_ReturnsNoContent()
    {
        _tagRepositoryMock
            .Setup(r => r.GetByUserIdWithTaskCountAsync(1))
            .ReturnsAsync(Enumerable.Empty<(Tag tag, int taskCount)>());

        var result = await _sut.GetByUserIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenUserHasTags_ReturnsList()
    {
        var tag1 = new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        var tag2 = new Tag { Id = 2, Name = "Home", ColorHex = "#00FF00", UserId = 1 };
        _tagRepositoryMock
            .Setup(r => r.GetByUserIdWithTaskCountAsync(1))
            .ReturnsAsync(new[] { (tag1, 3), (tag2, 0) });

        var result = await _sut.GetByUserIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.Contains(result.Data, t => t.Name == "Work" && t.ActiveTaskCount == 3);
    }

    [Fact]
    public async Task UpdateAsync_WhenTagNotFound_ReturnsNotFound()
    {
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag?)null);

        var result = await _sut.UpdateAsync(1, new TagEditViewModel
        {
            Id = 1,
            Name = "Updated",
            ColorHex = "#0000FF",
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenTagBelongsToAnotherUser_ReturnsConflict()
    {
        var tag = new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 2 };
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        var result = await _sut.UpdateAsync(1, new TagEditViewModel
        {
            Id = 1,
            Name = "Updated",
            ColorHex = "#0000FF",
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyTaken_ReturnsConflict()
    {
        var tag = new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);
        _tagRepositoryMock.Setup(r => r.ExistsByNameAsync("Personal", 1)).ReturnsAsync(true);

        var result = await _sut.UpdateAsync(1, new TagEditViewModel
        {
            Id = 1,
            Name = "Personal",
            ColorHex = "#0000FF",
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("Тег с таким названием уже существует", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_WhenDataIsValid_ReturnsUpdatedTag()
    {
        var tag = new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);
        _taskRepositoryMock.Setup(r => r.GetByTagIdAsync(1)).ReturnsAsync(new[]
        {
            new UserTask { Id = 10, TagId = 1, Title = "Task 1" },
        });

        var result = await _sut.UpdateAsync(1, new TagEditViewModel
        {
            Id = 1,
            Name = "Work Updated",
            ColorHex = "#0000FF",
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Work Updated", result.Data!.Name);
        Assert.Equal("#0000FF", result.Data.ColorHex);
        Assert.Equal(1, result.Data.ActiveTaskCount);
        _tagRepositoryMock.Verify(r => r.UpdateAsync(tag), Times.Once);
    }
}
