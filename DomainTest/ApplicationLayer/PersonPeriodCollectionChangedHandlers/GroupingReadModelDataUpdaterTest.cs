using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonPeriodCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonPeriodCollectionChangedHandlers
{
	[TestFixture]
	public class GroupingReadModelDataUpdaterTest
	{
		private GroupingReadModelDataUpdater _target;
		private MockRepository _mocks;
		private IGroupingReadOnlyRepository _groupReadOnlyRepository;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupReadOnlyRepository = _mocks.DynamicMock<IGroupingReadOnlyRepository>();

			_target = new GroupingReadModelDataUpdater(_groupReadOnlyRepository);
		}


		[Test]
		public void GroupingReadModelDataTest()
		{
			var skillTest = SkillFactory.CreateSkill("Test3");
			var tempGuid = Guid.NewGuid();
			skillTest.SetId(tempGuid);

			var ids = new[] { tempGuid };
			var @event = new PersonPeriodCollectionChangedEvent();
			@event.SetPersonIdCollection(ids);

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