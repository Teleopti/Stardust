using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers
{
#pragma warning disable 618
	[EnabledBy(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623,
			 Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439),
		DisabledBy(Toggles.GroupPageCollection_ToHangfire_38178)]
	public class AnalyticsGroupPageUpdaterOnServicebus : AnalyticsGroupPageUpdaterBase,
	IHandleEvent<GroupPageCollectionChangedEvent>,
	IRunOnServiceBus
#pragma warning restore 618
	{
		public AnalyticsGroupPageUpdaterOnServicebus(
			IGroupPageRepository groupPageRepository,
			IAnalyticsGroupPageRepository analyticsGroupPageRepository,
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository) : 
			base(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository)
		{
		}

		[AnalyticsUnitOfWork]
		public new virtual void Handle(GroupPageCollectionChangedEvent @event)
		{
			base.Handle(@event);
		}
	}

	[EnabledBy(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623,
			 Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439,
			Toggles.GroupPageCollection_ToHangfire_38178)]
	public class AnalyticsGroupPageUpdaterOnHangfire : AnalyticsGroupPageUpdaterBase,
	IHandleEvent<GroupPageCollectionChangedEvent>,
	IRunOnHangfire
	{
		public AnalyticsGroupPageUpdaterOnHangfire(IGroupPageRepository groupPageRepository, 
			IAnalyticsGroupPageRepository analyticsGroupPageRepository,
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository) : 
			base(groupPageRepository, analyticsGroupPageRepository, analyticsBridgeGroupPagePersonRepository)
		{
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		public new virtual void Handle(GroupPageCollectionChangedEvent @event)
		{
			base.Handle(@event);
		}
	}

	public class AnalyticsGroupPageUpdaterBase
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsGroupPageUpdaterBase));
		private readonly IGroupPageRepository _groupPageRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;

		public AnalyticsGroupPageUpdaterBase(IGroupPageRepository groupPageRepository, IAnalyticsGroupPageRepository analyticsGroupPageRepository, IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository)
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
				logger.Debug($"Did not find any group pages in APP database, deleting anything we have in analytics for group pages: {string.Join(",", @event.GroupPageIdCollection)}.");
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
							logger.Debug($"Updating group page {analyticsGroupPage.GroupPageCode}, group {analyticsGroupPage.GroupCode}");
							_analyticsGroupPageRepository.UpdateGroupPage(analyticsGroupPage);
						}
						else
						{
							logger.Debug($"Creating group page {analyticsGroupPage.GroupPageCode}, group {analyticsGroupPage.GroupCode}");
							_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);
						}
						var people = getRecursively(rootGroup.ChildGroupCollection, rootGroup.PersonCollection.ToList()).Select(x => x.Id.GetValueOrDefault()).ToList();
						var currentPersonCodesInGroupPage = _analyticsBridgeGroupPagePersonRepository.GetBridgeGroupPagePerson(rootGroupId);
						var toBeAdded = people.Where(x => !currentPersonCodesInGroupPage.Contains(x)).ToList();
						var toBeDeleted = currentPersonCodesInGroupPage.Where(x => !people.Contains(x)).ToList();

						if (toBeAdded.Any())
							logger.Debug($"Adding {toBeAdded.Count} people to group {rootGroupId}");
						_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePerson(toBeAdded, rootGroupId);
						if (toBeDeleted.Any())
							logger.Debug($"Removing {toBeDeleted.Count} people from group {rootGroupId}");
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