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
		[Test]
		public void ShouldCallAllCalculatorsWithSkillStaffPeriods()
		{
			var person = new Person();

			var workShiftCalculator = MockRepository.GenerateMock<IWorkShiftCalculator>();
			var nonBlendWorkShiftCalculator = MockRepository.GenerateMock<INonBlendWorkShiftCalculator>();
			var options = new SchedulingOptions();
			var target = new WorkShiftCalculatorsManager(workShiftCalculator, nonBlendWorkShiftCalculator);

			var cache1 = MockRepository.GenerateMock<IShiftProjectionCache>();
			var cache2 = MockRepository.GenerateMock<IShiftProjectionCache>();
			var proj1v = new VisualLayerCollection(person, new IVisualLayer[] { }, new ProjectionIntersectingPeriodMerger());
			var proj2v = new VisualLayerCollection(person, new IVisualLayer[] { }, new ProjectionIntersectingPeriodMerger());
			var proj1 = new WorkShiftCalculatableVisualLayerCollection(proj1v);
			var proj2 = new WorkShiftCalculatableVisualLayerCollection(proj2v);
			var caches = new List<IShiftProjectionCache> { cache1, cache2 };
			var dataHolders = MockRepository.GenerateMock<IWorkShiftCalculatorSkillStaffPeriodData>();
			var nonBlendSkillPeriods = MockRepository.GenerateMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();

			cache1.Stub(x => x.WorkShiftCalculatableLayers).Return(proj1);
			cache2.Stub(x => x.WorkShiftCalculatableLayers).Return(proj2);
			workShiftCalculator.Stub(x => x.CalculateShiftValue(proj1, dataHolders, WorkShiftLengthHintOption.AverageWorkTime, true, true, TimeHelper.FitToDefaultResolution)).Return(double.MinValue);
			workShiftCalculator.Stub(x => x.CalculateShiftValue(proj2, dataHolders, WorkShiftLengthHintOption.AverageWorkTime, true, true, TimeHelper.FitToDefaultResolution)).Return(5);

			nonBlendSkillPeriods.Stub(x => x.Count).Return(5).Repeat.Any();
			cache1.Stub(x => x.MainShiftProjection).Return(proj1v);
			cache2.Stub(x => x.MainShiftProjection).Return(proj2v);
			nonBlendWorkShiftCalculator.Stub(x => x.CalculateShiftValue(person, proj1v, nonBlendSkillPeriods, WorkShiftLengthHintOption.AverageWorkTime, true, true)).Return(5);
			nonBlendWorkShiftCalculator.Stub(x => x.CalculateShiftValue(person, proj2v, nonBlendSkillPeriods, WorkShiftLengthHintOption.AverageWorkTime, true, true)).Return(5);

			target.RunCalculators(person, caches, dataHolders, nonBlendSkillPeriods, options);

			nonBlendWorkShiftCalculator.AssertWasCalled(
				x =>
					x.CalculateShiftValue(person, proj1v, nonBlendSkillPeriods, WorkShiftLengthHintOption.AverageWorkTime, true, true));
			nonBlendWorkShiftCalculator.AssertWasCalled(
				x =>
					x.CalculateShiftValue(person, proj2v, nonBlendSkillPeriods, WorkShiftLengthHintOption.AverageWorkTime, true, true));
		}
	}
}