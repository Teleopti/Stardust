using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
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
		private readonly IToggleManager _toggleManager;
		private readonly IBadgeSettingProvider _badgeSettingProvider;

		public PortalViewModelFactory(IPermissionProvider permissionProvider, ILicenseActivatorProvider licenseActivatorProviderProvider,
			IPushMessageProvider pushMessageProvider, ILoggedOnUser loggedOnUser, IReportsNavigationProvider reportsNavigationProvider, IBadgeProvider badgeProvider,
			IBadgeSettingProvider badgeSettingProvider, IToggleManager toggleManager)
		{
			_permissionProvider = permissionProvider;
			_licenseActivatorProvider = licenseActivatorProviderProvider;
			_pushMessageProvider = pushMessageProvider;
			_loggedOnUser = loggedOnUser;
			_reportsNavigationProvider = reportsNavigationProvider;
			_badgeProvider = badgeProvider;
			_toggleManager = toggleManager;
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
			if (reportsItems != null && (reportsItems.Count.Equals(1) && reportsItems.First().IsMyReport))
			{
				navigationItems.Add(reportsItems.First());
			}
			else if (reportsItems != null) reportsList.AddRange(reportsItems);

			var licenseActivator = _licenseActivatorProvider.Current();
			var customerName = licenseActivator == null ? string.Empty : licenseActivator.CustomerName;

			IEnumerable<IAgentBadge> badges = null;

			var badgeEnabled = false;
			var badgeToggleEnabled = _toggleManager.IsEnabled(Toggles.MyTimeWeb_AgentBadge_28913);
			if (badgeToggleEnabled)
			{
				var badgeSettings = _badgeSettingProvider.GetBadgeSettings();
				if (badgeSettings != null)
				{
					badgeEnabled = badgeSettings.EnableBadge;
					badges = _badgeProvider.GetBadges();
				}
			}
			
			return new PortalViewModel
						{
							ReportNavigationItems = reportsList,
							NavigationItems = navigationItems,
							CustomerName = customerName,
							ShowChangePassword = showChangePassword(),
							HasAsmPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger),
							HasSignInAsAnotherUser = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.SignInAsAnotherUser),
							ShowMeridian = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t"),
							Badges = badges == null ? null : badges.Select(x => new BadgeViewModel
							{
								BadgeType = x.BadgeType,
								BronzeBadge = x.BronzeBadge,
								SilverBadge = x.SilverBadge,
								GoldBadge = x.GoldBadge
							}),
							IsBadgesToggleEnabled = badgeToggleEnabled,
							IsBadgeFeatureEnabled = badgeEnabled
						};
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

		private NavigationItem createPreferenceNavigationItem()
		{
			return new NavigationItem
						{
							Action = "Index",
							Controller = "Preference",
							Title = Resources.Preference,
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
