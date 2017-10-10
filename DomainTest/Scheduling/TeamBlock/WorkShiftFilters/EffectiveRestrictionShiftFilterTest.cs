using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class EffectiveRestrictionShiftFilterTest
	{
		private EffectiveRestrictionShiftFilter _target;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new EffectiveRestrictionShiftFilter();
		}

		[Test]
		public void ShouldFilterAccordingToEffectiveRestriction()
		{
			SchedulingOptions schedulingOptions = new SchedulingOptions();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

			using (_mocks.Record())
			{
				Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
				Expect.Call(effectiveRestriction.IsRotationDay).Return(false);
				Expect.Call(effectiveRestriction.IsAvailabilityDay).Return(false);
				Expect.Call(effectiveRestriction.IsPreferenceDay).Return(false);
				Expect.Call(effectiveRestriction.IsStudentAvailabilityDay).Return(false);
			}

			schedulingOptions.UseRotations = true;
			schedulingOptions.RotationDaysOnly = true;
			schedulingOptions.UsePreferences = true;
			schedulingOptions.PreferencesDaysOnly = true;
			schedulingOptions.UseAvailability = true;
			schedulingOptions.AvailabilityDaysOnly = true;
			schedulingOptions.UseStudentAvailability = true;

			using (_mocks.Playback())
			{
				bool ret = _target.Filter(schedulingOptions, effectiveRestriction);
				Assert.IsFalse(ret);
			}
		}


		[Test]
		public void ShouldReturnCorrectIfUsePreferenceMustHavesOnly()
		{
			SchedulingOptions schedulingOptions = new SchedulingOptions();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

			using (_mocks.Record())
			{
				Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
				Expect.Call(effectiveRestriction.IsPreferenceDay).Return(false);
			}

			schedulingOptions.UseRotations = false;
			schedulingOptions.RotationDaysOnly = false;
			schedulingOptions.UsePreferences = true;
			schedulingOptions.PreferencesDaysOnly = false;
			schedulingOptions.UseAvailability = false;
			schedulingOptions.AvailabilityDaysOnly = false;
			schedulingOptions.UseStudentAvailability = false;
			schedulingOptions.UsePreferencesMustHaveOnly = true;

			using (_mocks.Playback())
			{
				bool ret = _target.Filter(schedulingOptions, effectiveRestriction);
				Assert.IsFalse(ret);
			}
		}

		[Test]
		public void VerifyRestrictionCheckWhenTrue()
		{
			SchedulingOptions schedulingOptions = new SchedulingOptions();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			using (_mocks.Record())
			{
				Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
				Expect.Call(effectiveRestriction.IsRotationDay).Return(true);
			}

			schedulingOptions.UseRotations = true;
			schedulingOptions.RotationDaysOnly = true;
			schedulingOptions.UsePreferences = false;
			schedulingOptions.PreferencesDaysOnly = false;
			schedulingOptions.UseAvailability = false;
			schedulingOptions.AvailabilityDaysOnly = false;
			schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Playback())
			{
				bool ret = _target.Filter(schedulingOptions, effectiveRestriction);
				Assert.IsTrue(ret);
			}
		}

		[Test]
		public void VerifyRestrictionCheckWithNullEffectiveReturnsFalse()
		{
			SchedulingOptions schedulingOptions = new SchedulingOptions();
			IEffectiveRestriction effectiveRestriction = null;

			bool ret = _target.Filter(schedulingOptions, effectiveRestriction);
			Assert.IsFalse(ret);
		}

		[Test]
		public void ShouldCheckIfCategoryInRestrictionConflictsWithOptions()
		{
			var effective = _mocks.StrictMock<IEffectiveRestriction>();
			var category = new ShiftCategory("effCat");
			category.SetId(Guid.NewGuid());
			IShiftCategory category1 = new ShiftCategory("optCat");
			category1.SetId(Guid.NewGuid());
			var options = new SchedulingOptions{ShiftCategory = category1};

			Expect.Call(effective.ShiftCategory).Return(category).Repeat.Twice();

			_mocks.ReplayAll();
			var ret = _target.Filter(options, effective);
			Assert.That(ret, Is.False);
			_mocks.VerifyAll();
		}


		[Test]
		public void ShouldCheckParameters()
		{
			var effectiveRestriction = new EffectiveRestriction(
				new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
				new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
				new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
				null, null, null, new List<IActivityRestriction>());

			var result = _target.Filter(null, effectiveRestriction);
			Assert.IsFalse(result);
		}
	}
}
