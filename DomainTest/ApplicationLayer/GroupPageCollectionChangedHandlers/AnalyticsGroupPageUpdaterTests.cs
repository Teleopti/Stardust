using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsGroupPageUpdaterTests : ISetup
	{
		public AnalyticsGroupPageUpdater Target;
		public IGroupPageRepository GroupPageRepository;
		public IAnalyticsGroupPageRepository AnalyticsGroupPageRepository;
		public FakeAnalyticsBridgeGroupPagePersonRepository AnalyticsBridgeGroupPagePersonRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		private readonly Guid _businessUnitId = Guid.NewGuid();

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsGroupPageUpdater>();
		}

		[Test]
		public void ShouldDeleteGroupPagesAndBridgeGroupPagePersonWhenAGroupPageIsDeleted()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var groupPageId = Guid.NewGuid();

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup {GroupPageCode = groupPageId, BusinessUnitCode =  _businessUnitId});
			AnalyticsBridgeGroupPagePersonRepository.Has(new AnalyticsBridgeGroupPagePerson {BusinessUnitCode = _businessUnitId, GroupPageCode = groupPageId, PersonCode = Guid.NewGuid(), GroupCode = Guid.NewGuid(),PersonId = 1});

			Target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = {groupPageId}
			});

			AnalyticsGroupPageRepository.GetGroupPage(groupPageId, _businessUnitId).Should().Be.Empty();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddANewGroupPage()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("OptimizationData").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			GroupPageRepository.Add(groupPage);
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(personWithGuid.Id.GetValueOrDefault(), 1);

			Target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			});

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(groupPageId);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(groupPage.Description.Name);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.DescriptionKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(rootPersonGroup.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.True();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault());
			bridge.PersonCode.Should().Be.EqualTo(personWithGuid.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldUpdateGroupPage()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("OptimizationData").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			GroupPageRepository.Add(groupPage);
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup
			{
				GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
				GroupPageCode = groupPageId,
				BusinessUnitCode = _businessUnitId
			});
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(personWithGuid.Id.GetValueOrDefault(), 1);

			Target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			});

			var updatedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId);
			updatedGroupPage.Should().Not.Be.Null();
			updatedGroupPage.GroupPageCode.Should().Be.EqualTo(groupPageId);
			updatedGroupPage.GroupPageName.Should().Be.EqualTo(groupPage.Description.Name);
			updatedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.DescriptionKey);
			updatedGroupPage.GroupName.Should().Be.EqualTo(rootPersonGroup.Description.Name);
			updatedGroupPage.GroupIsCustom.Should().Be.True();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault());
			bridge.PersonCode.Should().Be.EqualTo(personWithGuid.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldAddPersonInChildGroup()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("OptimizationData").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var person1 = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(person1);
			var childPersonGroup = new ChildPersonGroup();
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "person2Last");
			childPersonGroup.AddPerson(person2);
			rootPersonGroup.AddChildGroup(childPersonGroup);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			GroupPageRepository.Add(groupPage);
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup
			{
				GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
				GroupPageCode = groupPageId,
				BusinessUnitCode = _businessUnitId
			});
			AnalyticsBridgeGroupPagePersonRepository
				.WithPersonMapping(person1.Id.GetValueOrDefault(), 1)
				.WithPersonMapping(person2.Id.GetValueOrDefault(), 2);

			Target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			});

			AnalyticsBridgeGroupPagePersonRepository.Bridges.ForEach(x => x.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId));
			AnalyticsBridgeGroupPagePersonRepository.Bridges.ForEach(x => x.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault()));
			AnalyticsBridgeGroupPagePersonRepository.Bridges.Any(x => x.PersonCode == person1.Id.GetValueOrDefault()).Should().Be.True();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.Any(x => x.PersonCode == person2.Id.GetValueOrDefault()).Should().Be.True();
		}

		[Test]
		public void ShouldDeletePersonInChildGroup()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			var groupPageId = Guid.NewGuid();
			var groupPage = new GroupPage("OptimizationData").WithId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1").WithId();
			var person1 = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(person1);
			var childPersonGroup = new ChildPersonGroup();
			rootPersonGroup.AddChildGroup(childPersonGroup);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			GroupPageRepository.Add(groupPage);
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup
			{
				GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
				GroupPageCode = groupPageId,
				BusinessUnitCode = _businessUnitId
			});

			var person2 = PersonFactory.CreatePersonWithGuid("person2", "person2Last");
			AnalyticsBridgeGroupPagePersonRepository
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

			Target.Handle(new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId,
				GroupPageIdCollection = { groupPageId }
			});

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault());
			bridge.PersonCode.Should().Be.EqualTo(person1.Id.GetValueOrDefault());
		}
	}
}