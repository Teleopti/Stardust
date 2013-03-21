﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class SuggestedShiftRestrictionExtractorTest
	{
		private MockRepository _mocks;
		private ISuggestedShiftRestrictionExtractor _target;
		private ISchedulingOptions _schedulingOptions;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
			_target = new SuggestedShiftRestrictionExtractor();
		}
		
		[Test]
		public void ShouldExtractStartTimeRestrictionFromSuggestedShift()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var startTime = new TimeSpan(8, 0, 0);

			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftStartTime).Return(startTime);
				Expect.Call(_schedulingOptions.UseLevellingSameShift).Return(false);
				Expect.Call(_schedulingOptions.UseLevellingSameStartTime).Return(true);
				Expect.Call(_schedulingOptions.UseLevellingSameEndTime).Return(false);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(startTime, startTime),
				                                        new EndTimeLimitation(),
				                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
				var result = _target.Extract(shift, _schedulingOptions);

				Assert.That(result, Is.EqualTo(expected));
			}
		}
	
		[Test]
		public void ShouldExtractEndTimeRestrictionFromSuggestedShift()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var endTime = new TimeSpan(17, 0, 0);

			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftEndTime).Return(endTime);
				Expect.Call(_schedulingOptions.UseLevellingSameShift).Return(false);
				Expect.Call(_schedulingOptions.UseLevellingSameStartTime).Return(false);
				Expect.Call(_schedulingOptions.UseLevellingSameEndTime).Return(true);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(endTime, endTime),
				                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
				var result = _target.Extract(shift, _schedulingOptions);

				Assert.That(result, Is.EqualTo(expected));
			}
		}	

		[Test]
		public void ShouldExtractStartAndEndTimeRestrictionFromSuggestedShift()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var startTime = new TimeSpan(8, 0, 0);
			var endTime = new TimeSpan(17, 0, 0);

			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftStartTime).Return(startTime);
				Expect.Call(shift.WorkShiftEndTime).Return(endTime);
				Expect.Call(_schedulingOptions.UseLevellingSameShift).Return(false);
				Expect.Call(_schedulingOptions.UseLevellingSameStartTime).Return(true);
				Expect.Call(_schedulingOptions.UseLevellingSameEndTime).Return(true);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(startTime, startTime),
														new EndTimeLimitation(endTime, endTime),
				                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
				var result = _target.Extract(shift, _schedulingOptions);

				Assert.That(result, Is.EqualTo(expected));
			}
		}
	
		[Test]
		public void ShouldExtractSameShiftAsCommonMainShiftRestrictionFromSuggestedShift()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
		
			var activity = ActivityFactory.CreateActivity("sd");
			activity.SetId(Guid.NewGuid());
			var category = ShiftCategoryFactory.CreateShiftCategory("dv");
			category.SetId(Guid.NewGuid());
			var mainShift = MainShiftFactory.CreateMainShift(new TimeSpan(11, 0, 0), new TimeSpan(19, 0, 0),
														 activity, category);
			using (_mocks.Record())
			{
				Expect.Call(shift.TheMainShift).Return(mainShift);
				Expect.Call(_schedulingOptions.UseLevellingSameShift).Return(true);
				Expect.Call(_schedulingOptions.UseLevellingSameStartTime).Return(false);
				Expect.Call(_schedulingOptions.UseLevellingSameEndTime).Return(false);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
				                                        new EndTimeLimitation(),
				                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
					{
						CommonMainShift = mainShift
					};

				var result = _target.Extract(shift, _schedulingOptions);

				Assert.That(result, Is.EqualTo(expected));
			}
		}
	}
}
