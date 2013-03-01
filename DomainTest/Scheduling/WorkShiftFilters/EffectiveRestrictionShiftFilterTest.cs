using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftFilters
{
	[TestFixture]
	public class EffectiveRestrictionShiftFilterTest
	{
		private IEffectiveRestrictionShiftFilter _target;
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
			ISchedulingOptions schedulingOptions = new SchedulingOptions();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			var result = _mocks.StrictMock<IWorkShiftFinderResult>();

			using (_mocks.Record())
			{
				Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
				Expect.Call(effectiveRestriction.IsRotationDay).Return(false);
				Expect.Call(effectiveRestriction.IsAvailabilityDay).Return(false);
				Expect.Call(effectiveRestriction.IsPreferenceDay).Return(false);
				Expect.Call(effectiveRestriction.IsStudentAvailabilityDay).Return(false);
				Expect.Call(() => result.AddFilterResults(null)).IgnoreArguments();
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
				bool ret = _target.Filter(schedulingOptions, effectiveRestriction, result);
				Assert.IsFalse(ret);
			}
		}


		[Test]
		public void ShouldReturnCorrectIfUsePreferenceMustHavesOnly()
		{
			ISchedulingOptions schedulingOptions = new SchedulingOptions();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			var result = _mocks.StrictMock<IWorkShiftFinderResult>();

			using (_mocks.Record())
			{
				Expect.Call(effectiveRestriction.ShiftCategory).Return(null);
				Expect.Call(effectiveRestriction.IsPreferenceDay).Return(false);
				Expect.Call(() => result.AddFilterResults(null)).IgnoreArguments();
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
				bool ret = _target.Filter(schedulingOptions, effectiveRestriction, result);
				Assert.IsFalse(ret);
			}
		}

		[Test]
		public void VerifyRestrictionCheckWhenTrue()
		{
			ISchedulingOptions schedulingOptions = new SchedulingOptions();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			var result = _mocks.StrictMock<IWorkShiftFinderResult>();
			IList<IWorkShiftFilterResult> lstResult = new List<IWorkShiftFilterResult>();
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
				bool ret = _target.Filter(schedulingOptions, effectiveRestriction, result);
				Assert.IsTrue(ret);
				Assert.AreEqual(0, lstResult.Count);
			}
		}

		[Test]
		public void VerifyRestrictionCheckWithNullEffectiveReturnsFalse()
		{
			ISchedulingOptions schedulingOptions = new SchedulingOptions();
			IEffectiveRestriction effectiveRestriction = null;
			var result = _mocks.StrictMock<IWorkShiftFinderResult>();

			using (_mocks.Record())
			{
				Expect.Call(() => result.AddFilterResults(null)).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				bool ret = _target.Filter(schedulingOptions, effectiveRestriction, result);
				Assert.IsFalse(ret);
			}
		}

		[Test]
		public void ShouldCheckIfCategoryInRestrictionConflictsWithOptions()
		{
			var effective = _mocks.StrictMock<IEffectiveRestriction>();
			var category = new ShiftCategory("effCat");
			category.SetId(Guid.NewGuid());
			IShiftCategory category1 = new ShiftCategory("optCat");
			category1.SetId(Guid.NewGuid());
			var options = _mocks.StrictMock<ISchedulingOptions>();
			var finderResult = new WorkShiftFinderResult(new Person(), new DateOnly(2009, 2, 3));

			Expect.Call(effective.ShiftCategory).Return(category).Repeat.Twice();
			Expect.Call(options.ShiftCategory).Return(category1).Repeat.Twice();

			_mocks.ReplayAll();
			var ret = _target.Filter(options, effective, finderResult);
			Assert.That(ret, Is.False);
			Assert.That(finderResult.FilterResults.Count, Is.GreaterThan(0));
			_mocks.VerifyAll();
		}
	}
}
