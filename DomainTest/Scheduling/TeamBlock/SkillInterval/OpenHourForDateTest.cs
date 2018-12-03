using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;



namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.SkillInterval
{
	[TestFixture]
	public class OpenHourForDateTest
	{
		private MockRepository _mocks;
		private IOpenHourForDate _target;
		private ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
		private DateOnly _dateOnly;
		private IActivity _activity1;
		private IActivity _activity2;
		private IDictionary<IActivity, IList<ISkillIntervalData>> _skillIntervalDataPerActivty;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skillIntervalDataOpenHour = _mocks.StrictMock<ISkillIntervalDataOpenHour>();
			_target = new OpenHourForDate(_skillIntervalDataOpenHour);
			_dateOnly = new DateOnly();
			_activity1 = new Activity("hej");
			_activity2 = new Activity("hopp");
			_skillIntervalDataPerActivty = new Dictionary<IActivity, IList<ISkillIntervalData>>();
		}

		[Test]
		public void TwoSkillsOfDifferentActivityShouldExpandOpenHours()
		{
			_skillIntervalDataPerActivty.Add(_activity1, new List<ISkillIntervalData>());
			_skillIntervalDataPerActivty.Add(_activity2, new List<ISkillIntervalData>());
			using (_mocks.Record())
			{
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDataPerActivty[_activity1], _dateOnly))
				      .Return(new TimePeriod(8, 0, 17, 0));
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDataPerActivty[_activity2], _dateOnly))
					  .Return(new TimePeriod(9, 0, 18, 0));
			}

			using (_mocks.Playback())
			{
				var result = _target.OpenHours(_dateOnly, _skillIntervalDataPerActivty);
				Assert.That(result.Equals(new TimePeriod(8, 0, 18, 0)));
			}
		}

		[Test]
		public void ShouldReturnNullIfAllIsClosed()
		{
			_skillIntervalDataPerActivty.Add(_activity2, new List<ISkillIntervalData>());
			using (_mocks.Record())
			{
				Expect.Call(_skillIntervalDataOpenHour.GetOpenHours(_skillIntervalDataPerActivty[_activity2], _dateOnly))
					  .Return(null);
			}

			using (_mocks.Playback())
			{
				var result = _target.OpenHours(_dateOnly, _skillIntervalDataPerActivty);
				Assert.IsFalse(result.HasValue);
			}
		}

	}
}