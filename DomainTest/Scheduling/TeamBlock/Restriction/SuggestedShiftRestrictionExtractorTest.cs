using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
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
			_schedulingOptions = new SchedulingOptions();
			_target = new SuggestedShiftRestrictionExtractor();
		}
		
		[Test]
		public void ShouldExtractStartTimeRestrictionFromSuggestedShift()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var startTime = new TimeSpan(8, 0, 0);
			_schedulingOptions.BlockSameStartTime = true;
			_schedulingOptions.UseTeamBlockPerOption = true;
			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftStartTime).Return(startTime);
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
		public void ShouldExtractStartEndTimeRestrictionFromSuggestedShiftForTeamScheduling()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var startTime = new TimeSpan(8, 0, 0);
			var endTime = new TimeSpan(17, 0, 0);
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameStartTime = true;
			_schedulingOptions.TeamSameEndTime = true;
			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftStartTime).Return(startTime);
				Expect.Call(shift.WorkShiftEndTime).Return(endTime);
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
		public void ShouldExtractStartEndTimeRestrictionFromSuggestedShiftForOneBlock()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var startTime = new TimeSpan(8, 0, 0);
			var endTime = new TimeSpan(17, 0, 0);
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.BlockSameStartTime = true;
			_schedulingOptions.BlockSameEndTime = true;
			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftStartTime).Return(startTime);
				Expect.Call(shift.WorkShiftEndTime).Return(endTime);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(startTime, startTime),
																	new EndTimeLimitation(endTime, endTime),
																	new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
				var result = _target.ExtractForOneBlock(shift, _schedulingOptions);

				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractStartEndTimeRestrictionFromSuggestedShiftForOneTeam()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var startTime = new TimeSpan(8, 0, 0);
			var endTime = new TimeSpan(17, 0, 0);
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameStartTime = true;
			_schedulingOptions.TeamSameEndTime = true;
			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftStartTime).Return(startTime);
				Expect.Call(shift.WorkShiftEndTime).Return(endTime);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(startTime, startTime),
																	new EndTimeLimitation(endTime, endTime),
																	new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
				var result = _target.ExtractForOneTeam(shift, _schedulingOptions);

				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractEndTimeRestrictionFromSuggestedShift()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var endTime = new TimeSpan(17, 0, 0);
			_schedulingOptions.BlockSameEndTime = true;
			_schedulingOptions.UseTeamBlockPerOption = true;
			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftEndTime).Return(endTime);
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

			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.BlockSameStartTime = true;
			_schedulingOptions.BlockSameEndTime = true;

			using (_mocks.Record())
			{
				Expect.Call(shift.WorkShiftStartTime).Return(startTime);
				Expect.Call(shift.WorkShiftEndTime).Return(endTime);
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
			var mainShift = EditableShiftFactory.CreateEditorShift(new TimeSpan(11, 0, 0), new TimeSpan(19, 0, 0),
														 activity, category);
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.BlockSameShift = true;

			using (_mocks.Record())
			{
				Expect.Call(shift.TheMainShift).Return(mainShift).Repeat.Twice();
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

				result = _target.ExtractForOneBlock(shift, _schedulingOptions);
				Assert.That(result, Is.EqualTo(expected));
			}
		}


		[Test]
		public void ShouldExtractSameShiftCategoryRestrictionFromSuggestedShift()
		{
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.BlockSameShiftCategory = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameShiftCategory = true;
			var activity = ActivityFactory.CreateActivity("sd");
			activity.SetId(Guid.NewGuid());
			var category = ShiftCategoryFactory.CreateShiftCategory("dv");
			category.SetId(Guid.NewGuid());
			var workShift = WorkShiftFactory.CreateWorkShift(TimeSpan.Zero, TimeSpan.Zero, activity, category);
			using (_mocks.Record())
			{
				Expect.Call(shift.TheWorkShift).Return(workShift).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
				                                        new EndTimeLimitation(),
				                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
					{
						ShiftCategory = category
					};
				var result = _target.Extract(shift, _schedulingOptions);
				Assert.That(result, Is.EqualTo(expected));

				result = _target.ExtractForOneBlock(shift, _schedulingOptions);
				Assert.That(result, Is.EqualTo(expected));

				result = _target.ExtractForOneTeam(shift, _schedulingOptions);
				Assert.That(result, Is.EqualTo(expected));
			}
		}

	}
}
