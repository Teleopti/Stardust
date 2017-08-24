using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.DomainTest.Forecasting.Import
{
	[TestFixture]
	public class ForecastsFileContentProviderTest
	{
		private IForecastsFileContentProvider _target;
		private byte[] _fileContent;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void Setup()
		{
			_timeZone = (TimeZoneInfo.Utc);
			_target = new ForecastsFileContentProvider(new ForecastsRowExtractor());
		}

		[Test]
		public void ShouldHandleWrongFileStream()
		{
			_fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,");

			Assert.Throws<ValidationException>(() => _target.LoadContent(_fileContent, _timeZone));
		}

		[Test]
		public void ShouldLoadContentFromFileStream()
		{
			_fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,4.05");
			var forecastRow = _target.LoadContent(_fileContent, _timeZone).First();

			Assert.That(forecastRow.TaskTime, Is.EqualTo(179));
			Assert.That(forecastRow.AfterTaskTime, Is.EqualTo(0));
			Assert.That(forecastRow.Agents, Is.EqualTo(4.05));
			Assert.That(forecastRow.LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 1, 12, 45, 0)));
			Assert.That(forecastRow.LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 1, 13, 0, 0)));
			Assert.That(forecastRow.SkillName, Is.EqualTo("Insurance"));
			Assert.That(forecastRow.Tasks, Is.EqualTo(17));
			Assert.That(forecastRow.UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 1, 12, 45, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 1, 13, 0, 0, DateTimeKind.Utc)));
		}

		[Test]
		public void ShouldImportWinterTime()
		{
			_fileContent = Encoding.UTF8.GetBytes("Insurance,20121028 02:00,20121028 02:15,17,179,0,4.05\r\nInsurance,20121028 02:45,20121028 03:00,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();

			Assert.That(forecastRows.Length, Is.EqualTo(4));
			Assert.That(forecastRows[0].LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 2, 0, 0)));
			Assert.That(forecastRows[0].LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 2, 15, 0)));
			Assert.That(forecastRows[0].UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 0, 0, 0)));
			Assert.That(forecastRows[0].UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 0, 15, 0)));
			Assert.That(forecastRows[1].LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 2, 0, 0)));
			Assert.That(forecastRows[1].LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 2, 15, 0)));
			Assert.That(forecastRows[1].UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 1, 0, 0)));
			Assert.That(forecastRows[1].UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 1, 15, 0)));
			Assert.That(forecastRows[2].LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 2, 45, 0)));
			Assert.That(forecastRows[2].LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 3, 0, 0)));
			Assert.That(forecastRows[2].UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 0, 45, 0)));
			Assert.That(forecastRows[2].UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 1, 0, 0)));
			Assert.That(forecastRows[3].LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 2, 45, 0)));
			Assert.That(forecastRows[3].LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 3, 0, 0)));
			Assert.That(forecastRows[3].UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 1, 45, 0)));
			Assert.That(forecastRows[3].UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 2, 0, 0)));
		}

		[Test]
		public void ShouldImportWinterTimeForAmbiguousTime()
		{
			_fileContent = Encoding.UTF8.GetBytes("Insurance,20121028 01:45,20121028 02:00,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();

			Assert.That(forecastRows.Length, Is.EqualTo(1));
			Assert.That(forecastRows[0].LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 28, 1, 45, 0)));
			Assert.That(forecastRows[0].LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 2, 0, 0)));
			Assert.That(forecastRows[0].UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 10, 27, 23, 45, 0)));
			Assert.That(forecastRows[0].UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 10, 28, 0, 0, 0)));
		}

		[Test]
		public void ShouldHandleFileWithEncodingUtfWithByteOrderMark()
		{
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms, new UTF8Encoding(true)))
			{
				sw.Write("Insurance,20121028 01:45,20121028 02:00,17,179,0,4.05");
				sw.Flush();
				_fileContent = ms.ToArray();
			}
			var forecastRows = _target.LoadContent(_fileContent, _timeZone).ToArray();
			forecastRows[0].SkillName.Should().Be.EqualTo("Insurance");
		}
	}
}
