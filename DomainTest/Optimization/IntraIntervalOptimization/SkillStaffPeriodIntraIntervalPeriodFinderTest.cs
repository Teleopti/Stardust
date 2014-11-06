using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class SkillStaffPeriodIntraIntervalPeriodFinderTest
	{
		private SkillStaffPeriodIntraIntervalPeriodFinder _target;
		private DateTimePeriod _skillStaffPeriod;
		private DateTimePeriod _activityPeriod;
		private DateTime _start;
		private DateTime _end;
		private MockRepository _mock;
		private IShiftProjectionCache _shiftProjectionCache;
		private ISkill _skill;
		private IVisualLayerCollection _visualLayerCollection;
		private IPerson _person;
		private IVisualLayer _visualLayer1;
		private IVisualLayer _visualLayer2;
		private IActivity _activity;

		[SetUp]
		public void SetUp()
		{
			_target = new SkillStaffPeriodIntraIntervalPeriodFinder();
			_start = new DateTime(2014, 1, 1, 10, 0, 0,DateTimeKind.Utc);
			_end = new DateTime(2014, 1, 1, 10, 30, 0, DateTimeKind.Utc);
			_skillStaffPeriod = new DateTimePeriod(_start, _end);
			_activityPeriod = new DateTimePeriod(_start.AddMinutes(10), _end.AddMinutes(10));
			_mock = new MockRepository();
			_shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
			_activity = ActivityFactory.CreateActivity("activity");
			_skill = SkillFactory.CreateSkill("skill");
			_skill.Activity = _activity;
			_person = PersonFactory.CreatePerson("person");
			_visualLayer1 = new VisualLayer(_activity, _activityPeriod,_activity,_person);
			_visualLayer2 = new VisualLayer(_activity, _activityPeriod.MovePeriod(TimeSpan.FromHours(1)), _activity, _person);
			_visualLayerCollection = new VisualLayerCollection(_person, new List<IVisualLayer> {_visualLayer1, _visualLayer2}, new ProjectionPayloadMerger());
		}

		[Test]
		public void ShouldFind()
		{
			using (_mock.Record())
			{
				Expect.Call(_shiftProjectionCache.MainShiftProjection).Return(_visualLayerCollection);
			}

			using (_mock.Playback())
			{
				var result = _target.Find(_skillStaffPeriod, _shiftProjectionCache, _skill);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(_activityPeriod, result[0]);
			}	
		}
	}
}
