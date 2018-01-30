using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.DomainTest.Forecasting.Import
{
    [TestFixture]
    public class ForecastsRowExtractorTest
    {
        private IForecastsRowExtractor _target;

        [SetUp]
        public void Setup()
        {
            _target = new ForecastsRowExtractor();
        }

        [Test]
        public void ShouldExtractRowFromString()
        {
            const string rowString = "Insurance,20120326 02:00,20120326 02:15,17,179,0,4.05";
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var row = _target.Extract(rowString, timeZone);

            Assert.That(row.Tasks, Is.EqualTo(17));
            Assert.That(row.TaskTime, Is.EqualTo(179));
            Assert.That(row.AfterTaskTime, Is.EqualTo(0));
            Assert.That(row.Agents, Is.EqualTo(4.05));
            Assert.That(row.LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 26, 2, 0, 0)));
            Assert.That(row.LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 26, 2, 15, 0)));
            Assert.That(row.UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 26, 0, 0, 0)));
            Assert.That(row.UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 26, 0, 15, 0)));
        }

        [Test]
        public void ShouldDetectInvalidTime()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            const string rowString = "Insurance,201203025 02:00,20120325 02:15,17,179,0,4.05";
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				"Date time format of 201203025 02:00 is wrong");
        }

        [Test]
        public void ShouldThrowErrorIfContentAreNotOk()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            const string rowString = "Insurance";
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				"There are more or less columns than expected");
        }

        [Test]
        public void ShouldThrowErrorIfCannotParseString()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            const string rowString = ",20120326 02:00,20120326 02:15,17,179,0,4.05";
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				"Skill name should not be empty");
        }

        [Test]
        public void ShouldThrowErrorIfCannotParseDate()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            const string rowString = "Insurance,20120326 02:00,,17,179,0,4.05";
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				"Date time format of  is wrong");
        }

        [Test]
        public void ShouldThrowErrorIfCannotParseStartDate()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            const string rowString = "Insurance,,20120326 02:00,17,179,0,4.05";
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				"Date time format of  is wrong");
        }

        [Test]
        public void ShouldThrowErrorIfCannotParseInteger()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            const string rowString = "Insurance,20120326 02:00,20120326 02:00,,179,0,4.05";
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				" should be an integer");
        }

        [Test]
        public void ShouldThrowErrorIfCannotParseDouble()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            const string rowString = "Insurance,20120326 02:00,20120326 02:00,17,,0,4.05";
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				" should be an double");
        }
		
		[Test]
		public void ShouldHandleSemicolonAsTokenSeparator()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			const string rowString = "Insurance;20120326 02:00;20120326 02:00;17;0.5;0;4.05";
			_target.PresetTokenSeparator(rowString);
			 _target.Extract(rowString, timeZone);
		}
		
		[Test]
		public void ShouldThrowIfTokenSeparatorsIsWrong()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			const string rowString =  @"Insurance,20120326 02:00,20120326 02:00,17,0.5,0,4.05";
			
			_target.PresetTokenSeparator(";");
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				"There are more or less columns than expected");
		}
		
		[Test]
		public void ShouldHandleAllValuesBeingDoubles()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			const string rowString =  @"Insurance,20120330 02:00,20120330 02:15,1.2,0.5,0.7,4.05";

			var forecastRow = _target.Extract(rowString, timeZone);

			Assert.That(forecastRow.SkillName, Is.EqualTo("Insurance"));
			Assert.That(forecastRow.LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 30, 2, 0, 0)));
			Assert.That(forecastRow.LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 30, 2, 15, 0)));
			Assert.That(forecastRow.UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 30, 0, 0, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 30, 0, 15, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.Tasks, Is.EqualTo(1.2));
			Assert.That(forecastRow.TaskTime, Is.EqualTo(0.5));
			Assert.That(forecastRow.AfterTaskTime, Is.EqualTo(0.7));
			Assert.That(forecastRow.Agents, Is.EqualTo(4.05));
		}
		
		[Test]
		public void ShouldHandleCommaAsDecimalSeparatorWhenUsingSemiAsTokenSeparator()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			const string rowString =  @"Insurance;20120330 02:00;20120330 02:15;1,2;0,5;0,7;4,05";

			_target.PresetTokenSeparator(rowString);
			var forecastRow = _target.Extract(rowString, timeZone);

			Assert.That(forecastRow.SkillName, Is.EqualTo("Insurance"));
			Assert.That(forecastRow.LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 30, 2, 0, 0)));
			Assert.That(forecastRow.LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 30, 2, 15, 0)));
			Assert.That(forecastRow.UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 30, 0, 0, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 30, 0, 15, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.Tasks, Is.EqualTo(1.2));
			Assert.That(forecastRow.TaskTime, Is.EqualTo(0.5));
			Assert.That(forecastRow.AfterTaskTime, Is.EqualTo(0.7));
			Assert.That(forecastRow.Agents, Is.EqualTo(4.05));
		}
		
		[Test]
		public void ShouldHandleBothCommaAndPointAsDecimalSeparators()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			const string rowString =  @"Insurance;20120330 02:00;20120330 02:15;1,2;0.5;0,7;4.05";

			_target.PresetTokenSeparator(rowString);
			var forecastRow = _target.Extract(rowString, timeZone);

			Assert.That(forecastRow.SkillName, Is.EqualTo("Insurance"));
			Assert.That(forecastRow.LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 30, 2, 0, 0)));
			Assert.That(forecastRow.LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 30, 2, 15, 0)));
			Assert.That(forecastRow.UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 30, 0, 0, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 30, 0, 15, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.Tasks, Is.EqualTo(1.2));
			Assert.That(forecastRow.TaskTime, Is.EqualTo(0.5));
			Assert.That(forecastRow.AfterTaskTime, Is.EqualTo(0.7));
			Assert.That(forecastRow.Agents, Is.EqualTo(4.05));
		}
		
		[Test]
		public void ShouldThrowWhenUsingPointAsThousandSeparator()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			const string rowString =  @"Insurance;20120330 02:00;20120330 02:15;1.7;1.200,5;0,7;4.05";

			_target.PresetTokenSeparator(rowString);
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				" should be an double");
		}
		
		[Test]
		public void ShouldThrowWhenUsingCommaAsThousandSeparator()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			const string rowString =  @"Insurance;20120330 02:00;20120330 02:15;1.7;1,200,.5;0,7;4.05";

			_target.PresetTokenSeparator(rowString);
			Assert.Throws<ValidationException>(() => _target.Extract(rowString, timeZone),
				" should be an double");
		}
    }
}
