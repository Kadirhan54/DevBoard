using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Infrastructure.Messaging.Configuration
{
    /// <summary>
    /// Defines which exceptions should be retried and which should go to error queue immediately
    /// </summary>
    public static class ExceptionFilters
    {
        /// <summary>
        /// Transient errors that are safe to retry
        /// </summary>
        public static readonly Type[] RetryableExceptions = new[]
        {
        typeof(TimeoutException),
        typeof(HttpRequestException),
        typeof(DbUpdateConcurrencyException),
        typeof(DbException),
        typeof(IOException)
    };

        /// <summary>
        /// Fatal errors that should NOT be retried (business logic errors, validation, etc.)
        /// </summary>
        public static readonly Type[] NonRetryableExceptions = new[]
        {
        typeof(ArgumentNullException),
        typeof(ArgumentException),
        typeof(InvalidOperationException),
        typeof(UnauthorizedAccessException),
        typeof(NotImplementedException)
    };

        public static bool IsRetryable(Exception exception)
        {
            var exceptionType = exception.GetType();

            // Check if it's explicitly non-retryable
            if (NonRetryableExceptions.Contains(exceptionType))
                return false;

            // Check if it's explicitly retryable
            if (RetryableExceptions.Contains(exceptionType))
                return true;

            // Default: retry unknown exceptions (can be changed based on your preference)
            return true;
        }
    }
}
