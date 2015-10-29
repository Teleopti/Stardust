using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
			_target = new MainShiftOptimizeActivitySpecificationSetter();
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

			ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences);

			var minShiftOptimizeActivitySpecification = schedulingOptions.MainShiftOptimizeActivitySpecification;
			Assert.AreEqual(typeof(All<IEditableShift>), minShiftOptimizeActivitySpecification.GetType());

			_target.SetMainShiftOptimizeActivitySpecification(schedulingOptions, _optimizationPreferences, _mainShift, DateOnly.MinValue);

			IOptimizerActivitiesPreferences optimizerActivitiesPreferences = new OptimizerActivitiesPreferences();
			optimizerActivitiesPreferences.KeepShiftCategory = _optimizationPreferences.Shifts.KeepShiftCategories;
			optimizerActivitiesPreferences.KeepStartTime = _optimizationPreferences.Shifts.KeepStartTimes;
			optimizerActivitiesPreferences.KeepEndTime = _optimizationPreferences.Shifts.KeepEndTimes;
			//throw new NotImplementedException();
			optimizerActivitiesPreferences.AllowAlterBetween = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(36));
			optimizerActivitiesPreferences.SetDoNotMoveActivities(new List<IActivity>());

			minShiftOptimizeActivitySpecification = new MainShiftOptimizeActivitiesSpecification(optimizerActivitiesPreferences, _mainShift, DateOnly.MinValue, StateHolderReader.Instance.StateReader.UserTimeZone);

			Assert.AreEqual(typeof(MainShiftOptimizeActivitiesSpecification), minShiftOptimizeActivitySpecification.GetType());
		}
	}
}