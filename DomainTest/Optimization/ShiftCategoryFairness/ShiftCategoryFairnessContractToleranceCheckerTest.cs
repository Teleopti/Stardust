using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
	[TestFixture]
	public class ShiftCategoryFairnessContractToleranceCheckerTest
	{
		private MockRepository _mocks;
		private ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private ShiftCategoryFairnessContractToleranceChecker _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulePeriodTargetTimeCalculator = _mocks.DynamicMock<ISchedulePeriodTargetTimeCalculator>();
			_target = new ShiftCategoryFairnessContractToleranceChecker(_schedulePeriodTargetTimeCalculator);

		}

		[Test]
		public void ShouldReturnTrueIfWithinTolerance()
		{
			var matrix = _mocks.DynamicMock<IScheduleMatrixPro>();
			var person = _mocks.DynamicMock<IPerson>();
			var scheduleDayPro = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay = _mocks.DynamicMock<IScheduleDay>();
			var projService = _mocks.DynamicMock<IProjectionService>();
			var layers = _mocks.DynamicMock<IVisualLayerCollection>();
			
			Expect.Call(matrix.Person).Return(person);
			Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrix)).Return(new TimePeriod(6, 0, 10, 0));
			Expect.Call(matrix.EffectivePeriodDays).Return(
				new ReadOnlyCollection<IScheduleDayPro>(new Collection<IScheduleDayPro> {scheduleDayPro}));
			Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
			Expect.Call(scheduleDay.ProjectionService()).Return(projService);
			Expect.Call(projService.CreateProjection()).Return(layers);
			Expect.Call(layers.ContractTime()).Return(TimeSpan.FromHours(8));
			_mocks.ReplayAll();
			var result = _target.IsOutsideTolerance(new List<IScheduleMatrixPro> {matrix}, person);
			Assert.That(result,Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfOutsideTolerance()
		{
			var matrix = _mocks.DynamicMock<IScheduleMatrixPro>();
			var person = _mocks.DynamicMock<IPerson>();
			var scheduleDayPro = _mocks.DynamicMock<IScheduleDayPro>();
			var scheduleDay = _mocks.DynamicMock<IScheduleDay>();
			var projService = _mocks.DynamicMock<IProjectionService>();
			var layers = _mocks.DynamicMock<IVisualLayerCollection>();

			Expect.Call(matrix.Person).Return(person);
			Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrix)).Return(new TimePeriod(6, 0, 10, 0));
			Expect.Call(matrix.EffectivePeriodDays).Return(
				new ReadOnlyCollection<IScheduleDayPro>(new Collection<IScheduleDayPro> { scheduleDayPro }));
			Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
			Expect.Call(scheduleDay.ProjectionService()).Return(projService);
			Expect.Call(projService.CreateProjection()).Return(layers);
			Expect.Call(layers.ContractTime()).Return(TimeSpan.FromHours(12));
			_mocks.ReplayAll();
			var result = _target.IsOutsideTolerance(new List<IScheduleMatrixPro> { matrix }, person);
			Assert.That(result, Is.True);
			_mocks.VerifyAll();
		}
	}

	
	
}