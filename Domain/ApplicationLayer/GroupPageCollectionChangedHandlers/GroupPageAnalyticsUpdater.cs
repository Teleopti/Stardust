using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623,
				 Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439)]
	public class GroupPageAnalyticsUpdater :
		IHandleEvent<GroupPageCollectionChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IGroupPageRepository _groupPageRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;

		public GroupPageAnalyticsUpdater(IGroupPageRepository groupPageRepository, IAnalyticsGroupPageRepository analyticsGroupPageRepository, IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository)
		{
			_groupPageRepository = groupPageRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_analyticsBridgeGroupPagePersonRepository = analyticsBridgeGroupPagePersonRepository;
		}

		public void Handle(GroupPageCollectionChangedEvent @event)
		{
			var groupPages = _groupPageRepository.LoadGroupPagesByIds(@event.GroupPageIdCollection);

			if (!groupPages.Any())
			{
				_analyticsBridgeGroupPagePersonRepository.DeleteAllBridgeGroupPagePerson(@event.GroupPageIdCollection);
				_analyticsGroupPageRepository.DeleteGroupPages(@event.GroupPageIdCollection);
			}
			else
			{
				foreach (var groupPage in groupPages)
				{
					var existingInAnalytics = _analyticsGroupPageRepository.GetGroupPage(groupPage.Id.GetValueOrDefault()).ToList();
					foreach (var rootGroup in groupPage.RootGroupCollection)
					{
						var analyticsGroupPage = new AnalyticsGroup
						{
							GroupPageCode = groupPage.Id.GetValueOrDefault(),
							GroupPageName = groupPage.Description.Name,
							GroupPageNameResourceKey = groupPage.DescriptionKey,
							GroupCode = rootGroup.Id.GetValueOrDefault(),
							GroupName = rootGroup.Description.Name,
							GroupIsCustom = true,
							BusinessUnitCode = @event.LogOnBusinessUnitId
						};
						if (existingInAnalytics.Any(x => x.GroupCode == rootGroup.Id))
						{
							_analyticsGroupPageRepository.UpdateGroupPage(analyticsGroupPage);
						}
						else
						{
							_analyticsGroupPageRepository.AddGroupPage(analyticsGroupPage);
						}
						var people = getRecursively(rootGroup.ChildGroupCollection, rootGroup.PersonCollection.ToList()).Select(x => x.Id.GetValueOrDefault()).ToList();
						var currentPersonCodesInGroupPage = _analyticsBridgeGroupPagePersonRepository.GetBridgeGroupPagePerson(rootGroup.Id.GetValueOrDefault());
						var toBeAdded = people.Where(x => !currentPersonCodesInGroupPage.Contains(x)).ToList();
						var toBeDeleted = currentPersonCodesInGroupPage.Where(x => !people.Contains(x)).ToList();

						_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePerson(toBeAdded, rootGroup.Id.GetValueOrDefault());
						_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePerson(toBeDeleted, rootGroup.Id.GetValueOrDefault());
					}
				}
			}
		}

		private static IEnumerable<IPerson> getRecursively(IEnumerable<IChildPersonGroup> childGroupCollection, List<IPerson> persons)
		{
			foreach (var childPersonGroup in childGroupCollection)
			{
				persons.AddRange(childPersonGroup.PersonCollection);
				getRecursively(childPersonGroup.ChildGroupCollection, persons);
			}
			return persons;
		}
	}
}