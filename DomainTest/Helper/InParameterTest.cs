using System;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    /// <summary>
    /// Tests class InParamer
    /// </summary>
    [TestFixture]
    public class InParameterTest
    {
        private String _value = "value";

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyValueMustBeLargerThanZero()
        {
            int value = 0;
            InParameter.ValueMustBeLargerThanZero(_value, value);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyTimeSpanIsNotBelowZero()
        {
        TimeSpan timeSpan = new TimeSpan(-1,0,0);
            InParameter.TimeSpanCannotBeNegative(_value, timeSpan);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyValueMustPositive()
        {
            int value = -1;
            InParameter.ValueMustBePositive(_value, value);
        }

        /// <summary>
        /// Verifies that parameter is not string.Empty
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotSetStringEmpty()
        {
            string empty = string.Empty;
            InParameter.NotStringEmptyOrNull(_value, empty);
        }

        /// <summary>
        /// Verifies that parameter is not null
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotSetNull()
        {
            InParameter.NotStringEmptyOrNull(_value, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestTimeCannotBeZero()
        {
            InParameter.CheckTimeSpanAtLeastOneTick(_value, new TimeSpan(0));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyBetweenOneAndHundredPercent()
        {
            InParameter.BetweenOneAndHundredPercent(_value, new Percent(0));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyBetweenOneAndHundredPercent1()
        {
            InParameter.BetweenOneAndHundredPercent(_value, new Percent(1.01));
        }

        [Test]
        public void VerifyBetweenOneAndHundredPercent2()
        {
            InParameter.BetweenOneAndHundredPercent(_value, new Percent(0.01));
        }

        [Test]
        public void VerifyBetweenOneAndHundredPercent3()
        {
            InParameter.BetweenOneAndHundredPercent(_value, new Percent(0.99));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyBetweenZeroAndHundredPercent()
        {
            InParameter.BetweenZeroAndHundredPercent(_value, new Percent(-0.01));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyBetweenZeroAndHundredPercent1()
        {
            InParameter.BetweenZeroAndHundredPercent(_value, new Percent(1.01));
        }

        [Test]
        public void VerifyBetweenZeroAndHundredPercent2()
        {
            InParameter.BetweenZeroAndHundredPercent(_value, new Percent(0));
        }

        [Test]
        public void VerifyBetweenZeroAndHundredPercent3()
        {
            InParameter.BetweenZeroAndHundredPercent(_value, new Percent(0.99));
        }

        [Test]
        public void VerifyStringExceedsLimit()
        {
            InParameter.StringTooLong(_value,"abc", 3);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyStringExceedsLimit2()
        {
            InParameter.StringTooLong(_value, "abc", 2);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyVerifyDateIsUtc()
        {
            DateTime dateTime = new DateTime(2008,1,1,0,0,0,DateTimeKind.Local);
            InParameter.VerifyDateIsUtc(_value, dateTime);
        }

        [Test]
        public void VerifyVerifyDateIsUtc2()
        {
            DateTime dateTime = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            InParameter.VerifyDateIsUtc(_value, dateTime);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyVerifyDateIsLocal()
        {
            DateTime dateTime = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            InParameter.VerifyDateIsLocal(_value, dateTime);
        }

        [Test]
        public void VerifyVerifyDateIsLocal2()
        {
            DateTime dateTime = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Local);
            InParameter.VerifyDateIsLocal(_value, dateTime);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyVerifyInParameterMustBeTrueThrowsArgumentexception()
        {
            InParameter.MustBeTrue("parameter", false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), Test]
        public void VerifyVerifyNestedArgumentNull()
        {
            NestedArgumentHolder nestedArgumentHolder = new NestedArgumentHolder();
            nestedArgumentHolder.TheNestedArgument = null;
            ParameterHolder parameterHolder = new ParameterHolder();
            parameterHolder.TheParameter = nestedArgumentHolder;
            Exception exception = null;

            try
            {
                InParameter.NestedArgumentNotNull("TheParameter", "TheNestedArgument", parameterHolder.TheParameter.TheNestedArgument);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<ArgumentException>(exception);
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOf<ArgumentNullException>(exception.InnerException);

            string errorMessage = exception.Message;
            Assert.IsTrue(errorMessage.Contains("TheParameter"));
            Assert.IsTrue(errorMessage.Contains("TheNestedArgument"));

            string innerErrorMessage = exception.InnerException.Message;
            Assert.IsTrue(innerErrorMessage.Contains("TheNestedArgument"));
            Assert.IsFalse(innerErrorMessage.Contains("TheParameter"));
        }

        private class ParameterHolder
        {
            public NestedArgumentHolder TheParameter{ get; set; }
        }

         private class NestedArgumentHolder
        {
            public string TheNestedArgument { get; set; }
        }

    }
}