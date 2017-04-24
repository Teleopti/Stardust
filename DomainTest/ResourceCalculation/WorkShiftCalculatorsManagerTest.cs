using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

			var day = new DateOnlyAsDateTimePeriod(new DateOnly(2001,1,1), TimeZoneInfo.Utc);
			var editableShift1 = MockRepository.GenerateMock<IEditableShift>();
			var editableShift2 = MockRepository.GenerateMock<IEditableShift>();
			var projectionService1 = MockRepository.GenerateMock<IProjectionService>();
			var projectionService2 = MockRepository.GenerateMock<IProjectionService>();
			var workShift1 = MockRepository.GenerateMock<IWorkShift>();
			var workShift2 = MockRepository.GenerateMock<IWorkShift>();
			var personalShiftMeetingTimeChecker = new PersonalShiftMeetingTimeChecker();
			var cache1 = new ShiftProjectionCache(workShift1,personalShiftMeetingTimeChecker);
			var cache2 = new ShiftProjectionCache(workShift2,personalShiftMeetingTimeChecker);
			cache1.SetDate(day);
			cache2.SetDate(day);
			var proj1v = new VisualLayerCollection(person, new IVisualLayer[] { }, new ProjectionIntersectingPeriodMerger());
			var proj2v = new VisualLayerCollection(person, new IVisualLayer[] { }, new ProjectionIntersectingPeriodMerger());
			var caches = new List<ShiftProjectionCache> { cache1, cache2 };
			var dataHolders = MockRepository.GenerateMock<IWorkShiftCalculatorSkillStaffPeriodData>();
			var nonBlendSkillPeriods = MockRepository.GenerateMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();

			workShift1.Stub(x => x.ToEditorShift(day, TimeZoneInfo.Utc)).Return(editableShift1);
			workShift2.Stub(x => x.ToEditorShift(day, TimeZoneInfo.Utc)).Return(editableShift2);
			editableShift1.Stub(x => x.ProjectionService()).Return(projectionService1);
			editableShift2.Stub(x => x.ProjectionService()).Return(projectionService2);
			projectionService1.Stub(x => x.CreateProjection()).Return(proj1v);
			projectionService2.Stub(x => x.CreateProjection()).Return(proj2v);
			workShiftCalculator.Stub(x => x.CalculateShiftValue(cache1.WorkShiftCalculatableLayers, dataHolders, WorkShiftLengthHintOption.AverageWorkTime, true, true, TimeHelper.FitToDefaultResolution)).Return(double.MinValue);
			workShiftCalculator.Stub(x => x.CalculateShiftValue(cache2.WorkShiftCalculatableLayers, dataHolders, WorkShiftLengthHintOption.AverageWorkTime, true, true, TimeHelper.FitToDefaultResolution)).Return(5);

			nonBlendSkillPeriods.Stub(x => x.Count).Return(5);
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