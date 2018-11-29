using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class MainShiftOptimizeActivitiesSpecificationShiftFilterTest
	{
		private MainShiftOptimizeActivitiesSpecificationShiftFilter _target;
		private TimeZoneInfo _timeZoneInfo;
		private ShiftCategory _category;
		private IActivity _activity;
		private DateOnly _dateOnly;

		[SetUp]
		public void Setup()
		{
			_dateOnly = new DateOnly(2013, 3, 1);
			_activity = ActivityFactory.CreateActivity("sd");
			_activity.SetId(Guid.NewGuid());
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
			_category.SetId(Guid.NewGuid());
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_target = new MainShiftOptimizeActivitiesSpecificationShiftFilter();
		}

		[Test]
		public void ShouldFilterAccordingToMainShiftOptimizeActivitiesSpecification()
		{
			var result = _target.Filter(getCashes(), new Domain.Specification.All<IEditableShift>());
			Assert.That(result.Count, Is.EqualTo(3));
		}
		
		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(new List<ShiftProjectionCache>(), new Domain.Specification.All<IEditableShift>());
			Assert.That(result.Count, Is.EqualTo(0));

			result = _target.Filter(null, new Domain.Specification.All<IEditableShift>());
			Assert.IsNull(result);
		}

		private IList<ShiftProjectionCache> getCashes()
		{
			var tmpList = getWorkShifts();
			var retList = new List<ShiftProjectionCache>();
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(_dateOnly, _timeZoneInfo);
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, dateOnlyAsDateTimePeriod);
				retList.Add(cache);
			}
			return retList;
		}

		private IEnumerable<IWorkShift> getWorkShifts()
		{
			var workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
															  _activity, _category);
			var workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														  _activity, _category);
			var workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
																	  _activity, _category);

			return new List<IWorkShift> { workShift1, workShift2, workShift3 };
		}
	}
}
