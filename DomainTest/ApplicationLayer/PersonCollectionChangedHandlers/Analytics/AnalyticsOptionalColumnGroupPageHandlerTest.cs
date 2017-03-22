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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsOptionalColumnGroupPageHandlerTest
	{
		private AnalyticsOptionalColumnGroupPageHandler _target;
		private FakeOptionalColumnRepository _optionalColumnRepository;
		private FakeAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private Guid _businessUnitId;
		private FakeAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private Guid _entityId;
		private AnalyticsGroup _analyticsGroupPage;
		private AnalyticsBridgeGroupPagePerson _bridgePersonGroupPage;

		[SetUp]
		public void Setup()
		{
			_businessUnitId = Guid.NewGuid();
			_analyticsGroupPageRepository = new FakeAnalyticsGroupPageRepository();
			_optionalColumnRepository = new FakeOptionalColumnRepository();
			_analyticsBridgeGroupPagePersonRepository = new FakeAnalyticsBridgeGroupPagePersonRepository();
			_target = new AnalyticsOptionalColumnGroupPageHandler(
				_optionalColumnRepository, 
				_analyticsGroupPageRepository, 
				_analyticsBridgeGroupPagePersonRepository
				);
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
			
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(_analyticsGroupPage);
			_analyticsBridgeGroupPagePersonRepository
				.Has(_bridgePersonGroupPage)
				.WithPersonMapping(_bridgePersonGroupPage.PersonCode, _bridgePersonGroupPage.PersonId);

			var message = new OptionalColumnCollectionChangedEvent {LogOnBusinessUnitId = _businessUnitId};
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			_target.Handle(message);

			_analyticsBridgeGroupPagePersonRepository.Bridges
				.Should().Be.Empty();
			_analyticsGroupPageRepository.GetGroupPage(_analyticsGroupPage.GroupPageCode, _analyticsGroupPage.BusinessUnitCode)
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

			_optionalColumnRepository.Add(optionalColumn);
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(_analyticsGroupPage);
			_analyticsBridgeGroupPagePersonRepository
				.Has(_bridgePersonGroupPage)
				.WithPersonMapping(_bridgePersonGroupPage.PersonCode, _bridgePersonGroupPage.PersonId);

			var message = new OptionalColumnCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			_target.Handle(message);

			_analyticsBridgeGroupPagePersonRepository.Bridges
				.Should().Be.Empty();
			_analyticsGroupPageRepository.GetGroupPage(_analyticsGroupPage.GroupPageCode, _analyticsGroupPage.BusinessUnitCode)
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

			_optionalColumnRepository.Add(optionalColumn);
			_optionalColumnRepository.AddPersonValues(person1.OptionalColumnValueCollection.First());
			_optionalColumnRepository.AddPersonValues(person2.OptionalColumnValueCollection.First());
			_optionalColumnRepository.AddPersonValues(person3.OptionalColumnValueCollection.First());
			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(person1.Id.Value, 5);
			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(person2.Id.Value, 6);
			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(person3.Id.Value, 7);

			new FakePersonRepositoryLegacy(new IPerson[] {person1, person2, person3});

			var message = new OptionalColumnCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			_target.Handle(message);

			IEnumerable<AnalyticsGroup> analyticsGroups =_analyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.Value, _businessUnitId);
			var groupCode1 = analyticsGroups.Single(x => x.GroupName == "group1").GroupCode;
			var groupCode2 = analyticsGroups.Single(x => x.GroupName == "group2").GroupCode;

			analyticsGroups.Count()
				.Should().Be.EqualTo(2);
			_analyticsBridgeGroupPagePersonRepository.Bridges.Count
				.Should().Be.EqualTo(3);
			analyticsGroups.SingleOrDefault(x => x.GroupPageCode == _entityId && x.GroupPageName == "opt1" && x.GroupName == "group1" && x.GroupIsCustom == false)
				.Should().Not.Be.Null();
			analyticsGroups.SingleOrDefault(x => x.GroupPageCode == _entityId && x.GroupPageName == "opt1" && x.GroupName == "group2" && x.GroupIsCustom == false)
				.Should().Not.Be.Null();
			_analyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person1.Id.Value && x.GroupCode == groupCode1)
				.Should().Not.Be.Null();
			_analyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person2.Id.Value && x.GroupCode == groupCode2)
				.Should().Not.Be.Null();
			_analyticsBridgeGroupPagePersonRepository.Bridges.SingleOrDefault(x => x.PersonCode == person3.Id.Value && x.GroupCode == groupCode2)
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

			_optionalColumnRepository.Add(optionalColumn);

			var message = new OptionalColumnCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			message.SetOptionalColumnIdCollection(new List<Guid>() { _entityId });

			_target.Handle(message);

			IEnumerable<AnalyticsGroup> analyticsGroups = _analyticsGroupPageRepository.GetGroupPage(optionalColumn.Id.Value, _businessUnitId);

			analyticsGroups
				.Should().Be.Empty();
			_analyticsBridgeGroupPagePersonRepository.Bridges
				.Should().Be.Empty();
		}
	}
}