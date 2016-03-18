using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	[TestFixture]
	public class GroupingReadModelGroupPageUpdaterTests
	{
		private GroupingReadModelGroupPageUpdater _target;
		private MockRepository _mocks;
		private IGroupingReadOnlyRepository _groupReadOnlyRepository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupReadOnlyRepository = _mocks.DynamicMock<IGroupingReadOnlyRepository>();
			_target = new GroupingReadModelGroupPageUpdater(_groupReadOnlyRepository);
		}

		[Test]
		public void GroupingReadModelGroupPageTest()
		{
			//const string ids = "IDS";
			var TempGuid = Guid.NewGuid();

			Guid[] ids = { TempGuid };

			var message = new GroupPageCollectionChangedEvent();
			message.SetGroupPageIdCollection(ids);

			using (_mocks.Record())
			{
				Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModelGroupPage(ids));
			}
			using (_mocks.Playback())
			{
				_target.Handle(message);
			}
		}
	}
}