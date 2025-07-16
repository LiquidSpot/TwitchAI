#pragma warning disable CS1591
namespace TwitchAI.Domain.Enums.ErrorCodes;

/// <summary>
/// Base error codes for the application.
/// </summary>
public enum BaseErrorCodes
{
    #region Common/Base Errors (-10500 .. -10999)

    InternalServerError = 10500,
    IncorrectRequest,
    OperationProcessError,
    ValidationProcessError,
    DataNotFound,
    DatabaseSaveError,
    ConnectionTimeout,
    DefaultError,
    EmptyError,
    NoData,
    WrongCode,
    WrongDateFormat,
    WrongLink,
    WrongRequestFormat,
    RequestExpired,
    SidIsNull,
    NotFound,
    NotImplemented,
    CancellationTokenRequested = 10999,
    #endregion
}