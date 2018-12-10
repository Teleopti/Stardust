using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class SkillStaffPeriodIntraIntervalPeriodFinderTest
	{
		[Test]
		public void ShouldFind()
		{
			var target = new SkillStaffPeriodIntraIntervalPeriodFinder();
			var start = new DateTime(2014, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2014, 1, 1, 10, 30, 0, DateTimeKind.Utc);
			var skillStaffPeriod = new DateTimePeriod(start, end);
			var activityPeriod = new DateTimePeriod(start.AddMinutes(10), end.AddMinutes(10));
			var workShift = MockRepository.GenerateMock<IWorkShift>();
			var editorShift = MockRepository.GenerateMock<IEditableShift>();
			var projectionService = MockRepository.GenerateMock<IProjectionService>();
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2014,1,1), TimeZoneInfo.Utc);
			var shiftProjectionCache = new ShiftProjectionCache(workShift, dateOnlyAsDateTimePeriod);
			var activity = ActivityFactory.CreateActivity("activity");
			var skill = SkillFactory.CreateSkill("skill");
			skill.Activity = activity;
			var visualLayer1 = new VisualLayer(activity, activityPeriod, activity);
			var visualLayer2 = new VisualLayer(activity, activityPeriod.MovePeriod(TimeSpan.FromHours(1)), activity);
			var visualLayerCollection = new VisualLayerCollection(new List<IVisualLayer> {visualLayer1, visualLayer2},
				new ProjectionPayloadMerger());

			workShift.Stub(x => x.ToEditorShift(dateOnlyAsDateTimePeriod,TimeZoneInfo.Utc)).Return(editorShift);
			editorShift.Stub(x => x.ProjectionService()).Return(projectionService);
			projectionService.Stub(x => x.CreateProjection()).Return(visualLayerCollection);

			var result = target.Find(skillStaffPeriod, shiftProjectionCache, skill);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(activityPeriod, result[0]);
		}
	}
}
