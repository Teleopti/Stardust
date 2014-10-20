using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GeneralPreferencesPersonalSettingsTest
	{
		private GeneralPreferencesPersonalSettings _target;
		private IList<IScheduleTag> _scheduleTags;
		private IScheduleTag _scheduleTag1;
		private Guid _scheduleTag1Id;
		private IScheduleTag _scheduleTag2;
		private Guid _scheduleTag2Id;
		private IGeneralPreferences _generalPreferencesSource;
		private IGeneralPreferences _generalPreferencesTarget;

		[SetUp]
		public void Setup()
		{
			_scheduleTag1Id = Guid.NewGuid();
			_scheduleTag2Id = Guid.NewGuid();
			_scheduleTag1 = new ScheduleTag();
			_scheduleTag2 = new ScheduleTag();
			_scheduleTag1.SetId(_scheduleTag1Id);
			_scheduleTag2.SetId(_scheduleTag2Id);
			_scheduleTags = new List<IScheduleTag> { _scheduleTag1 };
			_target = new GeneralPreferencesPersonalSettings();
			_generalPreferencesSource = new GeneralPreferences();
			_generalPreferencesTarget = new GeneralPreferences();
		}

		[Test]
		public void VerifyDefaultValues()
		{
			_target.MapTo(_generalPreferencesTarget, _scheduleTags);
			Assert.AreEqual(true, _generalPreferencesTarget.OptimizationStepDaysOff);
			Assert.AreEqual(true, _generalPreferencesTarget.OptimizationStepTimeBetweenDays);
			Assert.AreEqual(true, _generalPreferencesTarget.OptimizationStepShiftsWithinDay);

			Assert.AreEqual(true, _generalPreferencesTarget.UsePreferences);
			Assert.AreEqual(true, _generalPreferencesTarget.UseMustHaves);
			Assert.AreEqual(true, _generalPreferencesTarget.UseRotations);
			Assert.AreEqual(true, _generalPreferencesTarget.UseAvailabilities);
			Assert.AreEqual(false, _generalPreferencesTarget.UseStudentAvailabilities);
			Assert.AreEqual(true, _generalPreferencesTarget.UseShiftCategoryLimitations);

			Assert.AreEqual(0.8d, _generalPreferencesTarget.PreferencesValue);
			Assert.AreEqual(1d, _generalPreferencesTarget.MustHavesValue);
			Assert.AreEqual(1d, _generalPreferencesTarget.RotationsValue);
			Assert.AreEqual(1d, _generalPreferencesTarget.AvailabilitiesValue);
			Assert.AreEqual(1d, _generalPreferencesTarget.StudentAvailabilitiesValue);

			Assert.AreEqual(false, _generalPreferencesTarget.OptimizationStepIntraInterval);
		}

		[Test]
		public void MappingShouldGetAndSetSimpleProperties()
		{

			_generalPreferencesSource.OptimizationStepTimeBetweenDays = !_generalPreferencesSource.OptimizationStepTimeBetweenDays;
			_generalPreferencesSource.OptimizationStepShiftsWithinDay = !_generalPreferencesSource.OptimizationStepShiftsWithinDay;
			_generalPreferencesSource.OptimizationStepShiftsForFlexibleWorkTime = !_generalPreferencesSource.OptimizationStepShiftsForFlexibleWorkTime;
			_generalPreferencesSource.OptimizationStepDaysOffForFlexibleWorkTime = !_generalPreferencesSource.OptimizationStepDaysOffForFlexibleWorkTime;

			_generalPreferencesSource.UsePreferences = !_generalPreferencesSource.UsePreferences;
			_generalPreferencesSource.UseMustHaves = !_generalPreferencesSource.UseMustHaves;
			_generalPreferencesSource.UseRotations = !_generalPreferencesSource.UseRotations;
			_generalPreferencesSource.UseAvailabilities = !_generalPreferencesSource.UseAvailabilities;
			_generalPreferencesSource.UseStudentAvailabilities = !_generalPreferencesSource.UseStudentAvailabilities;
			_generalPreferencesSource.UseShiftCategoryLimitations = !_generalPreferencesSource.UseShiftCategoryLimitations;

			_generalPreferencesSource.PreferencesValue = 100d;
			_generalPreferencesSource.MustHavesValue = 101d;
			_generalPreferencesSource.RotationsValue = 102d;
			_generalPreferencesSource.AvailabilitiesValue = 103d;
			_generalPreferencesSource.StudentAvailabilitiesValue = 104d;

			_generalPreferencesSource.OptimizationStepIntraInterval = true;

			_target.MapFrom(_generalPreferencesSource);
			_target.MapTo(_generalPreferencesTarget, _scheduleTags);

			Assert.AreEqual(_generalPreferencesSource.OptimizationStepTimeBetweenDays, _generalPreferencesTarget.OptimizationStepTimeBetweenDays);
			Assert.AreEqual(_generalPreferencesSource.OptimizationStepShiftsWithinDay, _generalPreferencesTarget.OptimizationStepShiftsWithinDay);
			Assert.AreEqual(_generalPreferencesSource.OptimizationStepShiftsForFlexibleWorkTime, _generalPreferencesTarget.OptimizationStepShiftsForFlexibleWorkTime);
			Assert.AreEqual(_generalPreferencesSource.OptimizationStepDaysOffForFlexibleWorkTime, _generalPreferencesTarget.OptimizationStepDaysOffForFlexibleWorkTime);

			Assert.AreEqual(_generalPreferencesSource.UsePreferences, _generalPreferencesTarget.UsePreferences);
			Assert.AreEqual(_generalPreferencesSource.UseMustHaves, _generalPreferencesTarget.UseMustHaves);
			Assert.AreEqual(_generalPreferencesSource.UseRotations, _generalPreferencesTarget.UseRotations);
			Assert.AreEqual(_generalPreferencesSource.UseAvailabilities, _generalPreferencesTarget.UseAvailabilities);
			Assert.AreEqual(_generalPreferencesSource.UseStudentAvailabilities, _generalPreferencesTarget.UseStudentAvailabilities);
			Assert.AreEqual(_generalPreferencesSource.UseShiftCategoryLimitations, _generalPreferencesTarget.UseShiftCategoryLimitations);

			Assert.AreEqual(_generalPreferencesSource.PreferencesValue, _generalPreferencesTarget.PreferencesValue);
			Assert.AreEqual(_generalPreferencesSource.MustHavesValue, _generalPreferencesTarget.MustHavesValue);
			Assert.AreEqual(_generalPreferencesSource.RotationsValue, _generalPreferencesTarget.RotationsValue);
			Assert.AreEqual(_generalPreferencesSource.AvailabilitiesValue, _generalPreferencesTarget.AvailabilitiesValue);
			Assert.AreEqual(_generalPreferencesSource.StudentAvailabilitiesValue, _generalPreferencesTarget.StudentAvailabilitiesValue);

			Assert.AreEqual(_generalPreferencesSource.OptimizationStepIntraInterval, _generalPreferencesTarget.OptimizationStepIntraInterval);
		}

		[Test]
		public void ShouldFindScheduleTagByIdIfExistInScheduleTagList()
		{
			_target.SetScheduleTagId(_scheduleTag1Id);
			_target.MapTo(_generalPreferencesTarget, _scheduleTags);
			Assert.AreEqual(_generalPreferencesTarget.ScheduleTag.Id.Value, _scheduleTag1Id);
		}

		[Test]
		public void ShouldUseDefaultScheduleTagIfIdNotExistInScheduleTagList()
		{
			_target.SetScheduleTagId(_scheduleTag2Id);
			_target.MapTo(_generalPreferencesTarget, _scheduleTags);
			Assert.AreEqual(NullScheduleTag.Instance, _generalPreferencesTarget.ScheduleTag);
		}
	}
}
