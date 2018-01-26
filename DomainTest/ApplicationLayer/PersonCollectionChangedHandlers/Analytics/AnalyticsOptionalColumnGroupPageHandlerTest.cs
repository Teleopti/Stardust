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
using Teleopti.Ccc.TestCommon.FakeData;
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
		public FakePersonRepository PersonRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

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
			person1.SetOptionalColumnValue(new OptionalColumnValue("group1"), optionalColumn);
			var person2 = new Person().WithId();
			person2.SetOptionalColumnValue(new OptionalColumnValue("group2"), optionalColumn);
			var person3 = new Person().WithId();
			person3.SetOptionalColumnValue(new OptionalColumnValue("group2"), optionalColumn);

			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(person1.OptionalColumnValueCollection.First());
			OptionalColumnRepository.AddPersonValues(person2.OptionalColumnValueCollection.First());
			OptionalColumnRepository.AddPersonValues(person3.OptionalColumnValueCollection.First());
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(person1.Id.GetValueOrDefault(), 5);
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(person2.Id.GetValueOrDefault(), 6);
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(person3.Id.GetValueOrDefault(), 7);

			new FakePersonRepositoryLegacy(person1, person2, person3);

			var message = new OptionalColumnCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			Target.Handle(message);

			IEnumerable<AnalyticsGroup> analyticsGroups =AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.GetValueOrDefault(), _businessUnitId).ToList();
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
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person1.Id.GetValueOrDefault() && x.GroupCode == groupCode1)
				.Should().Not.Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person2.Id.GetValueOrDefault() && x.GroupCode == groupCode2)
				.Should().Not.Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person3.Id.GetValueOrDefault() && x.GroupCode == groupCode2)
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

			IEnumerable<AnalyticsGroup> analyticsGroups = AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.GetValueOrDefault(), _businessUnitId);

			analyticsGroups
				.Should().Be.Empty();
			AnalyticsBridgeGroupPagePersonRepository.Bridges
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldCreateGroupPageForPerson()
		{
			IOptionalColumn optionalColumn = new OptionalColumn("opt1")
			{
				AvailableAsGroupPage = true
			};
			optionalColumn.SetId(_entityId);
			var person1 = new Person().WithId();
			person1.SetOptionalColumnValue(new OptionalColumnValue("group1"), optionalColumn);

			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(person1.OptionalColumnValueCollection.First());
			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(person1.Id.GetValueOrDefault(), 5);
			AnalyticsGroupPageRepository.AnalyticsBridgeGroupPagePersonRepository = AnalyticsBridgeGroupPagePersonRepository;
			PersonRepository.Add(person1);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			var @event = new OptionalColumnValueChangedEvent()
			{
				LogOnBusinessUnitId = _businessUnitId,
				PersonId = person1.Id.GetValueOrDefault()
			};

			Target.Handle(@event);

			IEnumerable<AnalyticsGroup> analyticsGroups = AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.GetValueOrDefault(), _businessUnitId).ToList();
			var groupCode1 = analyticsGroups.Single(x => x.GroupName == "group1").GroupCode;

			analyticsGroups.Count()
				.Should().Be.EqualTo(1);
			analyticsGroups.SingleOrDefault(x => x.GroupPageCode == _entityId && x.GroupPageName == "opt1" && x.GroupName == "group1" && x.GroupIsCustom == false)
				.Should().Not.Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.Count
				.Should().Be.EqualTo(1);
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person1.Id.GetValueOrDefault() && x.GroupCode == groupCode1)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdateExistingGroupPageForOneChangedPerson()
		{
			var person1 = new Person();
			person1.SetId(_bridgePersonGroupPage.PersonCode);
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(_analyticsGroupPage);
			AnalyticsBridgeGroupPagePersonRepository
				.Has(_bridgePersonGroupPage)
				.WithPersonMapping(_bridgePersonGroupPage.PersonCode, _bridgePersonGroupPage.PersonId);
			AnalyticsGroupPageRepository.AnalyticsBridgeGroupPagePersonRepository = AnalyticsBridgeGroupPagePersonRepository;

			IOptionalColumn optionalColumn = new OptionalColumn(_analyticsGroupPage.GroupPageName)
			{
				AvailableAsGroupPage = true
			};
			optionalColumn.SetId(_entityId);
			person1.SetOptionalColumnValue(new OptionalColumnValue("My new group"), optionalColumn);

			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(person1.OptionalColumnValueCollection.First());
			PersonRepository.Add(person1);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			var @event = new OptionalColumnValueChangedEvent()
			{
				LogOnBusinessUnitId = _businessUnitId,
				PersonId = person1.Id.GetValueOrDefault()
			};

			Target.Handle(@event);

			IEnumerable<AnalyticsGroup> analyticsGroups = AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.GetValueOrDefault(), _businessUnitId).ToList();
			var groupCode1 = analyticsGroups.Single(x => x.GroupName == "My new group").GroupCode;

			analyticsGroups.Count()
				.Should().Be.EqualTo(1);
			analyticsGroups.SingleOrDefault(x => x.GroupPageCode == _entityId && x.GroupPageName == "opt1" && x.GroupName == "My new group" && x.GroupIsCustom == false)
				.Should().Not.Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.Count
				.Should().Be.EqualTo(1);
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person1.Id.GetValueOrDefault() && x.GroupCode == groupCode1)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRemoveUnusedGroupPage()
		{
			var person1 = new Person();
			person1.SetId(_bridgePersonGroupPage.PersonCode);

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(_analyticsGroupPage);
			AnalyticsBridgeGroupPagePersonRepository
				.Has(_bridgePersonGroupPage)
				.WithPersonMapping(_bridgePersonGroupPage.PersonCode, _bridgePersonGroupPage.PersonId);
			AnalyticsGroupPageRepository.AnalyticsBridgeGroupPagePersonRepository = AnalyticsBridgeGroupPagePersonRepository;

			IOptionalColumn optionalColumn = new OptionalColumn(_analyticsGroupPage.GroupPageName)
			{
				AvailableAsGroupPage = true
			};
			optionalColumn.SetId(_entityId);

			OptionalColumnRepository.Add(optionalColumn);
			PersonRepository.Add(person1);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			var @event = new OptionalColumnValueChangedEvent()
			{
				LogOnBusinessUnitId = _businessUnitId,
				PersonId = person1.Id.GetValueOrDefault()
			};

			Target.Handle(@event);

			var analyticsGroups = AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.GetValueOrDefault(), _businessUnitId).ToList();
			analyticsGroups.Any().Should().Be.False();
		}

		[Test]
		public void ShouldNotChangeExistingGroupPageWhenNoChangesToOptionalColumnValue()
		{
			var person1 = new Person();
			person1.SetId(_bridgePersonGroupPage.PersonCode);
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(_analyticsGroupPage);
			AnalyticsBridgeGroupPagePersonRepository
				.Has(_bridgePersonGroupPage)
				.WithPersonMapping(_bridgePersonGroupPage.PersonCode, _bridgePersonGroupPage.PersonId);
			AnalyticsGroupPageRepository.AnalyticsBridgeGroupPagePersonRepository = AnalyticsBridgeGroupPagePersonRepository;

			IOptionalColumn optionalColumn = new OptionalColumn(_analyticsGroupPage.GroupPageName)
			{
				AvailableAsGroupPage = true
			};
			optionalColumn.SetId(_entityId);
			person1.SetOptionalColumnValue(new OptionalColumnValue(_analyticsGroupPage.GroupName), optionalColumn);

			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(person1.OptionalColumnValueCollection.First());
			PersonRepository.Add(person1);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			var @event = new OptionalColumnValueChangedEvent()
			{
				LogOnBusinessUnitId = _businessUnitId,
				PersonId = person1.Id.GetValueOrDefault()
			};

			Target.Handle(@event);

			IEnumerable<AnalyticsGroup> analyticsGroups = AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.GetValueOrDefault(), _businessUnitId).ToList();
			var groupCode = analyticsGroups.Single(x => x.GroupPageCode == optionalColumn.Id &&  x.GroupName == _analyticsGroupPage.GroupName).GroupCode;

			analyticsGroups.Count()
				.Should().Be.EqualTo(1);
			analyticsGroups.SingleOrDefault(x => x.GroupPageCode == _entityId && x.GroupPageName == "opt1" && x.GroupName == _analyticsGroupPage.GroupName && x.GroupIsCustom == false)
				.Should().Not.Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.Count
				.Should().Be.EqualTo(1);
			AnalyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person1.Id.GetValueOrDefault() && x.GroupCode == groupCode)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldTreatEmptyPersonValueAsNoValueWhenCreatingGroupPage()
		{
			IOptionalColumn optionalColumn = new OptionalColumn("opt1")
			{
				AvailableAsGroupPage = true
			};
			optionalColumn.SetId(_entityId);
			var person1 = new Person().WithId();
			person1.SetOptionalColumnValue(new OptionalColumnValue(""), optionalColumn);

			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(person1.OptionalColumnValueCollection.First());
			PersonRepository.Add(person1);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			AnalyticsGroupPageRepository.AnalyticsBridgeGroupPagePersonRepository = AnalyticsBridgeGroupPagePersonRepository;
			AnalyticsBridgeGroupPagePersonRepository
				.WithPersonMapping(person1.Id.GetValueOrDefault(), 5);

			var @event = new OptionalColumnValueChangedEvent()
			{
				LogOnBusinessUnitId = _businessUnitId,
				PersonId = person1.Id.GetValueOrDefault()
			};

			Target.Handle(@event);

			IEnumerable<AnalyticsGroup> analyticsGroups = AnalyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.GetValueOrDefault(), _businessUnitId).ToList();
			analyticsGroups.Any().Should().Be.False();
		}
	}
}