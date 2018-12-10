using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class MainShiftOptimizeActivitySpecificationSetterTest
	{
		private IMainShiftOptimizeActivitySpecificationSetter _target;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private IOptimizationPreferences _optimizationPreferences;
		private IEditableShift _mainShift;

		[SetUp]
		public void Setup()
		{
			_target = new MainShiftOptimizeActivitySpecificationSetter(new CorrectAlteredBetween(UserTimeZone.Make()), new OptimizerActivitiesPreferencesFactory());
			_schedulingOptionsCreator = new SchedulingOptionsCreator();
			_optimizationPreferences = new OptimizationPreferences();
			_mainShift = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers();
		}

		[Test]
		public void ShouldSetCorrectValues()
		{
			_optimizationPreferences.Shifts.AlterBetween = true;
			_optimizationPreferences.Shifts.KeepEndTimes = true;
			_optimizationPreferences.Shifts.KeepShiftCategories = true;
			_optimizationPreferences.Shifts.KeepStartTimes = true;
			_optimizationPreferences.Shifts.SelectedActivities = new List<IActivity >();
			_optimizationPreferences.Shifts.KeepActivityLength = true;

			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences);

			var minShiftOptimizeActivitySpecification = schedulingOptions.MainShiftOptimizeActivitySpecification;
			Assert.AreEqual(typeof(All<IEditableShift>), minShiftOptimizeActivitySpecification.GetType());

			_target.SetMainShiftOptimizeActivitySpecification(schedulingOptions, _optimizationPreferences, _mainShift, DateOnly.MinValue);

			var optimizerActivitiesPreferences = new OptimizerActivitiesPreferences();
			optimizerActivitiesPreferences.KeepShiftCategory = _optimizationPreferences.Shifts.KeepShiftCategories;
			optimizerActivitiesPreferences.KeepStartTime = _optimizationPreferences.Shifts.KeepStartTimes;
			optimizerActivitiesPreferences.KeepEndTime = _optimizationPreferences.Shifts.KeepEndTimes;
			//throw new NotImplementedException();
			optimizerActivitiesPreferences.AllowAlterBetween = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(36));
			optimizerActivitiesPreferences.SetDoNotMoveActivities(new List<IActivity>());

			minShiftOptimizeActivitySpecification = new MainShiftOptimizeActivitiesSpecification(new CorrectAlteredBetween(UserTimeZone.Make()), optimizerActivitiesPreferences, _mainShift, DateOnly.MinValue);

			Assert.AreEqual(typeof(MainShiftOptimizeActivitiesSpecification), minShiftOptimizeActivitySpecification.GetType());
		}
	}
}