using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Helper
{
	/// <summary>
	/// Tests class InParamer
	/// </summary>
	[TestFixture]
	public class InParameterTest
	{
		private const string value = "value";

		[Test]
		public void VerifyValueMustBeLargerThanZero()
		{
			int value = 0;
			Assert.Throws<ArgumentOutOfRangeException>(() => InParameter.ValueMustBeLargerThanZero(InParameterTest.value, value));
		}

		[Test]
		public void VerifyTimeSpanIsNotBelowZero()
		{
			TimeSpan timeSpan = new TimeSpan(-1, 0, 0);
			Assert.Throws<ArgumentOutOfRangeException>(() => InParameter.TimeSpanCannotBeNegative(value, timeSpan));
		}

		[Test]
		public void VerifyValueMustPositive()
		{
			int value = -1;
			Assert.Throws<ArgumentOutOfRangeException>(() => InParameter.ValueMustBePositive(InParameterTest.value, value));
		}

		[Test]
		public void CannotSetStringEmpty()
		{
			string empty = string.Empty;
			Assert.Throws<ArgumentException>(() => InParameter.NotStringEmptyOrNull(value, empty));
		}

		[Test]
		public void CannotSetNull()
		{
			Assert.Throws<ArgumentException>(() => InParameter.NotStringEmptyOrNull(value, null));
		}

		[Test]
		public void ShouldStopWhiteSpace()
		{
			Assert.Throws<ArgumentException>(() => InParameter.NotStringEmptyOrWhiteSpace(value, " "));
		}

		[Test]
		public void ShouldStopEmpty()
		{
			Assert.Throws<ArgumentException>(() => InParameter.NotStringEmptyOrWhiteSpace(value, string.Empty));
		}

		[Test]
		public void ShouldStopNull()
		{
			Assert.Throws<ArgumentException>(() => InParameter.NotStringEmptyOrWhiteSpace(value, null));
		}

		[Test]
		public void TestTimeCannotBeZero()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => InParameter.CheckTimeSpanAtLeastOneTick(value, new TimeSpan(0)));
		}

		[Test]
		public void VerifyBetweenOneAndHundredPercent()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => InParameter.BetweenOneAndHundredPercent(value, new Percent(0)));
		}

		[Test]
		public void VerifyBetweenOneAndHundredPercent1()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => InParameter.BetweenOneAndHundredPercent(value, new Percent(1.01)));
		}

		[Test]
		public void VerifyBetweenOneAndHundredPercent2()
		{
			InParameter.BetweenOneAndHundredPercent(value, new Percent(0.01));
		}

		[Test]
		public void VerifyBetweenOneAndHundredPercent3()
		{
			InParameter.BetweenOneAndHundredPercent(value, new Percent(0.99));
		}

		[Test]
		public void VerifyBetweenZeroAndHundredPercent()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => InParameter.BetweenZeroAndHundredPercent(value, new Percent(-0.01)));
		}

		[Test]
		public void VerifyBetweenZeroAndHundredPercent1()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => InParameter.BetweenZeroAndHundredPercent(value, new Percent(1.01)));
		}

		[Test]
		public void VerifyBetweenZeroAndHundredPercent2()
		{
			InParameter.BetweenZeroAndHundredPercent(value, new Percent(0));
		}

		[Test]
		public void VerifyBetweenZeroAndHundredPercent3()
		{
			InParameter.BetweenZeroAndHundredPercent(value, new Percent(0.99));
		}

		[Test]
		public void VerifyStringExceedsLimit()
		{
			InParameter.StringTooLong(value, "abc", 3);
		}

		[Test]
		public void VerifyStringExceedsLimit2()
		{
			Assert.Throws<ArgumentException>(() => InParameter.StringTooLong(value, "abc", 2));
		}

		[Test]
		public void VerifyVerifyDateIsUtc()
		{
			DateTime dateTime = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Local);
			Assert.Throws<ArgumentException>(() => InParameter.VerifyDateIsUtc(value, dateTime));
		}

		[Test]
		public void VerifyVerifyDateIsUtc2()
		{
			DateTime dateTime = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			InParameter.VerifyDateIsUtc(value, dateTime);
		}

		[Test]
		public void VerifyVerifyInParameterMustBeTrueThrowsArgumentexception()
		{
			Assert.Throws<ArgumentException>(() => InParameter.MustBeTrue("parameter", false));
		}
	}
}