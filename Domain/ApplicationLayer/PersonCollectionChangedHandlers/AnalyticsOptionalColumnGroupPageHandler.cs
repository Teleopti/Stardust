using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class AnalyticsOptionalColumnGroupPageHandler : IHandleEvent<OptionalColumnCollectionChangedEvent>,
		IRunOnHangfire
	{
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private readonly Guid _businessUnitId;

		public AnalyticsOptionalColumnGroupPageHandler(
			IOptionalColumnRepository optionalColumnRepository, 
			IAnalyticsGroupPageRepository analyticsGroupPageRepository,
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository,
			Guid businessUnitId)
		{
			_optionalColumnRepository = optionalColumnRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_analyticsBridgeGroupPagePersonRepository = analyticsBridgeGroupPagePersonRepository;
			_businessUnitId = businessUnitId;
		}

		public void Handle(OptionalColumnCollectionChangedEvent @event)
		{
			foreach (var optionalColumnId in  @event.OptionalColumnIdCollection)
			{
				var optionalColumn = _optionalColumnRepository.Get(optionalColumnId);
				if (optionalColumn == null || !optionalColumn.AvailableAsGroupPage)
				{
					DeleteGroupPage(optionalColumnId);
					continue;
				}
				var personValues = _optionalColumnRepository.OptionalColumnValues(optionalColumnId);
				var newDimGroupPages = createNewDimGroupPages(optionalColumn, personValues);
				var newBridgeGroupPagePersons = createNewBridgeGroupPagePerson(personValues, newDimGroupPages);
				_analyticsBridgeGroupPagePersonRepository.DeleteAllBridgeGroupPagePerson(new List<Guid>() {optionalColumnId}, _businessUnitId);
				_analyticsGroupPageRepository.DeleteGroupPages(new List<Guid>() { optionalColumnId }, _businessUnitId);
				saveNewDimGroupPages(newDimGroupPages);
				saveNewBridgeGroupPagePersons(newBridgeGroupPagePersons);
			}
			
		}

		private void saveNewBridgeGroupPagePersons(IDictionary<Guid, IList<Guid>> newBridgeGroupPagePersons)
		{
			foreach (var bridgeGroupPagePerson in newBridgeGroupPagePersons)
			{
				_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePerson(bridgeGroupPagePerson.Value, bridgeGroupPagePerson.Key, _businessUnitId);
			}
		}

		private void saveNewDimGroupPages(IList<AnalyticsGroupPerson> newDimGroupPages)
		{
			foreach (var dimGroupPage in newDimGroupPages)
			{
				_analyticsGroupPageRepository.AddGroupPageIfNotExisting(dimGroupPage);
			}
		}

		private IDictionary<Guid, IList<Guid>> createNewBridgeGroupPagePerson(
			List<IOptionalColumnValue> personValues, 
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

		private IList<AnalyticsGroupPerson> createNewDimGroupPages(IOptionalColumn optionalColumn, IEnumerable<IOptionalColumnValue> personValues)
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
					BusinessUnitCode = _businessUnitId
				})
				.ToList();

			return dimGroupPages;
		}

		private void DeleteGroupPage(Guid groupPageId)
		{
			_analyticsBridgeGroupPagePersonRepository.DeleteAllBridgeGroupPagePerson(new[] {groupPageId }, _businessUnitId);
			_analyticsGroupPageRepository.DeleteGroupPages(new[] {groupPageId}, _businessUnitId);
		}
	}

	internal class AnalyticsGroupPerson : AnalyticsGroup
	{
		public Guid PersonCode { get; set; }
	}
}