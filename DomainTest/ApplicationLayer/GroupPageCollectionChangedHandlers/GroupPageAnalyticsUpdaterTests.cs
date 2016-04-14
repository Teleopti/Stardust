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
	public class GroupPageAnalyticsUpdaterTests
	{
		[Test]
		public void ShouldDeleteGroupPagesAndBridgeGroupPagePersonWhenAGroupPageIsDeleted()
		{
			var groupPageRepository = MockRepository.GenerateMock<IGroupPageRepository>();
			var groupPageIdCollection = new List<Guid> { Guid.NewGuid()};
			groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] {});
			var analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			var analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();
			var target = new GroupPageAnalyticsUpdater(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository);

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent();
			groupPageCollectionChangedEvent.SetGroupPageIdCollection( groupPageIdCollection);
			target.Handle(groupPageCollectionChangedEvent);

			analyticsGroupPageRepository.AssertWasCalled(x=>x.DeleteGroupPages(groupPageIdCollection));
			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x=>x.DeleteAllBridgeGroupPagePerson(groupPageIdCollection));
		}

		[Test]
		public void ShouldAddANewGroupPage()
		{
			var groupPageRepository = MockRepository.GenerateMock<IGroupPageRepository>();
			var groupPageId = Guid.NewGuid();
			var groupPageIdCollection = new List<Guid> { groupPageId };
			var groupPage = new GroupPage("Test1");
			groupPage.SetId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1");
			rootPersonGroup.SetId(Guid.NewGuid());
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1","person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] { groupPage });
			var analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			analyticsGroupPageRepository.Stub(x => x.GetGroupPage(groupPageId)).Return(new AnalyticsGroup[] {});
			var analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();
			analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault())).Return(new Guid[] { });
			var target = new GroupPageAnalyticsUpdater(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository);

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection(groupPageIdCollection);
			target.Handle(groupPageCollectionChangedEvent);

			analyticsGroupPageRepository.AssertWasCalled(
				c => c.AddGroupPageIfNotExisting(Arg<AnalyticsGroup>.Matches(x => x.GroupPageCode == groupPageId &&
																		 x.GroupPageName == groupPage.Description.Name &&
																		 x.GroupPageNameResourceKey == groupPage.DescriptionKey &&
																		 x.GroupCode == rootPersonGroup.Id.GetValueOrDefault() &&
																		 x.GroupName == rootPersonGroup.Description.Name &&
																		 x.GroupIsCustom &&
																		 x.BusinessUnitCode == groupPageCollectionChangedEvent.LogOnBusinessUnitId)));

			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault()));
			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { personWithGuid.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault()));
			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new Guid[] { }, rootPersonGroup.Id.GetValueOrDefault()));
		}

		[Test]
		public void ShouldUpdateGroupPage()
		{
			var groupPageRepository = MockRepository.GenerateMock<IGroupPageRepository>();
			var groupPageId = Guid.NewGuid();
			var groupPageIdCollection = new List<Guid> { groupPageId };
			var groupPage = new GroupPage("Test1");
			groupPage.SetId(groupPageId);
			var rootPersonGroup = new RootPersonGroup("rootPersonGroup1");
			rootPersonGroup.SetId(Guid.NewGuid());
			var personWithGuid = PersonFactory.CreatePersonWithGuid("person1", "person1Last");
			rootPersonGroup.AddPerson(personWithGuid);
			groupPage.AddRootPersonGroup(rootPersonGroup);
			groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] { groupPage });
			var analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			analyticsGroupPageRepository.Stub(x => x.GetGroupPage(groupPageId)).Return(new[]
			{
				new AnalyticsGroup
				{
					GroupCode = rootPersonGroup.Id.GetValueOrDefault()
				}
			});
			var analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();
			analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault())).Return(new Guid[] { });
			var target = new GroupPageAnalyticsUpdater(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository);

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection(groupPageIdCollection);
			target.Handle(groupPageCollectionChangedEvent);

			analyticsGroupPageRepository.AssertWasCalled(
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
			var groupPageRepository = MockRepository.GenerateMock<IGroupPageRepository>();
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
			groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] { groupPage });
			var analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			analyticsGroupPageRepository.Stub(x => x.GetGroupPage(groupPageId)).Return(new[]
			{
				new AnalyticsGroup
				{
					GroupCode = rootPersonGroup.Id.GetValueOrDefault()
				}
			});
			var analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();
			analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault())).Return(new Guid[] { });
			var target = new GroupPageAnalyticsUpdater(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository);

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection(groupPageIdCollection);
			target.Handle(groupPageCollectionChangedEvent);

			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault()));
			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { person1.Id.GetValueOrDefault(), person2.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault()));
			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new Guid[] { }, rootPersonGroup.Id.GetValueOrDefault()));
		}

		[Test]
		public void ShouldDeletePersonInChildGroup()
		{
			var groupPageRepository = MockRepository.GenerateMock<IGroupPageRepository>();
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
			groupPageRepository.Stub(x => x.LoadGroupPagesByIds(groupPageIdCollection)).Return(new IGroupPage[] { groupPage });
			var analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			analyticsGroupPageRepository.Stub(x => x.GetGroupPage(groupPageId)).Return(new[]
			{
				new AnalyticsGroup
				{
					GroupCode = rootPersonGroup.Id.GetValueOrDefault()
				}
			});
			var analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();
			var person2 = PersonFactory.CreatePersonWithGuid("person2", "person2Last");
			analyticsBridgeGroupPagePersonRepository.Stub(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault())).Return(new[] { person2.Id.GetValueOrDefault() });
			var target = new GroupPageAnalyticsUpdater(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository);

			var groupPageCollectionChangedEvent = new GroupPageCollectionChangedEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid()
			};
			groupPageCollectionChangedEvent.SetGroupPageIdCollection(groupPageIdCollection);
			target.Handle(groupPageCollectionChangedEvent);

			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.GetBridgeGroupPagePerson(rootPersonGroup.Id.GetValueOrDefault()));
			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.AddBridgeGroupPagePerson(new[] { person1.Id.GetValueOrDefault()  }, rootPersonGroup.Id.GetValueOrDefault()));
			analyticsBridgeGroupPagePersonRepository.AssertWasCalled(x => x.DeleteBridgeGroupPagePerson(new[] { person2.Id.GetValueOrDefault() }, rootPersonGroup.Id.GetValueOrDefault()));
		}
	}
}