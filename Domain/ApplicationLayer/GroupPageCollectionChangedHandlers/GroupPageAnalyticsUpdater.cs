using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623,
			 Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439),
		UseNotOnToggle(Toggles.GroupPageCollection_ToHangfire_38178)]
	public class GroupPageAnalyticsUpdaterOnServicebus : GroupPageAnalyticsUpdaterBase,
	IHandleEvent<GroupPageCollectionChangedEvent>,
	IRunOnServiceBus
	{
		public GroupPageAnalyticsUpdaterOnServicebus(IGroupPageRepository groupPageRepository, IAnalyticsGroupPageRepository analyticsGroupPageRepository, 
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository) : base(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository)
		{
		}

		public new void Handle(GroupPageCollectionChangedEvent @event)
		{
			base.Handle(@event);
		}
	}

	[UseOnToggle(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623,
			 Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439,
			Toggles.GroupPageCollection_ToHangfire_38178)]
	public class GroupPageAnalyticsUpdaterOnHangfire : GroupPageAnalyticsUpdaterBase,
	IHandleEvent<GroupPageCollectionChangedEvent>,
	IRunOnHangfire
	{
		public GroupPageAnalyticsUpdaterOnHangfire (IGroupPageRepository groupPageRepository, IAnalyticsGroupPageRepository analyticsGroupPageRepository, 
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository) : base(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository)
		{
		}

		[UnitOfWork]
		public new virtual void Handle(GroupPageCollectionChangedEvent @event)
		{
			base.Handle(@event);
		}
	}

	public class GroupPageAnalyticsUpdaterBase
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(GroupPageAnalyticsUpdaterBase));
		private readonly IGroupPageRepository _groupPageRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;

		public GroupPageAnalyticsUpdaterBase(IGroupPageRepository groupPageRepository, IAnalyticsGroupPageRepository analyticsGroupPageRepository, IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository)
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
				logger.DebugFormat("Did not find any group pages in APP database, deleting anything we have in analytics for group pages: {0}.",
						string.Join(",", @event.GroupPageIdCollection));
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
						var rootGroupId = rootGroup.Id.GetValueOrDefault();
						var analyticsGroupPage = new AnalyticsGroup
						{
							GroupPageCode = groupPage.Id.GetValueOrDefault(),
							GroupPageName = groupPage.Description.Name,
							GroupPageNameResourceKey = groupPage.DescriptionKey,
							GroupCode = rootGroupId,
							GroupName = rootGroup.Description.Name,
							GroupIsCustom = true,
							BusinessUnitCode = @event.LogOnBusinessUnitId
						};
						if (existingInAnalytics.Any(x => x.GroupCode == rootGroup.Id))
						{
							logger.DebugFormat("Updating group page {0}, group {1}", analyticsGroupPage.GroupPageCode, analyticsGroupPage.GroupCode);
							_analyticsGroupPageRepository.UpdateGroupPage(analyticsGroupPage);
						}
						else
						{
							logger.DebugFormat("Creating group page {0}, group {1}", analyticsGroupPage.GroupPageCode, analyticsGroupPage.GroupCode);
							_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);
						}
						var people = getRecursively(rootGroup.ChildGroupCollection, rootGroup.PersonCollection.ToList()).Select(x => x.Id.GetValueOrDefault()).ToList();
						var currentPersonCodesInGroupPage = _analyticsBridgeGroupPagePersonRepository.GetBridgeGroupPagePerson(rootGroupId);
						var toBeAdded = people.Where(x => !currentPersonCodesInGroupPage.Contains(x)).ToList();
						var toBeDeleted = currentPersonCodesInGroupPage.Where(x => !people.Contains(x)).ToList();

						if (toBeAdded.Any())
							logger.DebugFormat("Adding {0} people to group {1}", toBeAdded.Count, rootGroupId);
						_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePerson(toBeAdded, rootGroupId);
						if (toBeDeleted.Any())
							logger.DebugFormat("Removing {0} people from group {1}", toBeDeleted.Count, rootGroupId);
						_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePerson(toBeDeleted, rootGroupId);
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