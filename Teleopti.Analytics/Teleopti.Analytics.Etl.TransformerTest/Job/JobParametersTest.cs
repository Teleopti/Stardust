using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Analytics.Etl.TransformerTest.Job
{
	[TestFixture]
	public class JobParametersTest
	{
		private JobParameters _target;
		private IJobMultipleDate _jobMultipleDate;
		private string _olapServer;
		private string _olapDatabase;

		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_jobMultipleDate = JobMultipleDateFactory.CreateJobMultipleDate();
			_olapServer = "SSASServer\\Instance";
			_olapDatabase= "SSASDatabase";
			string cubeConnectionString = string.Concat("Data Source=", _olapServer, ";", "Initial Catalog=",
														_olapDatabase);
			_target = new JobParameters(_jobMultipleDate, 1, "W. Europe Standard Time", 5, cubeConnectionString, "true", CultureInfo.CurrentCulture, null);
		}

		#endregion

		[Test]
		public void VerifyDataSource()
		{
			Assert.AreEqual(1, _target.DataSource);
		}

		[Test]
		public void VerifyTimeZone()
		{
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

			Assert.AreEqual(timeZoneInfo.Id, _target.DefaultTimeZone.Id);
		}

		[Test]
		public void VerifyCanAccessJobMultipleDate()
		{
			Assert.AreEqual(new DateTime(2008, 1, 1),
							_target.JobCategoryDates.GetJobMultipleDateItem(JobCategoryType.Schedule).StartDateLocal);
			Assert.AreEqual(new DateTime(2007, 12, 28),
							_target.JobCategoryDates.GetJobMultipleDateItem(JobCategoryType.AgentStatistics).StartDateLocal);
		}

		[Test]
		public void VerifyOlapServerAndDatabase()
		{
			Assert.AreEqual(_olapServer, _target.OlapServer);
			Assert.AreEqual(_olapDatabase, _target.OlapDatabase);
		}

		[Test]
		public void VerifyTimeZonesUsedByDataSources()
		{
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			IList<TimeZoneInfo> timeZones = new List<TimeZoneInfo>();
			_target.TimeZonesUsedByDataSources = timeZones;
			Assert.AreEqual(0, _target.TimeZonesUsedByDataSources.Count);
			_target.TimeZonesUsedByDataSources.Add(timeZoneInfo);
			Assert.AreEqual(1, _target.TimeZonesUsedByDataSources.Count);
		}

		[Test]
		public void VerifyIsPMInstalled()
		{
			Assert.IsTrue(_target.IsPmInstalled);

			_target = new JobParameters(_jobMultipleDate, 1, "W. Europe Standard Time", 5, "", "false", CultureInfo.CurrentCulture, null);
			Assert.IsFalse(_target.IsPmInstalled);

			_target = new JobParameters(_jobMultipleDate, 1, "W. Europe Standard Time", 5, "", "True", CultureInfo.CurrentCulture, null);
			Assert.IsTrue(_target.IsPmInstalled);

			_target = new JobParameters(_jobMultipleDate, 1, "W. Europe Standard Time", 5, "", "", CultureInfo.CurrentCulture, null);
			Assert.IsFalse(_target.IsPmInstalled);
		}
	}
}