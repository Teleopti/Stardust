﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
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
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepo;
		private readonly ICurrentTenantUser _currentTenantUser;
		private readonly IUserCulture _userCulture;
		private readonly ICurrentTeleoptiPrincipal _currentIdentity;
		private readonly ILicenseAvailability _licenseAvailability;
		private readonly IToggleManager _toggleManager;

		public PortalViewModelFactory(IPermissionProvider permissionProvider,
			ILicenseActivatorProvider licenseActivatorProviderProvider,
			IPushMessageProvider pushMessageProvider, ILoggedOnUser loggedOnUser,
			IReportsNavigationProvider reportsNavigationProvider,
			IBadgeProvider badgeProvider,
			IPersonNameProvider personNameProvider,
			ITeamGamificationSettingRepository teamGamificationSettingReop,
			ICurrentTenantUser currentTenantUser,
			IUserCulture userCulture,
			ICurrentTeleoptiPrincipal currentIdentity, IToggleManager toggleManager, ILicenseAvailability licenseAvailability)
		{
			_permissionProvider = permissionProvider;
			_licenseActivatorProvider = licenseActivatorProviderProvider;
			_pushMessageProvider = pushMessageProvider;
			_loggedOnUser = loggedOnUser;
			_reportsNavigationProvider = reportsNavigationProvider;
			_badgeProvider = badgeProvider;
			_personNameProvider = personNameProvider;
			_teamGamificationSettingRepo = teamGamificationSettingReop;
			_currentTenantUser = currentTenantUser;
			_userCulture = userCulture;
			_currentIdentity = currentIdentity;
			_toggleManager = toggleManager;
			_licenseAvailability = licenseAvailability;
		}

		public PortalViewModel CreatePortalViewModel()
		{
			var navigationItems = new List<NavigationItem> { createWeekScheduleNavigationItem() };

			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			var useJalaaliCalendar = culture.IetfLanguageTag == "fa-IR";

			if (useJalaaliCalendar)
			{
				_currentIdentity.Current().Regional.ForceUseGregorianCalendar = true;
				culture = _userCulture.GetCulture(); // overwrite culture before useJalaali flag was set.
			}

			var reportsList = setupNavigationItems(navigationItems, useJalaaliCalendar);

			var licenseActivator = _licenseActivatorProvider.Current();
			var customerName = licenseActivator == null ? string.Empty : licenseActivator.CustomerName;

			var hasBadgePermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewBadge);

			ITeamGamificationSetting teamSetting = null;
			var person = _loggedOnUser.CurrentUser();
			var myTeam = person.MyTeam(DateOnly.Today);
			if (myTeam != null)
			{
				teamSetting =
					_teamGamificationSettingRepo.FindTeamGamificationSettingsByTeam(myTeam);
			}

			var badgeFeatureEnabled = (teamSetting != null);
			var showBadge = badgeFeatureEnabled && hasBadgePermission;
			IEnumerable<BadgeViewModel> badges = null;
			var rollingPeriodSet = GamificationRollingPeriodSet.OnGoing;
			if (showBadge)
			{
				var period = getDefaultPeriod(teamSetting.GamificationSetting);
				badges = _badgeProvider.GetBadges(period);
				rollingPeriodSet = teamSetting.GamificationSetting.RollingPeriodSet;
			}

			var showBadgePeriodNavigator = _toggleManager.IsEnabled(Toggles.WFM_Gamification_Create_Rolling_Periods_74866);
			
			return new PortalViewModel
			{
				ReportNavigationItems = reportsList,
				NavigationItems = navigationItems,
				CustomerName = customerName,
				CurrentLogonAgentName = _personNameProvider.BuildNameFromSetting(person.Name),
				ShowChangePassword = showChangePassword(),
				ShowWFMAppGuide = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewQRCodeForConfiguration)
				&& _toggleManager.IsEnabled(Toggles.MyTimeWeb_ViewWFMAppGuide_43848),
				AsmEnabled =
					_permissionProvider.HasApplicationFunctionPermission(
						DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger) && isAsmLicenseAvailable(),
				ShowMeridian = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t"),
				UseJalaaliCalendar = useJalaaliCalendar,
				DateFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper(),
				TimeFormat = culture.DateTimeFormat.ShortTimePattern,
				AMDesignator = culture.DateTimeFormat.AMDesignator,
				PMDesignator = culture.DateTimeFormat.PMDesignator,
				Badges = badges,
				BadgeRollingPeriodSet = rollingPeriodSet,
				DateTimeDefaultValues = getDateTimeDefaultValues(culture),
				ShowBadge = showBadge,
				ShowBadgePeriodNavigator = showBadgePeriodNavigator,
				DateFormatLocale = getLocale()
			};
		}

		public IEnumerable<BadgeViewModel> GetBadges(DateOnlyPeriod period)
		{
			return  _badgeProvider.GetBadges(period);
		}

		private string getLocale()
		{
			var principal = _currentIdentity.Current();
			var principalCacheable = principal as TeleoptiPrincipalCacheable;
			var regionnal = principalCacheable != null ? principalCacheable.Regional : principal.Regional;
			return regionnal.Culture.Name;
		}

		private DateOnlyPeriod getDefaultPeriod(IGamificationSetting gamificationSetting)
		{
			var onGoingPeriod = new DateOnlyPeriod(new DateOnly(1900, 1, 1), DateOnly.Today);
			if (!_toggleManager.IsEnabled(Toggles.WFM_Gamification_Create_Rolling_Periods_74866)) return onGoingPeriod;

			var firstDayOfWeek = _loggedOnUser.CurrentUser().FirstDayOfWeek;
			DateOnly start, end;

			switch (gamificationSetting.RollingPeriodSet)
			{
				case GamificationRollingPeriodSet.Weekly:
					var diff = DateTime.Today.DayOfWeek - firstDayOfWeek;
					start = diff == 0 ? DateOnly.Today : DateOnly.Today.AddDays(-diff);
					end = start.AddDays(7);
					return new DateOnlyPeriod(start, end);
				case GamificationRollingPeriodSet.Monthly:
					var year = DateTime.Today.Year;
					var month = DateTime.Today.Month;
					start = new DateOnly(year, month, 1);
					end = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
					return new DateOnlyPeriod(start, end);

				default: return onGoingPeriod;
			}
		}

		private IEnumerable<ReportNavigationItem> setupNavigationItems(ICollection<NavigationItem> navigationItems,
			bool useJalaaliCalendar)
		{
			var reportsItems = _reportsNavigationProvider.GetNavigationItems();
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule))
			{
				navigationItems.Add(createTeamScheduleNavigationItem());
			}
			if (
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StudentAvailability) &&
				!useJalaaliCalendar)
			{
				navigationItems.Add(createStudentAvailabilityNavigationItem());
			}
			if (
				(!useJalaaliCalendar || _toggleManager.IsEnabled(Toggles.MyTimeWeb_PreferenceForJalaliCalendar_42965)) &&
				(_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences) ||
				 _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)))
			{
				navigationItems.Add(createPreferenceNavigationItem());
			}
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests) ||
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb) ||
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb))
			{
				navigationItems.Add(createRequestsNavigationItem());
			}
			if (isAsmLicenseAvailable() && _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger))
			{
				navigationItems.Add(createMessageNavigationItem(_pushMessageProvider.UnreadMessageCount));
			}
			var reportsList = new List<ReportNavigationItem>();
			if (reportsItems != null && (reportsItems.Count.Equals(1) && reportsItems.First().IsWebReport))
			{
				navigationItems.Add(reportsItems.First());
			}
			else if (reportsItems != null) reportsList.AddRange(reportsItems);
			return reportsList;
		}

		private DateTimeDefaultValues getDateTimeDefaultValues(CultureInfo culture)
		{
			return new DateTimeDefaultValues
			{
				StartTime = TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0)),
				EndTime = TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0)),
				FullDayStartTime = @TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(0, 0, 0)),
				FullDayEndTime = @TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(23, 59, 0)),
				TodayYear = culture.Calendar.GetYear(DateTime.Today),
				TodayMonth = culture.Calendar.GetMonth(DateTime.Today),
				TodayDay = culture.Calendar.GetDayOfMonth(DateTime.Today)
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

		private bool isAsmLicenseAvailable()
		{
			return _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger);
		}
	}
}