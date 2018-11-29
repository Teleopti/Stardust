using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class ActivityRestrictionsShiftFilterTest
	{
		private ActivityRestrictionsShiftFilter _target;
		private IPerson _person;
		private DateOnly _dateOnly;

		[SetUp]
		public void Setup()
		{
			_dateOnly = new DateOnly(2009, 2, 2);
			_person = PersonFactory.CreatePerson("bill");
			_target = new ActivityRestrictionsShiftFilter();
		}

		[Test]
		public void ShouldFilterActivityRestriction()
		{
			IActivity testActivity = ActivityFactory.CreateActivity("test");
			IActivity breakActivity = ActivityFactory.CreateActivity("lunch");
			var breakPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 13, 0, 0, DateTimeKind.Utc));


			IWorkShift ws1 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(8), TimeSpan.FromHours(21), testActivity);
			ws1.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod));
			IWorkShift ws2 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(9), TimeSpan.FromHours(22), testActivity);
			ws2.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod.MovePeriod(new TimeSpan(1, 0, 0))));
			IList<IWorkShift> listOfWorkShifts = new List<IWorkShift> { ws1, ws2 };

			var timeZoneInfo = TimeZoneInfo.Utc;
			var casheList = new List<ShiftProjectionCache>();
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(_dateOnly, timeZoneInfo);
			foreach (IWorkShift shift in listOfWorkShifts)
			{
				var cache = new ShiftProjectionCache(shift, dateOnlyAsDateTimePeriod);
				casheList.Add(cache);
			}

			IList<IActivityRestriction> activityRestrictions = new List<IActivityRestriction>();
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation(), null, null, null,
																				  activityRestrictions);
			var ret = _target.Filter(_dateOnly, _person, casheList, effectiveRestriction);
			Assert.AreEqual(2, ret.Count);

			var activityRestriction = new ActivityRestriction(breakActivity)
			{
				StartTimeLimitation = new StartTimeLimitation(new TimeSpan(12, 0, 0), null),
				EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(13, 0, 0))
			};
			activityRestrictions = new List<IActivityRestriction> { activityRestriction };
			effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation(), null, null, null,
																				  activityRestrictions);

			ret = _target.Filter(_dateOnly, _person, casheList, effectiveRestriction);
			Assert.AreEqual(1, ret.Count);
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var casheList = new List<ShiftProjectionCache>();
			IList<IActivityRestriction> activityRestrictions = new List<IActivityRestriction>();
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation(), null, null, null,
																				  activityRestrictions);
			var result = _target.Filter(_dateOnly, _person, casheList, null);
			Assert.IsNull(result);
			
			result = _target.Filter(_dateOnly, null, casheList, effectiveRestriction);
			Assert.IsNull(result);
		}
	}
}
