using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class ShiftProjectionCacheTest
	{
		private ShiftProjectionCache target;
		private IWorkShift workShift;
		private DateOnly schedulingDate;
		private ShiftCategory category;
		private IActivity activity;
		private TimeSpan start;
		private TimeSpan end;
		private TimeZoneInfo timeZoneInfo;

		[SetUp]
		public void Setup()
		{
			schedulingDate = new DateOnly(2009, 2, 2);
			category = new ShiftCategory("öfdöf");
			workShift = new WorkShift(category);
			activity = new Activity("alsfd");
			activity.InWorkTime = true;
			start = new TimeSpan(8, 0, 0);
			end = new TimeSpan(17, 0, 0);
			DateTimePeriod period = new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(activity, period));
			timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");
			// blir 7 - 16 med denna tidszon (W. Central Africa Standard Time)
			target = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(schedulingDate, timeZoneInfo));
		}

		[Test]
		public void VerifyCreateShiftProjectionCache()
		{
			Assert.IsNotNull(target);
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.IsNotNull(target.TheMainShift);
			Assert.IsNotNull(target.TheWorkShift);
			Assert.IsNotNull(target.MainShiftProjection());
			Assert.IsNotNull(target.WorkShiftProjectionPeriod());
			Assert.IsNotNull(target.WorkShiftProjectionContractTime());
		}

		[Test]
		public void WorkShiftEndTimeShouldHandleWorkShiftsCrossingMidnight()
		{

			workShift = new WorkShift(category);
			start = new TimeSpan(22, 0, 0);
			end = new TimeSpan(1, 8, 0, 0);
			DateTimePeriod period = new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(activity, period));

			target = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));

			TimeSpan result;

			result = target.WorkShiftEndTime();

			Assert.AreEqual(new TimeSpan(1, 8, 0, 0), result);
		}	
	}
}
