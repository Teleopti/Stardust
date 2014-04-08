using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class WorkShiftCalculatorsManagerTest
	{
		private MockRepository _mocks;
		private WorkShiftCalculatorsManager _target;
		private IWorkShiftCalculator _workShiftCalculator;
		private INonBlendWorkShiftCalculator _nonBlendWorkShiftCalculator;
		private SchedulingOptions _options;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_workShiftCalculator = _mocks.StrictMock<IWorkShiftCalculator>();
			_nonBlendWorkShiftCalculator = _mocks.StrictMock<INonBlendWorkShiftCalculator>();
			_options = new SchedulingOptions();
			_target = new WorkShiftCalculatorsManager(_workShiftCalculator, _nonBlendWorkShiftCalculator);
		}

		[Test]
		public void ShouldCallAllCalculatorsWithSkillStaffPeriods()
		{
			var person = new Person();
			var cache1 = _mocks.StrictMock<IShiftProjectionCache>();
			var cache2 = _mocks.StrictMock<IShiftProjectionCache>();
			var proj1v = new VisualLayerCollection(person, new IVisualLayer[] { }, new ProjectionIntersectingPeriodMerger());
			var proj2v = new VisualLayerCollection(person, new IVisualLayer[] { }, new ProjectionIntersectingPeriodMerger());
			var proj1 = new WorkShiftCalculatableVisualLayerCollection(proj1v);
			var proj2 = new WorkShiftCalculatableVisualLayerCollection(proj2v);
			var caches = new List<IShiftProjectionCache> { cache1, cache2 };
			var dataHolders = MockRepository.GenerateMock<IWorkShiftCalculatorSkillStaffPeriodData>();
			var nonBlendSkillPeriods = _mocks.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();

			Expect.Call(cache1.WorkShiftCalculatableLayers).Return(proj1);
			Expect.Call(cache2.WorkShiftCalculatableLayers).Return(proj2);
			Expect.Call(_workShiftCalculator.CalculateShiftValue(proj1, dataHolders, WorkShiftLengthHintOption.AverageWorkTime, true, true, TimeHelper.FitToDefaultResolution)).Return(double.MinValue);
			Expect.Call(_workShiftCalculator.CalculateShiftValue(proj2, dataHolders, WorkShiftLengthHintOption.AverageWorkTime, true, true, TimeHelper.FitToDefaultResolution)).Return(5);

			Expect.Call(nonBlendSkillPeriods.Count).Return(5).Repeat.Any();
			Expect.Call(cache1.MainShiftProjection).Return(proj1v);
			Expect.Call(cache2.MainShiftProjection).Return(proj2v);
			Expect.Call(_nonBlendWorkShiftCalculator.CalculateShiftValue(person, proj1v, nonBlendSkillPeriods, WorkShiftLengthHintOption.AverageWorkTime, true, true)).Return(5);
			Expect.Call(_nonBlendWorkShiftCalculator.CalculateShiftValue(person, proj2v, nonBlendSkillPeriods, WorkShiftLengthHintOption.AverageWorkTime, true, true)).Return(5);

			_mocks.ReplayAll();
			_target.RunCalculators(person, caches, dataHolders, nonBlendSkillPeriods, _options);
			_mocks.VerifyAll();
		}
	}


}