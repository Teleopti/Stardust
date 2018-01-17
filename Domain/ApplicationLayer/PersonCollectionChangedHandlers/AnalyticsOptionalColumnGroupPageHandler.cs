﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class AnalyticsOptionalColumnGroupPageHandler : 
		IHandleEvent<OptionalColumnCollectionChangedEvent>,
		IHandleEvent<PersonCollectionChangedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsOptionalColumnGroupPageHandler));
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private readonly IPersonRepository _personRepository;

		public AnalyticsOptionalColumnGroupPageHandler(
			IOptionalColumnRepository optionalColumnRepository, 
			IAnalyticsGroupPageRepository analyticsGroupPageRepository,
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository,
			IPersonRepository personRepository
			)
		{
			_optionalColumnRepository = optionalColumnRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_analyticsBridgeGroupPagePersonRepository = analyticsBridgeGroupPagePersonRepository;
			_personRepository = personRepository;
		}

		[UnitOfWork, AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(OptionalColumnCollectionChangedEvent @event)
		{
			logger.Debug($"Handle OptionalColumnCollectionChangedEvent for {@event.SerializedOptionalColumn}");
			foreach (var optionalColumnId in  @event.OptionalColumnIdCollection)
			{
				var optionalColumn = _optionalColumnRepository.Get(optionalColumnId);
				if (optionalColumn == null || !optionalColumn.AvailableAsGroupPage)
				{
					logger.Debug($"Either optional column is deleted or it´s not AvailableAsGroupPage. Deleting group page for optional column {optionalColumnId}");
					DeleteGroupPage(optionalColumnId, @event.LogOnBusinessUnitId);
					continue;
				}
				var personValues = _optionalColumnRepository.OptionalColumnValues(optionalColumn);
				var newDimGroupPages = createNewDimGroupPages(optionalColumn, personValues, @event.LogOnBusinessUnitId);
				var newBridgeGroupPagePersons = createNewBridgeGroupPagePerson(personValues, newDimGroupPages);
				logger.Debug($"Prepare insert by deleting group page for optional column {optionalColumnId}");
				_analyticsBridgeGroupPagePersonRepository.DeleteAllBridgeGroupPagePerson(new List<Guid>() {optionalColumnId}, @event.LogOnBusinessUnitId);
				_analyticsGroupPageRepository.DeleteGroupPages(new List<Guid>() { optionalColumnId }, @event.LogOnBusinessUnitId);
				saveNewDimGroupPages(newDimGroupPages);
				saveNewBridgeGroupPagePersons(newBridgeGroupPagePersons, @event.LogOnBusinessUnitId);
			}
			
		}

		[ImpersonateSystem]
		[UnitOfWork, AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PersonCollectionChangedEvent @event)
		{
			var optionalColumns = _optionalColumnRepository.GetOptionalColumns<Person>()
				.Where(x => x.AvailableAsGroupPage)
				.ToList();
			var persons = _personRepository.FindPeopleSimplify(@event.PersonIdCollection);

			foreach (var column in optionalColumns)
			{
				var personColumnValues = new List<IOptionalColumnValue>();
				foreach (var person in persons)
				{
					var personColumnValue = person.OptionalColumnValueCollection
						.SingleOrDefault(x => ((IOptionalColumn)x.Parent).Id.GetValueOrDefault() == column.Id.GetValueOrDefault());
					if (string.IsNullOrWhiteSpace(personColumnValue?.Description))
						continue;
					personColumnValues.Add(personColumnValue);
				}
				_analyticsBridgeGroupPagePersonRepository.DeleteAllForPersons(
					column.Id.GetValueOrDefault(),
					persons.Select(x => x.Id.GetValueOrDefault()).ToList(), 
					@event.LogOnBusinessUnitId);

				IList<AnalyticsGroup> newDimGroupPages = createNewDimGroupPages(column, personColumnValues, @event.LogOnBusinessUnitId);
				saveNewDimGroupPagesOrGet(newDimGroupPages);
				IDictionary<Guid, IList<Guid>> newBridgeGroupPagePersons = createNewBridgeGroupPagePerson(personColumnValues, newDimGroupPages);
				saveNewBridgeGroupPagePersonsWithoutDelete(newBridgeGroupPagePersons, @event.LogOnBusinessUnitId);
			}

			_analyticsGroupPageRepository.DeleteUnusedGroupPages(@event.LogOnBusinessUnitId);
		}

		private void saveNewBridgeGroupPagePersonsWithoutDelete(IDictionary<Guid, IList<Guid>> newBridgeGroupPagePersons, Guid businessUnitId)
		{
			foreach (var bridgeGroupPagePerson in newBridgeGroupPagePersons)
			{
				logger.Debug($"Insert into bridge group page person for {bridgeGroupPagePerson.Value.Count} persons and group {bridgeGroupPagePerson.Key}");
				_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePerson(bridgeGroupPagePerson.Value, bridgeGroupPagePerson.Key, businessUnitId);
			}
		}

		private void saveNewBridgeGroupPagePersons(IDictionary<Guid, IList<Guid>> newBridgeGroupPagePersons, Guid businessUnitId)
		{
			foreach (var bridgeGroupPagePerson in newBridgeGroupPagePersons)
			{
				logger.Debug($"Insert into bridge group page person for {bridgeGroupPagePerson.Value.Count} persons and group {bridgeGroupPagePerson.Key}");
				_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePerson(bridgeGroupPagePerson.Value, bridgeGroupPagePerson.Key, businessUnitId);
				_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePerson(bridgeGroupPagePerson.Value, bridgeGroupPagePerson.Key, businessUnitId);
			}
		}

		private void saveNewDimGroupPages(IList<AnalyticsGroup> newDimGroupPages)
		{
			foreach (var dimGroupPage in newDimGroupPages)
			{
				logger.Debug($"Insert group page for {dimGroupPage.GroupPageName}/{dimGroupPage.GroupName}. Group page code: {dimGroupPage.GroupPageCode}");
				_analyticsGroupPageRepository.AddGroupPageIfNotExisting(dimGroupPage);
			}
		}

		private void saveNewDimGroupPagesOrGet(IList<AnalyticsGroup> newDimGroupPages)
		{
			foreach (var dimGroupPage in newDimGroupPages)
			{
				logger.Debug($"Insert or get group page for {dimGroupPage.GroupPageName}/{dimGroupPage.GroupName}. Group page code: {dimGroupPage.GroupPageCode}");
				var existingGroupPage = _analyticsGroupPageRepository.AddOrGetGroupPage(dimGroupPage);
				if (existingGroupPage == null)
					continue;

				dimGroupPage.GroupId = existingGroupPage.GroupId;
				dimGroupPage.GroupCode = existingGroupPage.GroupCode;
			}
		}

		private IDictionary<Guid, IList<Guid>> createNewBridgeGroupPagePerson(
			IEnumerable<IOptionalColumnValue> personValues, 
			IList<AnalyticsGroup> newDimGroupPages)
		{
			var bridgePersonPeriod = new Dictionary<Guid, IList<Guid>>();

			var groups = newDimGroupPages
				.GroupBy(g => g.GroupName)
				.Select(x => x.Key)
				.ToList();

			var people = personValues.ToLookup(p => p.Description);
			foreach (var groupName in groups)
			{
				var personsInGroup = people[groupName]
					.Select(x => x.ReferenceObject.Id.Value)
					.ToList();
				var groupCode = newDimGroupPages
					.Single(x => x.GroupName == groupName)
					.GroupCode;

				bridgePersonPeriod.Add(groupCode, personsInGroup);
			}

			return bridgePersonPeriod;
		}

		private IList<AnalyticsGroup> createNewDimGroupPages(
			IOptionalColumn optionalColumn, 
			IEnumerable<IOptionalColumnValue> personValues, 
			Guid businessUnitId)
		{
			var dimGroupPages = personValues
				.GroupBy(g => g.Description)
				.Select(s => new AnalyticsGroup
				{
					GroupPageCode = optionalColumn.Id.Value,
					GroupPageName = optionalColumn.Name,
					GroupCode = Guid.NewGuid(),
					GroupName = s.Key,
					GroupIsCustom = false,
					BusinessUnitCode = businessUnitId
				})
				.ToList();

			return dimGroupPages;
		}

		private void DeleteGroupPage(Guid groupPageId, Guid businessUnitId)
		{
			_analyticsBridgeGroupPagePersonRepository.DeleteAllBridgeGroupPagePerson(new[] {groupPageId }, businessUnitId);
			_analyticsGroupPageRepository.DeleteGroupPages(new[] {groupPageId}, businessUnitId);
		}
	}

	internal class AnalyticsGroupPerson : AnalyticsGroup
	{
		public Guid PersonCode { get; set; }
	}
}