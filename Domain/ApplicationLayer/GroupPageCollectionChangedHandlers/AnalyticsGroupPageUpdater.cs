using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	public class AnalyticsGroupPageUpdater :
		IHandleEvent<GroupPageCollectionChangedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsGroupPageUpdater));
		private readonly IGroupPageRepository _groupPageRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;

		public AnalyticsGroupPageUpdater(IGroupPageRepository groupPageRepository, 
			IAnalyticsGroupPageRepository analyticsGroupPageRepository,
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository) 
		{
			_groupPageRepository = groupPageRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_analyticsBridgeGroupPagePersonRepository = analyticsBridgeGroupPagePersonRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(GroupPageCollectionChangedEvent @event)
		{
			var groupPages = _groupPageRepository.LoadGroupPagesByIds(@event.GroupPageIdCollection);

			if (!groupPages.Any())
			{
				logger.Debug($"Did not find any group pages in APP database, deleting anything we have in analytics for group pages: {string.Join(",", @event.GroupPageIdCollection)}.");
				_analyticsBridgeGroupPagePersonRepository.DeleteAllBridgeGroupPagePerson(@event.GroupPageIdCollection, @event.LogOnBusinessUnitId);
				_analyticsGroupPageRepository.DeleteGroupPages(@event.GroupPageIdCollection, @event.LogOnBusinessUnitId);
			}
			else
			{
				foreach (var groupPage in groupPages)
				{
					var existingInAnalytics = _analyticsGroupPageRepository.GetGroupPage(groupPage.Id.GetValueOrDefault(), @event.LogOnBusinessUnitId).ToList();
					foreach (var rootGroup in groupPage.RootGroupCollection)
					{
						var rootGroupId = rootGroup.Id.GetValueOrDefault();
						var analyticsGroupPage = new AnalyticsGroup
						{
							GroupPageCode = groupPage.Id.GetValueOrDefault(),
							GroupPageName = groupPage.Description.Name,
							GroupPageNameResourceKey = groupPage.DescriptionKey,
							GroupCode = rootGroupId,
							GroupName = rootGroup.Name,
							GroupIsCustom = true,
							BusinessUnitCode = @event.LogOnBusinessUnitId
						};
						if (existingInAnalytics.Any(x => x.GroupCode == rootGroup.Id))
						{
							logger.Debug($"Updating group page {analyticsGroupPage.GroupPageCode}, group {analyticsGroupPage.GroupCode}");
							_analyticsGroupPageRepository.UpdateGroupPage(analyticsGroupPage);
						}
						else
						{
							logger.Debug($"Creating group page {analyticsGroupPage.GroupPageCode}, group {analyticsGroupPage.GroupCode}");
							_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);
						}
						var people = getRecursively(rootGroup.ChildGroupCollection, rootGroup.PersonCollection.ToList()).Select(x => x.Id.GetValueOrDefault()).ToList();
						var currentPersonCodesInGroupPage = _analyticsBridgeGroupPagePersonRepository.GetBridgeGroupPagePerson(rootGroupId, @event.LogOnBusinessUnitId).ToArray();
						var toBeAdded = people.Except(currentPersonCodesInGroupPage).ToArray();
						var toBeDeleted = currentPersonCodesInGroupPage.Except(people).ToArray();

						if (toBeAdded.Any())
							logger.Debug($"Adding {toBeAdded.Length} people to group {rootGroupId}");
						_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePerson(toBeAdded, rootGroupId, @event.LogOnBusinessUnitId);
						if (toBeDeleted.Any())
							logger.Debug($"Removing {toBeDeleted.Length} people from group {rootGroupId}");
						_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePerson(toBeDeleted, rootGroupId, @event.LogOnBusinessUnitId);
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