using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Global.Core;
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
		private readonly IToggleManager _toggleManager;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepo;
		private readonly ICurrentTenantUser _currentTenantUser;
		private readonly IUserCulture _userCulture;

		public PortalViewModelFactory(IPermissionProvider permissionProvider,
			ILicenseActivatorProvider licenseActivatorProviderProvider,
			IPushMessageProvider pushMessageProvider, ILoggedOnUser loggedOnUser,
			IReportsNavigationProvider reportsNavigationProvider,
			IBadgeProvider badgeProvider,
			IToggleManager toggleManager, IPersonNameProvider personNameProvider,
			ITeamGamificationSettingRepository teamGamificationSettingReop,
			ICurrentTenantUser currentTenantUser,
			IUserCulture userCulture)
		{
			_permissionProvider = permissionProvider;
			_licenseActivatorProvider = licenseActivatorProviderProvider;
			_pushMessageProvider = pushMessageProvider;
			_loggedOnUser = loggedOnUser;
			_reportsNavigationProvider = reportsNavigationProvider;
			_badgeProvider = badgeProvider;
			_toggleManager = toggleManager;
			_personNameProvider = personNameProvider;
			_teamGamificationSettingRepo = teamGamificationSettingReop;
			_currentTenantUser = currentTenantUser;
			_userCulture = userCulture;
		}

		public PortalViewModel CreatePortalViewModel()
		{
			var navigationItems = new List<NavigationItem> {createWeekScheduleNavigationItem()};
			var reportsItems = _reportsNavigationProvider.GetNavigationItems();
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule))
			{
				navigationItems.Add(createTeamScheduleNavigationItem());
			}
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StudentAvailability))
			{
				navigationItems.Add(createStudentAvailabilityNavigationItem());
			}
			if (
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences) ||
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

			var badgeToggleEnabled = _toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318);
			var hasBadgePermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewBadge);

			bool badgeFeatureEnabled = false;
			if (_toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318))
			{
				ITeamGamificationSetting teamSetting = null;
				if (_loggedOnUser.CurrentUser().MyTeam(DateOnly.Today) != null)
				{
					teamSetting =
						_teamGamificationSettingRepo.FindTeamGamificationSettingsByTeam(_loggedOnUser.CurrentUser().MyTeam(DateOnly.Today));
				}
				badgeFeatureEnabled = (teamSetting != null);
			}

			var showBadge = badgeToggleEnabled && badgeFeatureEnabled && hasBadgePermission;
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();

			return new PortalViewModel
			{
				ReportNavigationItems = reportsList,
				NavigationItems = navigationItems,
				CustomerName = customerName,
				CurrentLogonAgentName = _personNameProvider.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name),
				ShowChangePassword = showChangePassword(),
				HasAsmPermission =
					_permissionProvider.HasApplicationFunctionPermission(
						DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger),
				ShowMeridian = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t"),
				UseJalaaliCalendar = CultureInfo.CurrentCulture.IetfLanguageTag == "fa-IR",
				DateFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper(),
				TimeFormat = culture.DateTimeFormat.ShortTimePattern,
				AMDesignator = culture.DateTimeFormat.AMDesignator,
				PMDesignator = culture.DateTimeFormat.PMDesignator,
				Badges = showBadge ? _badgeProvider.GetBadges() : null,

				ShowBadge = showBadge
			};
		}

		private bool showChangePassword()
		{
			var currentPersonInfo = _currentTenantUser.CurrentUser();
			return currentPersonInfo != null &&
			       !string.IsNullOrEmpty(_currentTenantUser.CurrentUser().ApplicationLogonInfo.LogonName);
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
