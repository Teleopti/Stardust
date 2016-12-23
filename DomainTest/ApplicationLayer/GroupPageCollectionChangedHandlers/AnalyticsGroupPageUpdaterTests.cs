using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	[TestFixture]
	public class AnalyticsGroupPageUpdaterTests
	{
		private Guid _businessUnitId;
		private IGroupPageRepository _groupPageRepository;
		private IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private AnalyticsGroupPageUpdater _target;

		[SetUp]
		public void Setup()
		{
			_groupPageRepository = new FakeGroupPageRepository();
			_analyticsGroupPageRepository = new FakeAnalyticsGroupPageRepository();
			_analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();

			_target = new AnalyticsGroupPageUpdater(_groupPageRepository, _analyticsGroupPageRepository, _analyticsBridgeGroupPagePersonRepository);
			_businessUnitId = Guid.NewGuid();
		}

		[Test]
		public void ShouldDeleteGroupPagesAndBridgeGroupPagePersonWhenAGroupPageIsDeleted()
		{
			var groupPageId = Guid.NewGuid();

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup {GroupPageCode = groupPageId, BusinessUnitCode =  _businessUnitId});

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent {LogOnBusinessUnitId = _businessUnitId, GroupPageIdCollection = { groupPageId } };
			_target.Handle(groupPageCollectionChangedEvent);

			_analyticsGroupPageRepository.GetGroupPage(groupPageId, _businessUnitId).Should().Be.Empty();
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x=>x.DeleteAllBridgeGroupPagePerson(new List<Guid> { groupPageId }, _businessUnitId));
		}

		[Test]
		public void ShouldAddANewGroupPage()
		{
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("Test1").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1","person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Add(groupPage);
			_analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)).Return(new Guid[] { });

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			};
			_target.Handle(groupPageCollectionChangedEvent);

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(groupPageId);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(groupPage.Description.Name);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.DescriptionKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(rootPersonGroup.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.True();

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { personWithGuid.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new Guid[] { }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
		}

		[Test]
		public void ShouldUpdateGroupPage()
		{
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("Test1").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Add(groupPage);
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup
			{
				GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
				GroupPageCode = groupPageId,
				BusinessUnitCode = _businessUnitId
			});
			_analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)).Return(new Guid[] { });

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			};
			_target.Handle(groupPageCollectionChangedEvent);

			var updatedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId);
			updatedGroupPage.Should().Not.Be.Null();
			updatedGroupPage.GroupPageCode.Should().Be.EqualTo(groupPageId);
			updatedGroupPage.GroupPageName.Should().Be.EqualTo(groupPage.Description.Name);
			updatedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.DescriptionKey);
			updatedGroupPage.GroupName.Should().Be.EqualTo(rootPersonGroup.Description.Name);
			updatedGroupPage.GroupIsCustom.Should().Be.True();
		}

		[Test]
		public void ShouldAddPersonInChildGroup()
		{
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("Test1").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var person1 = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(person1);
			var childPersonGroup = new ChildPersonGroup();
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "person2Last");
			childPersonGroup.AddPerson(person2);
			rootPersonGroup.AddChildGroup(childPersonGroup);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Add(groupPage);
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup
			{
				GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
				GroupPageCode = groupPageId,
				BusinessUnitCode = _businessUnitId
			});
			_analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)).Return(new Guid[] { });

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId}
			};
			_target.Handle(groupPageCollectionChangedEvent);

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { person1.Id.GetValueOrDefault(), person2.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new Guid[] { }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
		}

		[Test]
		public void ShouldDeletePersonInChildGroup()
		{
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("Test1").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var person1 = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(person1);
			var childPersonGroup = new ChildPersonGroup();
			rootPersonGroup.AddChildGroup(childPersonGroup);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Add(groupPage);
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup
			{
				GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
				GroupPageCode = groupPageId,
				BusinessUnitCode = _businessUnitId
			});

			var person2 = PersonFactory.CreatePersonWithGuid("person2", "person2Last");
			_analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)).Return(new[] { person2.Id.GetValueOrDefault() });

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId}
			};
			_target.Handle(groupPageCollectionChangedEvent);

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { person1.Id.GetValueOrDefault()  }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new[] { person2.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
		}
	}
}