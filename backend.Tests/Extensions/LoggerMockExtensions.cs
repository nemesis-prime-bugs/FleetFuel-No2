using Microsoft.Extensions.Logging;
using Moq;

namespace FleetFuel.Api.Tests.Extensions;

/// <summary>
/// Extension methods for testing logger interactions.
/// </summary>
public static class LoggerMockExtensions
{
    public static void VerifyLogInformation<T>(
        this Mock<ILogger<T>> logger,
        string expectedMessage,
        Times times)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v?.ToString()?.Contains(expectedMessage) ?? false),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    public static void VerifyLogWarning<T>(
        this Mock<ILogger<T>> logger,
        string expectedMessage,
        Times times)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v?.ToString()?.Contains(expectedMessage) ?? false),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    public static void VerifyLogError<T>(
        this Mock<ILogger<T>> logger,
        string expectedMessage,
        Times times)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v?.ToString()?.Contains(expectedMessage) ?? false),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}