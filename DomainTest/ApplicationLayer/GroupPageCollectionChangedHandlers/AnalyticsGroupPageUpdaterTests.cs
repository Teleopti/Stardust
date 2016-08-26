using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	[TestFixture]
	public class AnalyticsGroupPageUpdaterTests
	{
		private Guid _businessUnitId;
		private IGroupPageRepository _groupPageRepository;
		private IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private AnalyticsGroupPageUpdaterBase _target;

		[SetUp]
		public void Setup()
		{
			_groupPageRepository = MockRepository.GenerateMock<IGroupPageRepository>();
			_analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			_analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();

			_target = new AnalyticsGroupPageUpdaterBase(_groupPageRepository, _analyticsGroupPageRepository, _analyticsBridgeGroupPagePersonRepository);
			_businessUnitId = Guid.NewGuid();
		}

		[Test]
		public void ShouldDeleteGroupPagesAndBridgeGroupPagePersonWhenAGroupPageIsDeleted()
		{
			var groupPageIdCollection = new List<Guid> { Guid.NewGuid()};
			_groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] {});

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent {LogOnBusinessUnitId = _businessUnitId};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection( groupPageIdCollection);
			_target.Handle(groupPageCollectionChangedEvent);

			_analyticsGroupPageRepository.AssertWasCalled(x=>x.DeleteGroupPages(groupPageIdCollection, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x=>x.DeleteAllBridgeGroupPagePerson(groupPageIdCollection, _businessUnitId));
		}

		[Test]
		public void ShouldAddANewGroupPage()
		{
			var groupPageId = Guid.NewGuid();
			var groupPageIdCollection = new List<Guid> { groupPageId };
			var groupPage = new GroupPage("Test1");
			groupPage.SetId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1");
			rootPersonGroup.SetId(Guid.NewGuid());
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1","person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] { groupPage });
			_analyticsGroupPageRepository.Stub(x => x.GetGroupPage(groupPageId, _businessUnitId)).Return(new AnalyticsGroup[] {});
			_analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)).Return(new Guid[] { });

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection(groupPageIdCollection);
			_target.Handle(groupPageCollectionChangedEvent);

			_analyticsGroupPageRepository.AssertWasCalled(
				c => c.AddGroupPageIfNotExisting(Arg<AnalyticsGroup>.Matches(x => x.GroupPageCode == groupPageId &&
																		 x.GroupPageName == groupPage.Description.Name &&
																		 x.GroupPageNameResourceKey == groupPage.DescriptionKey &&
																		 x.GroupCode == rootPersonGroup.Id.GetValueOrDefault() &&
																		 x.GroupName == rootPersonGroup.Description.Name &&
																		 x.GroupIsCustom &&
																		 x.BusinessUnitCode == groupPageCollectionChangedEvent.LogOnBusinessUnitId)));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { personWithGuid.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new Guid[] { }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
		}

		[Test]
		public void ShouldUpdateGroupPage()
		{
			var groupPageId = Guid.NewGuid();
			var groupPageIdCollection = new List<Guid> { groupPageId };
			var groupPage = new GroupPage("Test1");
			groupPage.SetId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1");
			rootPersonGroup.SetId(Guid.NewGuid());
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] { groupPage });
			_analyticsGroupPageRepository.Stub(x => x.GetGroupPage(groupPageId, _businessUnitId)).Return(new[]
			{
				new AnalyticsGroup
				{
					GroupCode = rootPersonGroup.Id.GetValueOrDefault()
				}
			});
			_analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)).Return(new Guid[] { });

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection(groupPageIdCollection);
			_target.Handle(groupPageCollectionChangedEvent);

			_analyticsGroupPageRepository.AssertWasCalled(
				c => c.UpdateGroupPage(Arg<AnalyticsGroup>.Matches(x => x.GroupPageCode == groupPageId &&
																		 x.GroupPageName == groupPage.Description.Name &&
																		 x.GroupPageNameResourceKey == groupPage.DescriptionKey &&
																		 x.GroupCode == rootPersonGroup.Id.GetValueOrDefault() &&
																		 x.GroupName == rootPersonGroup.Description.Name &&
																		 x.GroupIsCustom &&
																		 x.BusinessUnitCode == groupPageCollectionChangedEvent.LogOnBusinessUnitId)));
		}

		[Test]
		public void ShouldAddPersonInChildGroup()
		{
			var groupPageId = Guid.NewGuid();
			var groupPageIdCollection = new List<Guid> { groupPageId };
			var groupPage = new GroupPage("Test1");
			groupPage.SetId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1");
			rootPersonGroup.SetId(Guid.NewGuid());
			var person1 = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(person1);
			var childPersonGroup = new ChildPersonGroup();
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "person2Last");
			childPersonGroup.AddPerson(person2);
			rootPersonGroup.AddChildGroup(childPersonGroup);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] { groupPage });
			_analyticsGroupPageRepository.Stub(x => x.GetGroupPage(groupPageId, _businessUnitId)).Return(new[]
			{
				new AnalyticsGroup
				{
					GroupCode = rootPersonGroup.Id.GetValueOrDefault()
				}
			});
			_analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)).Return(new Guid[] { });

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection(groupPageIdCollection);
			_target.Handle(groupPageCollectionChangedEvent);

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { person1.Id.GetValueOrDefault(), person2.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new Guid[] { }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
		}

		[Test]
		public void ShouldDeletePersonInChildGroup()
		{
			var groupPageId = Guid.NewGuid();
			var groupPageIdCollection = new List<Guid> { groupPageId };
			var groupPage = new GroupPage("Test1");
			groupPage.SetId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1");
			rootPersonGroup.SetId(Guid.NewGuid());
			var person1 = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(person1);
			var childPersonGroup = new ChildPersonGroup();
			rootPersonGroup.AddChildGroup(childPersonGroup);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			_groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] { groupPage });
			_analyticsGroupPageRepository.Stub(x => x.GetGroupPage(groupPageId, _businessUnitId)).Return(new[]
			{
				new AnalyticsGroup
				{
					GroupCode = rootPersonGroup.Id.GetValueOrDefault()
				}
			});
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "person2Last");
			_analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)).Return(new[] { person2.Id.GetValueOrDefault() });

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection(groupPageIdCollection);
			_target.Handle(groupPageCollectionChangedEvent);

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { person1.Id.GetValueOrDefault()  }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new[] { person2.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId));
		}
	}
}