using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using Deadliner.Domain.ViewModel.Account;
using Deadliner.Service.Implementations;
using Deadliner.Service.Interfaces;
using Moq;

namespace UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUserTokenRepository> _userTokenRepositoryMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(
            _userRepositoryMock.Object,
            _userTokenRepositoryMock.Object,
            _emailServiceMock.Object);
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
            EmailConfirmed = true,
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
            EmailConfirmed = true,
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
    public async Task LoginAsync_WhenEmailNotConfirmed_ReturnsConflict()
    {
        const string password = "password123";
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            EmailConfirmed = false,
            CreatedAt = DateTime.Now,
        };
        _userRepositoryMock.Setup(r => r.GetByLoginAsync("artem")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginViewModel
        {
            LoginOrEmail = "artem",
            Password = password,
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("У вас не активный Email, активируйте его!", result.Description);
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
            EmailConfirmed = true,
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
    public async Task RegisterAsync_WhenDataIsValid_CreatesUserSendsTokenAndEmail()
    {
        _userRepositoryMock.Setup(r => r.ExistByEmailAsync("new@test.com")).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.ExistByLoginAsync("newuser")).ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => u.Id = 1)
            .Returns(Task.CompletedTask);

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
        Assert.Equal("На ваш email отправлена ссылка для подтверждения.", result.Description);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u =>
            u.Login == "newuser" &&
            u.Email == "new@test.com" &&
            u.EmailConfirmed == false &&
            BCrypt.Net.BCrypt.Verify("password123", u.PasswordHash))), Times.Once);
        _userTokenRepositoryMock.Verify(r => r.CreateAsync(It.Is<UserToken>(t =>
            t.UserId == 1 &&
            t.TokenType == UserTokenType.EmailConfirm &&
            !t.IsUsed &&
            !string.IsNullOrWhiteSpace(t.Token))), Times.Once);
        _emailServiceMock.Verify(s => s.SendEmailAsync(
            "new@test.com",
            "Подтверждение регистрации",
            It.Is<string>(body => body.Contains("ConfirmEmail"))), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenTokenNotFound_ReturnsNotFound()
    {
        _userTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("invalid"))
            .ReturnsAsync((UserToken?)null);

        var result = await _sut.ConfirmEmailAsync("invalid");

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
        Assert.Equal("Токен не найден", result.Description);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenTokenExpired_ReturnsBadRequest()
    {
        var userToken = new UserToken
        {
            Id = 1,
            UserId = 1,
            Token = "expired",
            TokenType = UserTokenType.EmailConfirm,
            ExpiresAt = DateTime.Now.AddHours(-1),
            IsUsed = false,
        };
        _userTokenRepositoryMock.Setup(r => r.GetByTokenAsync("expired")).ReturnsAsync(userToken);

        var result = await _sut.ConfirmEmailAsync("expired");

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Срок действия токена истёк", result.Description);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenTokenValid_ConfirmsEmail()
    {
        var userToken = new UserToken
        {
            Id = 1,
            UserId = 1,
            Token = "valid",
            TokenType = UserTokenType.EmailConfirm,
            ExpiresAt = DateTime.Now.AddHours(1),
            IsUsed = false,
        };
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            EmailConfirmed = false,
        };
        _userTokenRepositoryMock.Setup(r => r.GetByTokenAsync("valid")).ReturnsAsync(userToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.ConfirmEmailAsync("valid");

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.True(result.Data);
        Assert.True(user.EmailConfirmed);
        Assert.True(userToken.IsUsed);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _userTokenRepositoryMock.Verify(r => r.UpdateAsync(userToken), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenUserNotFound_ReturnsNotFound()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("missing@test.com"))
            .ReturnsAsync((User?)null);

        var result = await _sut.ForgotPasswordAsync("missing@test.com");

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
        _emailServiceMock.Verify(s => s.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenUserExists_SendsResetEmail()
    {
        var user = new User { Id = 1, Login = "artem", Email = "artem@test.com", EmailConfirmed = true };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("artem@test.com")).ReturnsAsync(user);

        var result = await _sut.ForgotPasswordAsync("artem@test.com");

        Assert.True(result.IsSuccess);
        Assert.Equal("Ссылка для сброса пароля отправлена на ваш email.", result.Description);
        _userTokenRepositoryMock.Verify(r => r.CreateAsync(It.Is<UserToken>(t =>
            t.UserId == 1 &&
            t.TokenType == UserTokenType.PasswordReset &&
            !t.IsUsed)), Times.Once);
        _emailServiceMock.Verify(s => s.SendEmailAsync(
            "artem@test.com",
            "Сброс пароля",
            It.Is<string>(body => body.Contains("ResetPassword"))), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenValid_UpdatesPassword()
    {
        const string newPassword = "newPassword123";

        var userToken = new UserToken
        {
            Id = 1,
            UserId = 1,
            Token = "reset",
            TokenType = UserTokenType.PasswordReset,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            IsUsed = false,
        };
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("old"),
            EmailConfirmed = true,
        };
        _userTokenRepositoryMock.Setup(r => r.GetByTokenAsync("reset")).ReturnsAsync(userToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.ResetPasswordAsync("reset", newPassword);

        Assert.True(result.IsSuccess);
        Assert.Equal("Пароль успешно изменён.", result.Description);
        Assert.True(userToken.IsUsed);
        Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash));
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _userTokenRepositoryMock.Verify(r => r.UpdateAsync(userToken), Times.Once);
    }

    [Fact]
    public async Task ResendConfirmationAsync_WhenEmailAlreadyConfirmed_ReturnsConflict()
    {
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            EmailConfirmed = true,
        };
        _userRepositoryMock.Setup(r => r.GetByLoginAsync("artem")).ReturnsAsync(user);

        var result = await _sut.ResendConfirmationAsync("artem");

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("Email уже подтверждён", result.Description);
        _emailServiceMock.Verify(s => s.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResendConfirmationAsync_WhenEmailNotConfirmed_SendsEmail()
    {
        var user = new User
        {
            Id = 1,
            Login = "artem",
            Email = "artem@test.com",
            EmailConfirmed = false,
        };
        _userRepositoryMock.Setup(r => r.GetByLoginAsync("artem")).ReturnsAsync(user);

        var result = await _sut.ResendConfirmationAsync("artem");

        Assert.True(result.IsSuccess);
        Assert.Equal("Письмо отправлено повторно.", result.Description);
        _userTokenRepositoryMock.Verify(r => r.CreateAsync(It.Is<UserToken>(t =>
            t.UserId == 1 &&
            t.TokenType == UserTokenType.EmailConfirm)), Times.Once);
        _emailServiceMock.Verify(s => s.SendEmailAsync(
            "artem@test.com",
            "Подтверждение регистрации",
            It.IsAny<string>()), Times.Once);
    }
}
