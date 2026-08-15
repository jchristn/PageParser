namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Minimal, framework-agnostic assertion helpers used by the Touchstone
    /// test descriptors. A failed assertion throws, which Touchstone interprets
    /// as a failing test case regardless of the host runner (CLI, xUnit, NUnit).
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        public static void True(bool condition, string message)
        {
            if (!condition) throw new TouchstoneAssertException(message);
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        public static void False(bool condition, string message)
        {
            if (condition) throw new TouchstoneAssertException(message);
        }

        /// <summary>
        /// Assert that two values are equal.
        /// </summary>
        public static void Equal<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new TouchstoneAssertException(
                    (message ?? "Values are not equal")
                    + " (expected: '" + Describe(expected) + "', actual: '" + Describe(actual) + "')");
            }
        }

        /// <summary>
        /// Assert that a reference is null.
        /// </summary>
        public static void Null(object value, string message)
        {
            if (value != null) throw new TouchstoneAssertException(message + " (expected null, actual: '" + Describe(value) + "')");
        }

        /// <summary>
        /// Assert that a reference is not null.
        /// </summary>
        public static void NotNull(object value, string message)
        {
            if (value == null) throw new TouchstoneAssertException(message + " (expected non-null, actual: null)");
        }

        /// <summary>
        /// Assert that a string is null or empty.
        /// </summary>
        public static void NullOrEmpty(string value, string message)
        {
            if (!String.IsNullOrEmpty(value)) throw new TouchstoneAssertException(message + " (actual: '" + value + "')");
        }

        /// <summary>
        /// Assert that a collection contains an expected item.
        /// </summary>
        public static void Contains<T>(T expected, IEnumerable<T> collection, string message)
        {
            if (collection == null || !collection.Contains(expected))
                throw new TouchstoneAssertException(message + " (missing: '" + Describe(expected) + "')");
        }

        /// <summary>
        /// Assert that a collection does not contain a given item.
        /// </summary>
        public static void DoesNotContain<T>(T unexpected, IEnumerable<T> collection, string message)
        {
            if (collection != null && collection.Contains(unexpected))
                throw new TouchstoneAssertException(message + " (unexpectedly present: '" + Describe(unexpected) + "')");
        }

        /// <summary>
        /// Assert that a string contains an expected substring.
        /// </summary>
        public static void StringContains(string substring, string actual, string message)
        {
            if (actual == null || actual.IndexOf(substring, StringComparison.Ordinal) < 0)
                throw new TouchstoneAssertException(message + " (substring '" + substring + "' not found in '" + actual + "')");
        }

        /// <summary>
        /// Assert that a string does not contain a given substring.
        /// </summary>
        public static void StringDoesNotContain(string substring, string actual, string message)
        {
            if (actual != null && actual.IndexOf(substring, StringComparison.Ordinal) >= 0)
                throw new TouchstoneAssertException(message + " (substring '" + substring + "' unexpectedly found in '" + actual + "')");
        }

        /// <summary>
        /// Assert that the supplied action throws an exception of type
        /// <typeparamref name="TException"/> (or a derived type).
        /// </summary>
        public static void Throws<TException>(Action action, string message) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new TouchstoneAssertException(
                    message + " (expected " + typeof(TException).Name + " but got " + ex.GetType().Name + ": " + ex.Message + ")");
            }

            throw new TouchstoneAssertException(message + " (expected " + typeof(TException).Name + " but no exception was thrown)");
        }

        private static string Describe(object value)
        {
            if (value == null) return "null";
            return value.ToString();
        }
    }

    /// <summary>
    /// Exception thrown when a <see cref="Check"/> assertion fails.
    /// </summary>
    public sealed class TouchstoneAssertException : Exception
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="message">Failure message.</param>
        public TouchstoneAssertException(string message) : base(message)
        {
        }
    }
}
