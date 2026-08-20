namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Represents the result of an operation, encapsulating success or failure.
/// </summary>
public class Result
{
    /// <summary>
    /// Represents the result of an operation, encapsulating success or failure.
    /// </summary>
    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("A result cannot be successful and contain an error.");
            case false when error == Error.None:
                throw new InvalidOperationException("A result cannot be failure and not contain an error.");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    /// <summary>
    /// Indicates whether the operation was successful.
    /// A value of <c>true</c> represents a successful result, while <c>false</c> indicates failure.
    /// </summary>
    protected bool IsSuccess { get; }

    /// <summary>
    /// Gets the error associated with the result of an operation.
    /// Provides details about the nature of the failure when the operation is not successful.
    /// For a successful operation, the value will be <c>Error.None</c>.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Indicates whether the operation was a failure.
    /// A value of <c>true</c> represents a failed result, while <c>false</c> indicates success.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Creates a successful result with no associated error.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being encapsulated in the result.</typeparam>
    /// <param name="value">The value representing a successful result.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing the specified value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failure result with a specified error.
    /// </summary>
    /// <param name="error">The error that describes the reason for the failure.</param>
    /// <returns>A failure <see cref="Result"/> containing the specified error.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with an associated error for a specific value type.
    /// </summary>
    /// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
    /// <param name="error">The error describing the reason for the failure.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> with the specified error.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default!, false, error);

    /// <summary>
    /// Creates a result based on the provided value. If the value is not null, a successful result is created;
    /// otherwise, a failed result is created with an associated null value error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be encapsulated in the result.</typeparam>
    /// <param name="value">The value to be evaluated for creating the result.</param>
    /// <returns>A <see cref="Result{TValue}"/> that is either successful or failed based on the provided value.</returns>
    protected static Result<TValue> Create<TValue>(TValue value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

/// <summary>
/// Represents the outcome of an operation, indicating success or failure.
/// </summary>
public class Result<TValue> : Result
{
    /// <summary>
    /// Represents the result of an operation, encapsulating success or failure.
    /// </summary>
    protected internal Result(TValue value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value associated with the result of an operation.
    /// Accessing this property is only valid for successful results.
    /// Attempts to access the value in a failed result will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    public TValue Value =>
        IsSuccess ? field : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>
    /// Defines an implicit conversion operator that allows a value of type <typeparamref name="TValue"/> to be converted to a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be encapsulated in the result.</typeparam>
    /// <param name="value">The value to be encapsulated in the result.</param>
    /// <returns>A <see cref="Result{TValue}"/> that is either successful if the provided value is not null, or failed with a corresponding error if the value is null.</returns>
    public static implicit operator Result<TValue>(TValue value) => Create(value);
}