using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using Deadliner.Domain.ViewModel.Account;
using Deadliner.Service.Implementations;
using Moq;

namespace UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserNotFound_ReturnsBadRequest()
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((User?)null);

        var result = await _sut.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Пользователь с таким id не найден", result.Description);
        _userRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_ReturnsOk()
    {
        var user = new User { Id = 1, Login = "test", Email = "test@test.com" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.True(result.Data);
        _userRepositoryMock.Verify(r => r.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ReturnsNotFound()
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetByIdAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsProfile()
    {
        var createdAt = new DateTime(2025, 1, 1);
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            CreatedAt = createdAt,
        };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Id);
        Assert.Equal("artem", result.Data.Login);
        Assert.Equal("artem@test.com", result.Data.Email);
        Assert.Equal(createdAt, result.Data.CreatedAt);
    }

    [Fact]
    public async Task LoginAsync_ByLoginWithValidPassword_ReturnsOk()
    {
        const string password = "password123";
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.Now,
        };
        _userRepositoryMock.Setup(r => r.GetByLoginAsync("artem")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginViewModel
        {
            LoginOrEmail = "artem",
            Password = password,
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.Equal("Вход выполнен успешно", result.Description);
        Assert.Equal("artem", result.Data!.Login);
        _userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ByEmailWithValidPassword_ReturnsOk()
    {
        const string password = "password123";
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.Now,
        };
        _userRepositoryMock.Setup(r => r.GetByLoginAsync("artem@test.com")).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("artem@test.com")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginViewModel
        {
            LoginOrEmail = "artem@test.com",
            Password = password,
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("artem@test.com", result.Data!.Email);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsNotFound()
    {
        _userRepositoryMock.Setup(r => r.GetByLoginAsync("unknown")).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("unknown")).ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new LoginViewModel
        {
            LoginOrEmail = "unknown",
            Password = "password123",
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordInvalid_ReturnsUnauthorized()
    {
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct"),
            CreatedAt = DateTime.Now,
        };
        _userRepositoryMock.Setup(r => r.GetByLoginAsync("artem")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginViewModel
        {
            LoginOrEmail = "artem",
            Password = "wrong",
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
        Assert.Equal("Неверный пароль", result.Description);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ReturnsConflict()
    {
        _userRepositoryMock.Setup(r => r.ExistByEmailAsync("test@test.com")).ReturnsAsync(true);

        var result = await _sut.RegisterAsync(new RegisterViewModel
        {
            Login = "newuser",
            Email = "test@test.com",
            Password = "password123",
            ConfirmPassword = "password123",
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("Пользователь с таким Email уже существует", result.Description);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenLoginExists_ReturnsConflict()
    {
        _userRepositoryMock.Setup(r => r.ExistByEmailAsync("new@test.com")).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.ExistByLoginAsync("newuser")).ReturnsAsync(true);

        var result = await _sut.RegisterAsync(new RegisterViewModel
        {
            Login = "newuser",
            Email = "new@test.com",
            Password = "password123",
            ConfirmPassword = "password123",
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("Пользователь с таким Login уже существует", result.Description);
    }

    [Fact]
    public async Task RegisterAsync_WhenDataIsValid_CreatesUserAndReturnsOk()
    {
        _userRepositoryMock.Setup(r => r.ExistByEmailAsync("new@test.com")).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.ExistByLoginAsync("newuser")).ReturnsAsync(false);

        var result = await _sut.RegisterAsync(new RegisterViewModel
        {
            Login = "newuser",
            Email = "new@test.com",
            Password = "password123",
            ConfirmPassword = "password123",
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.True(result.Data);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u =>
            u.Login == "newuser" &&
            u.Email == "new@test.com" &&
            BCrypt.Net.BCrypt.Verify("password123", u.PasswordHash))), Times.Once);
    }
}
