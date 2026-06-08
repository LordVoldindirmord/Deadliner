using Deadliner.Service.Implementations;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTests.Services;

public class EmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_WhenSmtpPortIsMissing_ThrowsArgumentNullException()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "smtp.test.com",
            ["Smtp:Username"] = "user@test.com",
            ["Smtp:Password"] = "password",
        });
        var sut = new EmailService(configuration);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.SendEmailAsync("recipient@test.com", "Subject", "<p>Body</p>"));
    }

    [Fact]
    public async Task SendEmailAsync_WhenSmtpHostIsMissing_ThrowsArgumentNullException()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Smtp:Port"] = "587",
            ["Smtp:Username"] = "user@test.com",
            ["Smtp:Password"] = "password",
        });
        var sut = new EmailService(configuration);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.SendEmailAsync("recipient@test.com", "Subject", "<p>Body</p>"));
    }

    [Fact]
    public async Task SendEmailAsync_WhenSmtpServerIsUnreachable_ThrowsException()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "127.0.0.1",
            ["Smtp:Port"] = "1",
            ["Smtp:Username"] = "user@test.com",
            ["Smtp:Password"] = "password",
        });
        var sut = new EmailService(configuration);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            sut.SendEmailAsync("recipient@test.com", "Subject", "<p>Body</p>"));
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(c => c[It.IsAny<string>()])
            .Returns((string key) => values.GetValueOrDefault(key));
        return configurationMock.Object;
    }
}
