using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory
{
	public class PortalViewModelFactory : IPortalViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPreferenceOptionsProvider _preferenceOptionsProvider;
		private readonly ILicenseActivator _licenseActivator;
		private readonly IPushMessageProvider _pushMessageProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public PortalViewModelFactory(IPermissionProvider permissionProvider,
												  IPreferenceOptionsProvider preferenceOptionsProvider,
												  ILicenseActivator licenseActivator,
												  IPushMessageProvider pushMessageProvider,
												  ILoggedOnUser loggedOnUser)
		{
			_permissionProvider = permissionProvider;
			_preferenceOptionsProvider = preferenceOptionsProvider;
			_licenseActivator = licenseActivator;
			_pushMessageProvider = pushMessageProvider;
			_loggedOnUser = loggedOnUser;
		}

		public PortalViewModel CreatePortalViewModel()
		{
			var navigationItems = new List<NavigationItem> { createWeekScheduleNavigationItem() };
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

			return new PortalViewModel
						{
							NavigationItems = navigationItems,
							CustomerName = _licenseActivator.CustomerName,
							ShowChangePassword = showChangePassword(),
							HasAsmPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)
						};
		}

		private bool showChangePassword()
		{
			var agent = _loggedOnUser.CurrentUser();
			return agent != null && agent.ApplicationAuthenticationInfo != null && !string.IsNullOrEmpty(agent.ApplicationAuthenticationInfo.ApplicationLogOnName);
		}

		private static NavigationItem createTeamScheduleNavigationItem()
		{
			return new NavigationItem
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
				PayAttention = unreadMessageCount != 0,
				UnreadMessageCount = unreadMessageCount
			};
		}

		private NavigationItem createPreferenceNavigationItem()
		{
			/*var preferenceOptions = PreferenceOptions();
			if (!_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb))
			{
				toolbarItems.AddRange(
					new ToolBarItemBase[]
						{
							new ToolBarSeparatorItem(),
							new ToolBarSplitButton
								{
									Title = Resources.Preference,
									Options = preferenceOptions
								}
						});
			}

			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb))
			{
				toolbarItems.Add(new ToolBarSeparatorItem());
				toolbarItems.Add(new ToolBarButtonItem { Title = Resources.Preference, ButtonType = "add-extended" });
			}
			toolbarItems.Add(new ToolBarButtonItem { Title = Resources.Delete, ButtonType = "delete" });


			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences) ||
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb))
			{
				toolbarItems.Add(new ToolBarSeparatorItem());
				toolbarItems.Add(new ToolBarButtonItem { Title = Resources.MustHave, ButtonType = "must-have", Icon = "heart" });
				toolbarItems.Add(new ToolBarButtonItem { Title = Resources.Delete, ButtonType = "must-have-delete", Icon = "heart-delete" });
				toolbarItems.Add(new ToolBarTextItem { Id = "must-have-numbers", Text = "0(0)" });
			}
			*/
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
						Controller = "StudentAvailability",
						Title = Resources.StudentAvailability
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
