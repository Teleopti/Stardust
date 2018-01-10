using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[DomainTest]
	public class AnalyticsOptionalColumnGroupPageHandlerTest
	{
		public AnalyticsOptionalColumnGroupPageHandler Target;
		public FakeOptionalColumnRepository OptionalColumnRepository;
		public FakeAnalyticsGroupPageRepository AnalyticsGroupPageRepository;
		public FakeAnalyticsBridgeGroupPagePersonRepository AnalyticsBridgeGroupPagePersonRepository;

		private Guid _businessUnitId;
		private Guid _entityId;
		private AnalyticsGroup _analyticsGroupPage;
		private AnalyticsBridgeGroupPagePerson _bridgePersonGroupPage;

		[SetUp]
		public void Setup()
		{
			_businessUnitId = Guid.NewGuid();
			_entityId = Guid.NewGuid();
			_analyticsGroupPage = new AnalyticsGroup
			{
				GroupPageId = 1,
				GroupPageCode = _entityId,
				GroupPageName = "opt1",
				GroupId = 2,
				GroupCode = Guid.NewGuid(),
				GroupName = "A",
				BusinessUnitCode = _businessUnitId
			};
			_bridgePersonGroupPage = new AnalyticsBridgeGroupPagePerson
			{
				GroupPageCode = _analyticsGroupPage.GroupPageCode,
				GroupCode = Guid.NewGuid(),
				PersonCode = Guid.NewGuid(),
				PersonId = 5,
				BusinessUnitCode = _businessUnitId
			};
		}

		[Test]
		public void ShouldDeleteGroupPageWhenEntityIsDeleted()
		{
			
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(_analyticsGroupPage);
			AnalyticsBridgeGroupPagePersonRepository
				.Has(_bridgePersonGroupPage)
				.WithPersonMapping(_bridgePersonGroupPage.PersonCode, _bridgePersonGroupPage.PersonId);

			var message = new OptionalColumnCollectionChangedEvent {LogOnBusinessUnitId = _businessUnitId};
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			Target.Handle(message);

			AnalyticsBridgeGroupPagePersonRepository.Bridges
				.Should().Be.Empty();
			AnalyticsGroupPageRepository.GetGroupPage(_analyticsGroupPage.GroupPageCode, _analyticsGroupPage.BusinessUnitCode)
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteGroupPageWhenEntityIsNotAvailableAsGroupPage()
		{
			IOptionalColumn optionalColumn = new OptionalColumn("opt1")
			{
				AvailableAsGroupPage = false
			};
			optionalColumn.SetId(_entityId);

			OptionalColumnRepository.Add(optionalColumn);
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(_analyticsGroupPage);
			AnalyticsBridgeGroupPagePersonRepository
				.Has(_bridgePersonGroupPage)
				.WithPersonMapping(_bridgePersonGroupPage.PersonCode, _bridgePersonGroupPage.PersonId);

			var message = new OptionalColumnCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			Target.Handle(message);

			AnalyticsBridgeGroupPagePersonRepository.Bridges
				.Should().Be.Empty();
			AnalyticsGroupPageRepository.GetGroupPage(_analyticsGroupPage.GroupPageCode, _analyticsGroupPage.BusinessUnitCode)
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddGroupsAccordingToTheUpdatedOptionalColumn()
		{
			IOptionalColumn optionalColumn = new OptionalColumn("opt1")
			{
				AvailableAsGroupPage = true
			};
			optionalColumn.SetId(_entityId);
			var person1 = new Person().WithId();
			person1.AddOptionalColumnValue(new OptionalColumnValue("group1"), optionalColumn);
			var person2 = new Person().WithId();
			person2.AddOptionalColumnValue(new OptionalColumnValue("group2"), optionalColumn);
			var person3 = new Person().WithId();
			person3.AddOptionalColumnValue(new OptionalColumnValue("group2"), optionalColumn);

			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(person1.OptionalColumnValueCollection.First());
			OptionalColumnRepository.AddPersonValues(person2.OptionalColumnValueCollection.First());
			OptionalColumnRepository.AddPersonValues(person3.OptionalColumnValueCollection.First());
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(person1.Id.Value, 5);
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(person2.Id.Value, 6);
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(person3.Id.Value, 7);

			new FakePersonRepositoryLegacy(new IPerson[] {person1, person2, person3});

			var message = new OptionalColumnCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			Target.Handle(message);

			IEnumerable<AnalyticsGroup> analyticsGroups =AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.Value, _businessUnitId);
			var groupCode1 = analyticsGroups.Single(x => x.GroupName == "group1").GroupCode;
			var groupCode2 = analyticsGroups.Single(x => x.GroupName == "group2").GroupCode;

			analyticsGroups.Count()
				.Should().Be.EqualTo(2);
			AnalyticsBridgeGroupPagePersonRepository.Bridges.Count
				.Should().Be.EqualTo(3);
			analyticsGroups.SingleOrDefault(x => x.GroupPageCode == _entityId && x.GroupPageName == "opt1" && x.GroupName == "group1" && x.GroupIsCustom == false)
				.Should().Not.Be.Null();
			analyticsGroups.SingleOrDefault(x => x.GroupPageCode == _entityId && x.GroupPageName == "opt1" && x.GroupName == "group2" && x.GroupIsCustom == false)
				.Should().Not.Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person1.Id.Value && x.GroupCode == groupCode1)
				.Should().Not.Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person2.Id.Value && x.GroupCode == groupCode2)
				.Should().Not.Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person3.Id.Value && x.GroupCode == groupCode2)
				.Should().Not.Be.Null();

		}

		[Test]
		public void ShouldNotAddGroupPageIfNoOptionalColumnValuesAssigned()
		{
			IOptionalColumn optionalColumn = new OptionalColumn("opt1")
			{
				AvailableAsGroupPage = true
			};
			optionalColumn.SetId(_entityId);

			OptionalColumnRepository.Add(optionalColumn);

			var message = new OptionalColumnCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			Target.Handle(message);

			IEnumerable<AnalyticsGroup> analyticsGroups = AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.Value, _businessUnitId);

			analyticsGroups
				.Should().Be.Empty();
			AnalyticsBridgeGroupPagePersonRepository.Bridges
				.Should().Be.Empty();
		}
	}
}