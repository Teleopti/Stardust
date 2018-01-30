using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
		public void ShouldThrowOnEmptyForecastFile()
		{
			_fileContent = Encoding.UTF8.GetBytes("");

			Assert.Throws<ValidationException>(() => _target.LoadContent(_fileContent, _timeZone), "empty");
		}

		[Test]
		public void ShouldThrowOnWrongFileStream()
		{
			_fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,");

			Assert.Throws<ValidationException>(() => _target.LoadContent(_fileContent, _timeZone));
		}

		[Test]
		public void ShouldLoadContentFromFileStream()
		{
			_fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,2,4.05");
			var forecastRow = _target.LoadContent(_fileContent, _timeZone).First();

			Assert.That(forecastRow.TaskTime, Is.EqualTo(179));
			Assert.That(forecastRow.AfterTaskTime, Is.EqualTo(2));
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

		[Test]
		public void ShouldHandleHeaders()
		{
			_fileContent = Encoding.UTF8.GetBytes(@"skillname,startdatetime,enddatetime,tasks,tasktime,aftertasktime,agents
Insurance,20121028 02:00,20121028 02:15,17,179,0,4.05
Insurance,20121028 02:45,20121028 03:00,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();
		}
		
		[Test]
		public void ShouldHandleIsoDate() //"yyyy-MM-dd H:mm"
		{
			_fileContent = Encoding.UTF8.GetBytes(@"Insurance,2012-10-28 02:00,2012-10-28 02:15,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();
			forecastRows.First().UtcDateTimeFrom.Should().Be.EqualTo(new DateTime(2012,10,28,0,0,0));
			forecastRows.First().UtcDateTimeTo.Should().Be.EqualTo(new DateTime(2012,10,28,0,15,0));
		}
		
		[Test]
		[SetCulture("en-US")]
		public void ShouldHandleIsoDateTimeFormatInUs()
		{
			var culture = Thread.CurrentThread.CurrentCulture;
			_fileContent = Encoding.UTF8.GetBytes(@"Insurance,2012-10-28 02:00,2012-10-28 02:15,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();
			forecastRows.First().UtcDateTimeFrom.Should().Be.EqualTo(new DateTime(2012,10,28,0,0,0));
			forecastRows.First().UtcDateTimeTo.Should().Be.EqualTo(new DateTime(2012,10,28,0,15,0));
		}
		
		[Test]
		[SetCulture("fr-FR")]
		public void ShouldHandleIsoDateTimeFormatInFrance()
		{
			_fileContent = Encoding.UTF8.GetBytes(@"Insurance,2012-10-28 02:00,2012-10-28 02:15,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();
			forecastRows.First().UtcDateTimeFrom.Should().Be.EqualTo(new DateTime(2012,10,28,0,0,0));
			forecastRows.First().UtcDateTimeTo.Should().Be.EqualTo(new DateTime(2012,10,28,0,15,0));
		}
		
		[Test]
		public void ShouldHandleOldDateTimeFormat() //"yyyyMMdd H:mm"
		{
			_fileContent = Encoding.UTF8.GetBytes(@"Insurance,20121028 02:00,20121028 02:15,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();
			forecastRows.First().UtcDateTimeFrom.Should().Be.EqualTo(new DateTime(2012,10,28,0,0,0));
			forecastRows.First().UtcDateTimeTo.Should().Be.EqualTo(new DateTime(2012,10,28,0,15,0));
		}
		
		[Test]
		public void ShouldHandleOldDateTimeFormatShortTime() //"yyyyMMdd H:mm"
		{
			_fileContent = Encoding.UTF8.GetBytes(@"Insurance,20121028 2:00,20121028 2:15,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();
			forecastRows.First().UtcDateTimeFrom.Should().Be.EqualTo(new DateTime(2012,10,28,0,0,0));
			forecastRows.First().UtcDateTimeTo.Should().Be.EqualTo(new DateTime(2012,10,28,0,15,0));
		}
		
		[Test]
		public void ShouldHandleSemicolonAsSeparator()
		{
			_fileContent = Encoding.UTF8.GetBytes(@"skillname;startdatetime;enddatetime;tasks;tasktime;aftertasktime;agents
Insurance;20121027 02:00;20121027 02:15;17;179;0;4.05
Insurance;20121027 02:45;20121027 03:00;17;179;0;4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var forecastRows = _target.LoadContent(_fileContent, timeZone).ToArray();
			forecastRows.Count().Should().Be.EqualTo(2);	
			forecastRows[0].UtcDateTimeFrom.Should().Be.EqualTo(new DateTime(2012,10,27,0,0,0));
			forecastRows[0].UtcDateTimeTo.Should().Be.EqualTo(new DateTime(2012,10,27,0,15,0));
			forecastRows[1].UtcDateTimeFrom.Should().Be.EqualTo(new DateTime(2012,10,27,0,45,0));
			forecastRows[1].UtcDateTimeTo.Should().Be.EqualTo(new DateTime(2012,10,27,1,0,0));
		}
		
		[Test]
		public void ShouldThrowWhenMixingTokenSeparator()
		{
			_fileContent = Encoding.UTF8.GetBytes(@"skillname;startdatetime;enddatetime;tasks;tasktime;aftertasktime;agents
Insurance;20121027 02:00;20121027 02:15;17;179;0;4.05
Insurance,20121027 02:45,20121027 03:00,17,179,0,4.05");
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			Assert.Throws<ValidationException>(() => _target.LoadContent(_fileContent, _timeZone),
				"There are more or less columns than expected");
		}
		
		[Test]
		public void ShouldHandleDoubleValuesForAll()
		{
			_fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17.1,179.1,2.1,4.05");
			var forecastRow = _target.LoadContent(_fileContent, _timeZone).First();

			Assert.That(forecastRow.SkillName, Is.EqualTo("Insurance"));
			Assert.That(forecastRow.LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 1, 12, 45, 0)));
			Assert.That(forecastRow.LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 1, 13, 0, 0)));
			Assert.That(forecastRow.UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 1, 12, 45, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 1, 13, 0, 0, DateTimeKind.Utc)));
			Assert.That(forecastRow.Tasks, Is.EqualTo(17.1));
			Assert.That(forecastRow.TaskTime, Is.EqualTo(179.1));
			Assert.That(forecastRow.AfterTaskTime, Is.EqualTo(2.1));
			Assert.That(forecastRow.Agents, Is.EqualTo(4.05));
			
		}
	}
}
