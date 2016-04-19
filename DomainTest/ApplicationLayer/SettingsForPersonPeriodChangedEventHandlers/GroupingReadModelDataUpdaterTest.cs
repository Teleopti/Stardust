using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	[TestFixture]
	public class GroupingReadModelDataUpdaterTest
	{
		private GroupingReadModelDataUpdaterHangfire _target;
		private MockRepository _mocks;
		private IGroupingReadOnlyRepository _groupReadOnlyRepository;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupReadOnlyRepository = _mocks.DynamicMock<IGroupingReadOnlyRepository>();

			_target = new GroupingReadModelDataUpdaterHangfire(_groupReadOnlyRepository);
		}


		[Test]
		public void GroupingReadModelDataTest()
		{
			var skillTest = SkillFactory.CreateSkill("Test3");
			var tempGuid = Guid.NewGuid();
			skillTest.SetId(tempGuid);

			var ids = new[] { tempGuid };
			var @event = new SettingsForPersonPeriodChangedEvent();
			@event.SetIdCollection(ids);

			using (_mocks.Record())
			{
				Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModelData(ids));
			}
			using (_mocks.Playback())
			{
				_target.Handle(@event);
			}
		}
	}
}