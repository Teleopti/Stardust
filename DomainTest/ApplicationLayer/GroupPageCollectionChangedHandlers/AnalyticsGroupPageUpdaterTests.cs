using System;
using System.Linq;
using NUnit.Framework;
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
		private FakeAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private AnalyticsGroupPageUpdater _target;

		[SetUp]
		public void Setup()
		{
			_groupPageRepository = new FakeGroupPageRepository();
			_analyticsGroupPageRepository = new FakeAnalyticsGroupPageRepository();
			_analyticsBridgeGroupPagePersonRepository = new FakeAnalyticsBridgeGroupPagePersonRepository();

			_target = new AnalyticsGroupPageUpdater(_groupPageRepository, _analyticsGroupPageRepository, _analyticsBridgeGroupPagePersonRepository);
			_businessUnitId = Guid.NewGuid();
		}

		[Test]
		public void ShouldDeleteGroupPagesAndBridgeGroupPagePersonWhenAGroupPageIsDeleted()
		{
			var groupPageId = Guid.NewGuid();

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup {GroupPageCode = groupPageId, BusinessUnitCode =  _businessUnitId});
			_analyticsBridgeGroupPagePersonRepository.Has(new AnalyticsBridgeGroupPagePerson {BusinessUnitCode = _businessUnitId, GroupPageCode = groupPageId, PersonCode = Guid.NewGuid(), GroupCode = Guid.NewGuid(),PersonId = 1});

			_target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = {groupPageId}
			});

			_analyticsGroupPageRepository.GetGroupPage(groupPageId, _businessUnitId).Should().Be.Empty();
			_analyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddANewGroupPage()
		{
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("Test1").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Add(groupPage);
			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(personWithGuid.Id.GetValueOrDefault(), 1);
			_target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			});

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(groupPageId);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(groupPage.Description.Name);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.DescriptionKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(rootPersonGroup.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.True();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault());
			bridge.PersonCode.Should().Be.EqualTo(personWithGuid.Id.GetValueOrDefault());
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
			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(personWithGuid.Id.GetValueOrDefault(), 1);

			_target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			});

			var updatedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId);
			updatedGroupPage.Should().Not.Be.Null();
			updatedGroupPage.GroupPageCode.Should().Be.EqualTo(groupPageId);
			updatedGroupPage.GroupPageName.Should().Be.EqualTo(groupPage.Description.Name);
			updatedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.DescriptionKey);
			updatedGroupPage.GroupName.Should().Be.EqualTo(rootPersonGroup.Description.Name);
			updatedGroupPage.GroupIsCustom.Should().Be.True();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault());
			bridge.PersonCode.Should().Be.EqualTo(personWithGuid.Id.GetValueOrDefault());
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
			_analyticsBridgeGroupPagePersonRepository
				.WithPersonMapping(person1.Id.GetValueOrDefault(), 1)
				.WithPersonMapping(person2.Id.GetValueOrDefault(), 2);

			_target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			});

			_analyticsBridgeGroupPagePersonRepository.Bridges.ForEach(x => x.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.Bridges.ForEach(x => x.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.Bridges.Any(x => x.PersonCode == person1.Id.GetValueOrDefault()).Should().Be.True();
			_analyticsBridgeGroupPagePersonRepository.Bridges.Any(x => x.PersonCode == person2.Id.GetValueOrDefault()).Should().Be.True();
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
			_analyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
					GroupPageCode = groupPageId,
					PersonCode = person2.Id.GetValueOrDefault(),
					PersonId = 2
				})
				.WithPersonMapping(person1.Id.GetValueOrDefault(), 1)
				.WithPersonMapping(person2.Id.GetValueOrDefault(), 2);

			_target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			});

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault());
			bridge.PersonCode.Should().Be.EqualTo(person1.Id.GetValueOrDefault());
		}
	}
}