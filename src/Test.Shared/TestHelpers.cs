namespace Test.Shared
{
    using System;
    using System.Threading.Tasks;
    using Touchstone.Core;

    /// <summary>
    /// Assertion and descriptor helpers shared by all RegexMatcher test suites.
    /// Assertions throw on failure; no console output is produced.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Build a test case descriptor from a synchronous action.
        /// </summary>
        /// <param name="suiteId">Suite identifier.</param>
        /// <param name="caseId">Case identifier, unique within the suite.</param>
        /// <param name="displayName">Human-readable name.</param>
        /// <param name="action">Test body; throws on failure.</param>
        /// <returns>Test case descriptor.</returns>
        public static TestCaseDescriptor Case(string suiteId, string caseId, string displayName, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            return new TestCaseDescriptor(
                suiteId: suiteId,
                caseId: caseId,
                displayName: displayName,
                executeAsync: ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    action();
                    return Task.CompletedTask;
                });
        }

        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Failure message.</param>
        /// <exception cref="InvalidOperationException">Thrown when the condition is false.</exception>
        public static void True(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Failure message.</param>
        /// <exception cref="InvalidOperationException">Thrown when the condition is true.</exception>
        public static void False(bool condition, string message)
        {
            if (condition) throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Assert that two values are equal using object.Equals.
        /// </summary>
        /// <param name="expected">Expected value.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="message">Failure message context.</param>
        /// <exception cref="InvalidOperationException">Thrown when the values differ.</exception>
        public static void Equal(object? expected, object? actual, string message)
        {
            if (!Object.Equals(expected, actual))
                throw new InvalidOperationException(
                    message + " (expected: " + Describe(expected) + ", actual: " + Describe(actual) + ")");
        }

        /// <summary>
        /// Assert that an action throws an exception of exactly the specified type.
        /// </summary>
        /// <typeparam name="TException">Expected exception type.</typeparam>
        /// <param name="action">Action to invoke.</param>
        /// <param name="message">Failure message context.</param>
        /// <returns>The thrown exception.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no exception or a different exception type is thrown.</exception>
        public static TException Throws<TException>(Action action, string message) where TException : Exception
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            try
            {
                action();
            }
            catch (Exception ex) when (ex.GetType() == typeof(TException))
            {
                return (TException)ex;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    message + " (expected " + typeof(TException).Name + ", got " + ex.GetType().Name + ": " + ex.Message + ")", ex);
            }

            throw new InvalidOperationException(
                message + " (expected " + typeof(TException).Name + ", no exception thrown)");
        }

        private static string Describe(object? val)
        {
            if (val == null) return "null";
            return val.ToString() + " [" + val.GetType().Name + "]";
        }
    }
}
