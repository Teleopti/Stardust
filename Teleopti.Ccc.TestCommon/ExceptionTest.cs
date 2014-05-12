using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.TestCommon
{
	/// <summary>
    /// Base class for tests running against
    /// own implemented exceptions
    /// </summary>
    public abstract class ExceptionTest<T> where T : Exception, new()
    {
        private T ex;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            ex = new T();
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        protected abstract T CreateTestInstance(string message, Exception innerException);

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected abstract T CreateTestInstance(string message);

        /// <summary>
        /// Verifies that the default constructor works.
        /// </summary>
        [Test]
        public void VerifyDefaultConstructorWorks()
        {
            Assert.IsNotNull(ex);
        }

        /// <summary>
        /// Verifies that the message and inner constructor works.
        /// </summary>
        [Test]
        public void VerifyMessageAndInnerConstructorWorks()
        {
            const string mess = "test";
            T outer = CreateTestInstance(mess, ex);

            Assert.AreSame(ex, outer.InnerException);
            StringAssert.StartsWith(mess, outer.Message);
        }

        [Test]
        public void VerifyMessageConstructorWorks()
        {
            const string mess = "test";
            T outer = CreateTestInstance(mess);
            StringAssert.StartsWith(mess, outer.Message);
        }

        /// <summary>
        /// Verifies the protected constructor works.
        /// </summary>
        /// <remarks>
        /// No ccc-specific data is serialized right now...
        /// </remarks>
        [Test]
        public virtual void VerifyProtectedConstructorWorks()
        {
            const string exString = "testing...";
            T exOrg = CreateTestInstance(exString);
            var result = SerializationHelper.SerializeAsBinary(exOrg);
            T exSerialized = SerializationHelper.Deserialize<T>(result);
            Assert.IsNotNull(exSerialized);
            StringAssert.StartsWith(exString, exSerialized.Message);
        }
    }
}