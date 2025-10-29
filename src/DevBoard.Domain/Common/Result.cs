//using DevBoard.Domain.Enums;
//using System;

//namespace DevBoard.Domain.Common
//{
//    public class Result
//    {
//        public bool IsSuccess { get; }
//        public string? Error { get; }
//        public ErrorType ErrorType { get; }

//        public bool IsFailure => !IsSuccess;

//        protected Result(bool isSuccess, string? error, ErrorType errorType = ErrorType.None)
//        {
//            if (isSuccess && error != null)
//                throw new InvalidOperationException("Successful result cannot have an error message.");
//            if (!isSuccess && error == null)
//                throw new InvalidOperationException("Failed result must have an error message.");

//            IsSuccess = isSuccess;
//            Error = error;
//            ErrorType = errorType;
//        }

//        // ✅ Explicitly passing ErrorType.None for clarity
//        public static Result Success() => new(true, null, ErrorType.None);

//        public static Result Failure(string error, ErrorType errorType = ErrorType.Unexpected) =>
//            new(false, error, errorType);
//    }

//    public class Result<T> : Result
//    {
//        public T? Value { get; }

//        protected internal Result(T? value, bool isSuccess, string? error, ErrorType errorType = ErrorType.None)
//            : base(isSuccess, error, errorType)
//        {
//            Value = value;
//        }

//        // ✅ Explicitly passing ErrorType.None for clarity
//        public static Result<T> Success(T value) => new(value, true, null, ErrorType.None);

//        public static new Result<T> Failure(string error, ErrorType errorType = ErrorType.Unexpected) =>
//            new(default, false, error, errorType);
//    }
//}
