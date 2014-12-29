using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory
{
	public class PortalViewModelFactory : IPortalViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILicenseActivatorProvider _licenseActivatorProvider;
		private readonly IPushMessageProvider _pushMessageProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IReportsNavigationProvider _reportsNavigationProvider;
		private readonly IBadgeProvider _badgeProvider;
		private readonly IBadgeWithRankProvider _badgeWithRankProvider;
		private readonly IToggleManager _toggleManager;
		private readonly IBadgeSettingProvider _badgeSettingProvider;
		private readonly IPersonNameProvider _personNameProvider;

		public PortalViewModelFactory(IPermissionProvider permissionProvider, ILicenseActivatorProvider licenseActivatorProviderProvider,
			IPushMessageProvider pushMessageProvider, ILoggedOnUser loggedOnUser, IReportsNavigationProvider reportsNavigationProvider,
			IBadgeProvider badgeProvider, IBadgeWithRankProvider badgeWithRankProvider, IBadgeSettingProvider badgeSettingProvider,
			IToggleManager toggleManager, IPersonNameProvider personNameProvider)
		{
			_permissionProvider = permissionProvider;
			_licenseActivatorProvider = licenseActivatorProviderProvider;
			_pushMessageProvider = pushMessageProvider;
			_loggedOnUser = loggedOnUser;
			_reportsNavigationProvider = reportsNavigationProvider;
			_badgeProvider = badgeProvider;
			_badgeWithRankProvider = badgeWithRankProvider;
			_toggleManager = toggleManager;
			_personNameProvider = personNameProvider;
			_badgeSettingProvider = badgeSettingProvider;
		}

		public PortalViewModel CreatePortalViewModel()
		{
			var navigationItems = new List<NavigationItem> { createWeekScheduleNavigationItem() };
			var reportsItems = _reportsNavigationProvider.GetNavigationItems();
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule))
			{
				navigationItems.Add(createTeamScheduleNavigationItem());
			}
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StudentAvailability))
			{
				navigationItems.Add(createStudentAvailabilityNavigationItem());
			}
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences) ||
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb))
			{
				navigationItems.Add(createPreferenceNavigationItem());
			}
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests) ||
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb) ||
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb))
			{
				navigationItems.Add(createRequestsNavigationItem());
			}
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger))
			{
				navigationItems.Add(createMessageNavigationItem(_pushMessageProvider.UnreadMessageCount));
			}
			var reportsList = new List<ReportNavigationItem>();
			if (reportsItems != null && (reportsItems.Count.Equals(1) && reportsItems.First().IsWebReport))
			{
				navigationItems.Add(reportsItems.First());
			}
			else if (reportsItems != null) reportsList.AddRange(reportsItems);

			var licenseActivator = _licenseActivatorProvider.Current();
			var customerName = licenseActivator == null ? string.Empty : licenseActivator.CustomerName;

			var badgeToggleEnabled = _toggleManager.IsEnabled(Toggles.MyTimeWeb_AgentBadge_28913);
			var hasBadgePermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewBadge);
			var badgeSettings = _badgeSettingProvider.GetBadgeSettings() ?? new AgentBadgeSettings();
			var badgeFeatureEnabled = badgeSettings.BadgeEnabled;

			var showBadge = badgeToggleEnabled && badgeFeatureEnabled && hasBadgePermission;
			
			var allBadges = showBadge ? _badgeProvider.GetBadges(badgeSettings) : null;

			var toggleEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);
			var allBadgesWithRank
				= showBadge && toggleEnabled && badgeSettings.CalculateBadgeWithRank
				? _badgeWithRankProvider.GetBadges(badgeSettings)
				: null;

			var totalBadgeVm = mergeBadges(allBadges, allBadgesWithRank,
				badgeSettings.SilverToBronzeBadgeRate, badgeSettings.GoldToSilverBadgeRate);

			return new PortalViewModel
			{
				ReportNavigationItems = reportsList,
				NavigationItems = navigationItems,
				CustomerName = customerName,
				CurrentLogonAgentName = _personNameProvider.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name),
				ShowChangePassword = showChangePassword(),
				HasAsmPermission =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger),
				ShowMeridian = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t"),
				Badges = totalBadgeVm,
				ShowBadge = showBadge
			};
		}

		/// <summary>
		/// Merge total amount of badges.
		/// To handle the scenario that switch from old badge calculation 
		/// </summary>
		/// <param name="allBadges"></param>
		/// <param name="allBadgesWithRank"></param>
		/// <param name="silverToBronzeBadgeRate"></param>
		/// <param name="goldToSilverBadgeRate"></param>
		/// <returns></returns>
		private static IEnumerable<BadgeViewModel> mergeBadges(IEnumerable<IAgentBadge> allBadges,
			IEnumerable<IAgentBadgeWithRank> allBadgesWithRank,
			int silverToBronzeBadgeRate, int goldToSilverBadgeRate)
		{
			// Get badges calculated with old method.
			var agentWithBadgeList = allBadges == null
				? null
				: allBadges.ToDictionary(x => new {x.Person, x.BadgeType}, x => new
				{
					BronzeBadge = x.GetBronzeBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
					SilverBadge = x.GetSilverBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate),
					GoldBadge = x.GetGoldBadge(silverToBronzeBadgeRate, goldToSilverBadgeRate)
				});
			var badgeWithRankList = allBadgesWithRank == null
				? null
				: allBadgesWithRank.ToDictionary(x => new {x.Person, x.BadgeType}, x => new
				{
					BronzeBadge = x.BronzeBadgeAmount,
					SilverBadge = x.SilverBadgeAmount,
					GoldBadge = x.GoldBadgeAmount
				});

			if (agentWithBadgeList == null || agentWithBadgeList.Any())
			{
				// If there is no badges calculated with old method, then return badges with rank.
				agentWithBadgeList = badgeWithRankList;
			}
			else if (badgeWithRankList != null && badgeWithRankList.Any())
			{
				// Else merge the 2 badges together.
				var personHasBadge = badgeWithRankList.Keys;
				foreach (var personAndBadgeType in personHasBadge)
				{
					if (agentWithBadgeList.ContainsKey(personAndBadgeType))
					{
						var agentWithBadgeVm = agentWithBadgeList[personAndBadgeType];
						var badgeWithRank = badgeWithRankList[personAndBadgeType];
						agentWithBadgeList[personAndBadgeType] = new
						{
							BronzeBadge = agentWithBadgeVm.BronzeBadge + badgeWithRank.BronzeBadge,
							SilverBadge = agentWithBadgeVm.SilverBadge + badgeWithRank.SilverBadge,
							GoldBadge = agentWithBadgeVm.GoldBadge + badgeWithRank.GoldBadge
						};
					}
					else
					{
						agentWithBadgeList[personAndBadgeType] = badgeWithRankList[personAndBadgeType];
					}
				}
			}

			var totalBadgeVm = agentWithBadgeList == null
				? null
				: agentWithBadgeList.Select(x => new BadgeViewModel
				{
					BadgeType = x.Key.BadgeType,
					BronzeBadge = x.Value.BronzeBadge,
					SilverBadge = x.Value.SilverBadge,
					GoldBadge = x.Value.GoldBadge
				});
			return totalBadgeVm;
		}

		private bool showChangePassword()
		{
			var agent = _loggedOnUser.CurrentUser();
			return agent != null && agent.ApplicationAuthenticationInfo != null && !string.IsNullOrEmpty(agent.ApplicationAuthenticationInfo.ApplicationLogOnName);
		}

		private static NavigationItem createTeamScheduleNavigationItem()
		{
			return new NavigationItem.TeamScheduleNavigationItem
						{
							Action = "Index",
							Controller = "TeamSchedule",
							Title = Resources.TeamSchedule
						};
		}

		private NavigationItem createRequestsNavigationItem()
		{
			return new NavigationItem
					{
						Action = "Index",
						Controller = "Requests",
						Title = Resources.Requests
					};
		}

		private static NavigationItem createMessageNavigationItem(int unreadMessageCount)
		{
			return new NavigationItem
			{
				Action = "Index",
				Controller = "Message",
				Title = Resources.Messages,
				TitleCount = string.Format(Resources.MessagesParenthesis, unreadMessageCount),
				PayAttention = unreadMessageCount != 0,
				UnreadMessageCount = unreadMessageCount
			};
		}

		private static NavigationItem createMyeReportNavigationItem()
		{
			return new NavigationItem
			{
				Action = "Index",
				Controller = "MyReport",
				Title = Resources.MyReport,
			};
		}

		private static NavigationItem createPreferenceNavigationItem()
		{
			return new NavigationItem
						{
							Action = "Index",
							Controller = "Preference",
							Title = Resources.Preferences,
						};
		}

		private static NavigationItem createStudentAvailabilityNavigationItem()
		{
			return new NavigationItem
					{
						Action = "Index",
						Controller = "Availability",
						Title = Resources.Availability,
					};
		}

		private static NavigationItem createWeekScheduleNavigationItem()
		{
			return new NavigationItem
					{
						Action = "Week",
						Controller = "Schedule",
						Title = Resources.Schedule
					};
		}
	}
}
