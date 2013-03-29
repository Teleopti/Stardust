using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class ActivityRestrictionsShiftFilterTest
	{
		private IActivityRestrictionsShiftFilter _target;
		private MockRepository _mocks;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = PersonFactory.CreatePerson("bill");
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_target = new ActivityRestrictionsShiftFilter();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldFilterActivityRestriction()
		{
			var dateOnly = new DateOnly(2009, 2, 2);
			var finderResult = new WorkShiftFinderResult(new Person(), new DateOnly(2009, 2, 3));
			IActivity testActivity = ActivityFactory.CreateActivity("test");
			IActivity breakActivity = ActivityFactory.CreateActivity("lunch");
			var breakPeriod = new DateTimePeriod(new DateTime(1800, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 13, 0, 0, DateTimeKind.Utc));


			IWorkShift ws1 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(8), TimeSpan.FromHours(21), testActivity);
			ws1.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod));
			IWorkShift ws2 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(9), TimeSpan.FromHours(22), testActivity);
			ws2.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod.MovePeriod(new TimeSpan(1, 0, 0))));
			IList<IWorkShift> listOfWorkShifts = new List<IWorkShift> { ws1, ws2 };

			var timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var casheList = new List<IShiftProjectionCache>();
			foreach (IWorkShift shift in listOfWorkShifts)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(dateOnly, timeZoneInfo);
				casheList.Add(cache);
			}

			IList<IActivityRestriction> activityRestrictions = new List<IActivityRestriction>();
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation(), null, null, null,
																				  activityRestrictions);
			var ret = _target.Filter(dateOnly, _person, casheList, effectiveRestriction, finderResult);
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

			ret = _target.Filter(dateOnly, _person, casheList, effectiveRestriction, finderResult);
			Assert.AreEqual(0, ret.Count);
		}
	}
}
