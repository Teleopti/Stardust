using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class AnalyticsOptionalColumnGroupPageHandler : IHandleEvent<OptionalColumnCollectionChangedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsOptionalColumnGroupPageHandler));
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;

		public AnalyticsOptionalColumnGroupPageHandler(
			IOptionalColumnRepository optionalColumnRepository, 
			IAnalyticsGroupPageRepository analyticsGroupPageRepository,
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository
			)
		{
			_optionalColumnRepository = optionalColumnRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_analyticsBridgeGroupPagePersonRepository = analyticsBridgeGroupPagePersonRepository;
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

		private void saveNewBridgeGroupPagePersons(IDictionary<Guid, IList<Guid>> newBridgeGroupPagePersons, Guid businessUnitId)
		{
			foreach (var bridgeGroupPagePerson in newBridgeGroupPagePersons)
			{
				logger.Debug($"Insert into bridge group page person for {bridgeGroupPagePerson.Value.Count} persons and group {bridgeGroupPagePerson.Key}");
				_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePerson(bridgeGroupPagePerson.Value, bridgeGroupPagePerson.Key, businessUnitId);
			}
		}

		private void saveNewDimGroupPages(IList<AnalyticsGroupPerson> newDimGroupPages)
		{
			foreach (var dimGroupPage in newDimGroupPages)
			{
				logger.Debug($"Insert group page for {dimGroupPage.GroupPageName}/{dimGroupPage.GroupName}. Group page code: {dimGroupPage.GroupPageCode}");
				_analyticsGroupPageRepository.AddGroupPageIfNotExisting(dimGroupPage);
			}
		}

		private IDictionary<Guid, IList<Guid>> createNewBridgeGroupPagePerson(
			IEnumerable<IOptionalColumnValue> personValues, 
			IList<AnalyticsGroupPerson> newDimGroupPages)
		{
			var bridgePersonPeriod = new Dictionary<Guid, IList<Guid>>();

			var groups = newDimGroupPages
				.GroupBy(g => g.GroupName)
				.Select(x => x.Key)
				.ToList();

			foreach (var groupName in groups)
			{
				var personsInGroup = personValues
					.Where(x => x.Description == groupName)
					.Select(x => x.ReferenceObject.Id.Value)
					.ToList();
				var groupCode = newDimGroupPages
					.Single(x => x.GroupName == groupName)
					.GroupCode;

				bridgePersonPeriod.Add(groupCode, personsInGroup);
			}

			return bridgePersonPeriod;
		}

		private IList<AnalyticsGroupPerson> createNewDimGroupPages(
			IOptionalColumn optionalColumn, 
			IEnumerable<IOptionalColumnValue> personValues, 
			Guid businessUnitId)
		{
			var dimGroupPages = personValues
				.GroupBy(g => g.Description)
				.Select(s => new AnalyticsGroupPerson()
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