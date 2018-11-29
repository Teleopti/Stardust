using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class IntervalTransformerTest : IDisposable
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			// get list with 96 intervals per day
			_intervalList96 = IntervalFactory.CreateIntervalCollection(96);
			// get list with 288 intervals per day
			_intervalList288 = IntervalFactory.CreateIntervalCollection(288);
			var target = new IntervalTransformer(_insertDateTime);

			_table96 = new DataTable();
			_table96.Locale = Thread.CurrentThread.CurrentCulture;
			IntervalInfrastructure.AddColumnsToDataTable(_table96);
			_table288 = new DataTable { Locale = Thread.CurrentThread.CurrentCulture };
			IntervalInfrastructure.AddColumnsToDataTable(_table288);

			target.Transform(_intervalList96, _table96);
			target.Transform(_intervalList288, _table288);
		}

		#endregion

		private readonly DateTime _insertDateTime = DateTime.Now;
		private IList<Interval> _intervalList288 = new List<Interval>();
		private IList<Interval> _intervalList96 = new List<Interval>();
		private DataTable _table288;
		private DataTable _table96;

		[Test]
		public void Verify96IntervalPerDay()
		{
			Assert.AreEqual(96, _intervalList96.Count);
			Assert.AreEqual(0, _intervalList96[0].Id);
			var period1 =
				 new DateTimePeriod(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),
										  new DateTime(1900, 1, 1, 0, 15, 0, DateTimeKind.Utc));
			Assert.AreEqual(period1, _intervalList96[0].Period);
		}

		[Test]
		public void VerifyIntervalContent()
		{
			Assert.AreEqual(0, _table288.Rows[0]["interval_id"]);
			Assert.AreEqual(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc), _table288.Rows[0]["interval_start"]);
			Assert.AreEqual(new DateTime(1900, 1, 1, 0, 5, 0, DateTimeKind.Utc), _table288.Rows[0]["interval_end"]);
			Assert.AreEqual(new DateTime(1900, 1, 1, 23, 55, 0, DateTimeKind.Utc), _table288.Rows[287]["interval_start"]);
			Assert.AreEqual(new DateTime(1900, 1, 2, 0, 0, 0, DateTimeKind.Utc), _table288.Rows[287]["interval_end"]);
			Assert.AreEqual("00:05-00:10", _table288.Rows[1]["interval_name"]);
			Assert.AreEqual("23:50-23:55", _table288.Rows[286]["interval_name"]);
			Assert.AreEqual("23:55-24:00", _table288.Rows[287]["interval_name"]);
			Assert.AreEqual("00:00-00:30", _table288.Rows[0]["halfhour_name"]);
			Assert.AreEqual("23:00-23:30", _table288.Rows[281]["halfhour_name"]);
			Assert.AreEqual("23:30-24:00", _table288.Rows[287]["halfhour_name"]);
			Assert.AreEqual("00:00-01:00", _table288.Rows[0]["hour_name"]);
			Assert.AreEqual("23:00-24:00", _table288.Rows[287]["hour_name"]);
		}

		[Test]
		public void VerifyIntervalMatch()
		{
			Assert.AreEqual(_intervalList96[0].Id, _table96.Rows[0]["interval_id"]);
			Assert.AreEqual(_intervalList96[0].Period.StartDateTime, _table96.Rows[0]["interval_start"]);
			Assert.AreEqual(_intervalList96[0].Period.EndDateTime, _table96.Rows[0]["interval_end"]);
			Assert.AreEqual(_intervalList96[95].Period.StartDateTime, _table96.Rows[95]["interval_start"]);
			Assert.AreEqual(_intervalList96[95].Period.EndDateTime, _table96.Rows[95]["interval_end"]);
			Assert.AreEqual(_intervalList96[0].IntervalName, _table96.Rows[0]["interval_name"]);
			Assert.AreEqual(_intervalList96[95].HalfHourName, _table96.Rows[95]["halfhour_name"]);
			Assert.AreEqual(_intervalList96[1].HourName, _table96.Rows[1]["hour_name"]);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_table96 != null)
					_table96.Dispose();
				if (_table288 != null)
					_table288.Dispose();
			}
		}
	}
}