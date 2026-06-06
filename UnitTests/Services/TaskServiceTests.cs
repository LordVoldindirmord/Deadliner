using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using Deadliner.Domain.ViewModel.Tasks;
using Deadliner.Service.Implementations;
using Moq;

namespace UnitTests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _sut = new TaskService(_taskRepositoryMock.Object, _tagRepositoryMock.Object);
    }

    [Fact]
    public async Task CompleteAsync_WhenTaskNotFound_ReturnsNotFound()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((UserTask?)null);

        var result = await _sut.CompleteAsync(userId: 1, taskId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task CompleteAsync_WhenTaskBelongsToAnotherUser_ReturnsConflict()
    {
        var task = new UserTask { Id = 1, TagId = 5, Title = "Task", Status = TasksStatus.Active };
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 2 };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.CompleteAsync(userId: 1, taskId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("Эта задача принадлежит другому пользователю", result.Description);
    }

    [Fact]
    public async Task CompleteAsync_WhenTaskAlreadyCompleted_ReturnsConflict()
    {
        var task = new UserTask { Id = 1, TagId = 5, Title = "Task", Status = TasksStatus.Completed };
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.CompleteAsync(userId: 1, taskId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("Задача уже выполнена", result.Description);
    }

    [Fact]
    public async Task CompleteAsync_WhenTaskIsActive_ReturnsOk()
    {
        var task = new UserTask { Id = 1, TagId = 5, Title = "Task", Status = TasksStatus.Active };
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.CompleteAsync(userId: 1, taskId: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(TasksStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenTitleAlreadyExists_ReturnsConflict()
    {
        _taskRepositoryMock.Setup(r => r.ExistsByTitleAsync("Task", 5)).ReturnsAsync(true);

        var result = await _sut.CreateAsync(1, new TaskCreateViewModel
        {
            Title = "Task",
            TagId = 5,
            Priority = PriorityStatus.Medium,
            Deadline = DateTime.Now.AddDays(1),
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WhenTagNotFound_ReturnsNotFound()
    {
        _taskRepositoryMock.Setup(r => r.ExistsByTitleAsync("Task", 5)).ReturnsAsync(false);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Tag?)null);

        var result = await _sut.CreateAsync(1, new TaskCreateViewModel
        {
            Title = "Task",
            TagId = 5,
            Priority = PriorityStatus.Medium,
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WhenTagBelongsToAnotherUser_ReturnsConflict()
    {
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 2 };
        _taskRepositoryMock.Setup(r => r.ExistsByTitleAsync("Task", 5)).ReturnsAsync(false);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.CreateAsync(1, new TaskCreateViewModel
        {
            Title = "Task",
            TagId = 5,
            Priority = PriorityStatus.Medium,
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsValid_ReturnsTaskDetail()
    {
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _taskRepositoryMock.Setup(r => r.ExistsByTitleAsync("New Task", 5)).ReturnsAsync(false);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.CreateAsync(1, new TaskCreateViewModel
        {
            Title = "New Task",
            Description = "Description",
            TagId = 5,
            Priority = PriorityStatus.High,
            Deadline = DateTime.Now.AddDays(3),
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("New Task", result.Data.Title);
        Assert.Equal("Work", result.Data.Tag.Name);
        Assert.Equal(TasksStatus.Active, result.Data.Status);
        _taskRepositoryMock.Verify(r => r.CreateAsync(It.Is<UserTask>(t =>
            t.Title == "New Task" &&
            t.TagId == 5 &&
            t.Priority == PriorityStatus.High)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskNotFound_ReturnsNotFound()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((UserTask?)null);

        var result = await _sut.DeleteAsync(userId: 1, taskId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskExists_ReturnsOk()
    {
        var task = new UserTask { Id = 1, TagId = 5, Title = "Task" };
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.DeleteAsync(userId: 1, taskId: 1);

        Assert.True(result.IsSuccess);
        _taskRepositoryMock.Verify(r => r.DeleteAsync(task), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskBelongsToAnotherUser_ReturnsConflict()
    {
        var task = new UserTask { Id = 1, TagId = 5, Title = "Task" };
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 2 };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.GetByIdAsync(userId: 1, taskId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskExists_ReturnsTaskDetail()
    {
        var task = new UserTask
        {
            Id = 1,
            TagId = 5,
            Title = "Task",
            Description = "Desc",
            Priority = PriorityStatus.Medium,
            Status = TasksStatus.Active,
            CreatedAt = DateTime.Now,
        };
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.GetByIdAsync(userId: 1, taskId: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Task", result.Data!.Title);
        Assert.Equal("Work", result.Data.Tag.Name);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenUserHasNoTags_ReturnsNoContent()
    {
        _tagRepositoryMock
            .Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(Enumerable.Empty<Tag>());

        var result = await _sut.GetDashboardAsync(userId: 1, limit: 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenUserHasNoTasks_ReturnsNoContent()
    {
        _tagRepositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(new[]
        {
            new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 },
        });
        _taskRepositoryMock
            .Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(Enumerable.Empty<UserTask>());

        var result = await _sut.GetDashboardAsync(userId: 1, limit: 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenDataExists_ReturnsDashboard()
    {
        var tags = new[]
        {
            new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 },
        };
        var tasks = new[]
        {
            new UserTask
            {
                Id = 1,
                TagId = 1,
                Title = "Active",
                Status = TasksStatus.Active,
                Deadline = DateTime.Now.AddDays(1),
                CreatedAt = DateTime.Now,
            },
            new UserTask
            {
                Id = 2,
                TagId = 1,
                Title = "Done",
                Status = TasksStatus.Completed,
                CreatedAt = DateTime.Now,
            },
        };
        _tagRepositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(tags);
        _taskRepositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(tasks);

        var result = await _sut.GetDashboardAsync(userId: 1, limit: 5);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.ActiveCount);
        Assert.Equal(1, result.Data.CompletedCount);
        Assert.NotEmpty(result.Data.TaskGroups);
    }

    [Fact]
    public async Task GetFilteredAsync_WhenUserHasNoTasks_ReturnsNoContent()
    {
        _taskRepositoryMock
            .Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(Enumerable.Empty<UserTask>());
        _tagRepositoryMock
            .Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(Enumerable.Empty<Tag>());

        var result = await _sut.GetFilteredAsync(1, new TaskFilterViewModel());

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task GetFilteredAsync_WhenFilterMatchesTasks_ReturnsFilteredList()
    {
        var tags = new[]
        {
            new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 },
        };
        var tasks = new[]
        {
            new UserTask
            {
                Id = 1,
                TagId = 1,
                Title = "Active task",
                Status = TasksStatus.Active,
                Priority = PriorityStatus.High,
                CreatedAt = DateTime.Now,
            },
            new UserTask
            {
                Id = 2,
                TagId = 1,
                Title = "Done task",
                Status = TasksStatus.Completed,
                Priority = PriorityStatus.Low,
                CreatedAt = DateTime.Now,
            },
        };
        _taskRepositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(tasks);
        _tagRepositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(tags);

        var result = await _sut.GetFilteredAsync(1, new TaskFilterViewModel
        {
            Status = TasksStatus.Active,
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
        Assert.Single(result.Data.Tasks);
        Assert.Equal("Active task", result.Data.Tasks.First().Title);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskNotFound_ReturnsNotFound()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((UserTask?)null);

        var result = await _sut.UpdateAsync(1, new TaskEditViewModel
        {
            Id = 1,
            Title = "Updated",
            TagId = 5,
            Priority = PriorityStatus.Medium,
            Status = TasksStatus.Active,
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenDataIsValid_ReturnsUpdatedTask()
    {
        var task = new UserTask
        {
            Id = 1,
            TagId = 5,
            Title = "Old title",
            Status = TasksStatus.Active,
            Priority = PriorityStatus.Low,
        };
        var tag = new Tag { Id = 5, Name = "Work", ColorHex = "#FF0000", UserId = 1 };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _taskRepositoryMock.Setup(r => r.ExistsByTitleAsync("New title", 5)).ReturnsAsync(false);
        _tagRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        var result = await _sut.UpdateAsync(1, new TaskEditViewModel
        {
            Id = 1,
            Title = "New title",
            Description = "Updated desc",
            TagId = 5,
            Priority = PriorityStatus.Critical,
            Status = TasksStatus.Completed,
            Deadline = DateTime.Now.AddDays(2),
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("New title", result.Data!.Title);
        Assert.Equal(TasksStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task GetFilteredAsync_WhenSortedByDeadlineDesc_ReturnsCorrectOrder()
    {
        var tags = new[]
        {
        new Tag { Id = 1, Name = "Work", ColorHex = "#FF0000", UserId = 1 },
    };
        var tasks = new[]
        {
        new UserTask { Id = 1, TagId = 1, Title = "First", Status = TasksStatus.Active, Deadline = new DateTime(2026, 5, 10), CreatedAt = DateTime.Now },
        new UserTask { Id = 2, TagId = 1, Title = "Second", Status = TasksStatus.Active, Deadline = new DateTime(2026, 5, 20), CreatedAt = DateTime.Now },
        new UserTask { Id = 3, TagId = 1, Title = "Third", Status = TasksStatus.Active, Deadline = new DateTime(2026, 5, 15), CreatedAt = DateTime.Now },
    };
        _taskRepositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(tasks);
        _tagRepositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(tags);

        var result = await _sut.GetFilteredAsync(1, new TaskFilterViewModel
        {
            SortBy = "deadline",
            SortDirection = "desc",
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data!.TotalCount);
        Assert.Equal("Second", result.Data.Tasks[0].Title);  // 20 мая
        Assert.Equal("Third", result.Data.Tasks[1].Title);   // 15 мая
        Assert.Equal("First", result.Data.Tasks[2].Title);   // 10 мая
    }
}
