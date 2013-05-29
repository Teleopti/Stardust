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
			var navigationItems = new List<SectionNavigationItem> { createWeekScheduleNavigationItem() };
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

		private static SectionNavigationItem createTeamScheduleNavigationItem()
		{
			return new TeamScheduleNavigationItem
						{
							Action = "Index",
							Controller = "TeamSchedule",
							Title = Resources.TeamSchedule,
							NavigationItems = new NavigationItem[0],
							ToolBarItems = new ToolBarItemBase[]
			       		               	{
			       		               		new ToolBarDatePicker
			       		               			{
			       		               				NextTitle = Resources.NextPeriod,
			       		               				PrevTitle = Resources.PreviousPeriod
			       		               			}
			       		               	}
						};
		}

		private SectionNavigationItem createRequestsNavigationItem()
		{
			var toolBarMenuItems = new List<ToolBarDropDownMenuItem>();

			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests))
			{
				toolBarMenuItems.Add(new ToolBarDropDownMenuItem
				{
					Title = Resources.NewTextRequest,
					MenyType = "addTextRequest"
				});
			}

			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb))
			{
				toolBarMenuItems.Add(new ToolBarDropDownMenuItem
				{
					Title = Resources.NewAbsenceRequest,
					MenyType = "addAbsenceRequest"
				});
			}

			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb))
			{
				toolBarMenuItems.Add(new ToolBarDropDownMenuItem
				{
					Title = Resources.NewShiftTradeRequest,
					MenyType = "addShiftTradeRequest"
				});
			}
			
			return new SectionNavigationItem
					{
						Action = "Index",
						Controller = "Requests",
						Title = Resources.Requests,
						NavigationItems = new NavigationItem[0],
						ToolBarItems = new List<ToolBarItemBase>
						               	{
						               		new ToolBarDropDown { Icon = "toolbar-addRequest", MenuItems = toolBarMenuItems}
						               	}
					};
		}

		private static SectionNavigationItem createMessageNavigationItem(int unreadMessageCount)
		{
			return new SectionNavigationItem
			{
				Action = "Index",
				Controller = "Message",
				Title = Resources.Messages,
				TitleCount = string.Format(Resources.MessagesParenthesis, unreadMessageCount),
				NavigationItems = new NavigationItem[0],
				ToolBarItems = new List<ToolBarItemBase>(),
				PayAttention = unreadMessageCount != 0,
				UnreadMessageCount = unreadMessageCount
			};
		}

		private PreferenceNavigationItem createPreferenceNavigationItem()
		{
			var preferenceOptions = PreferenceOptions();
			var toolbarItems = new List<ToolBarItemBase>
			                   	{
			                   		new ToolBarDatePicker
			                   			{
			                   				NextTitle = Resources.NextPeriod,
			                   				PrevTitle = Resources.PreviousPeriod
			                   			}
			                   	};
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


			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences))
			{
				toolbarItems.Add(new ToolBarSeparatorItem());
				toolbarItems.Add(new ToolBarButtonItem { Title = Resources.MustHave, ButtonType = "must-have", Icon = "heart" });
				toolbarItems.Add(new ToolBarButtonItem { Title = Resources.Delete, ButtonType = "must-have-delete", Icon = "heart-delete" });
				toolbarItems.Add(new ToolBarTextItem { Id = "must-have-numbers", Text = "0(0)" });
			}

			return new PreferenceNavigationItem
						{
							Action = "Index",
							Controller = "Preference",
							Title = Resources.Preference,
							NavigationItems = new NavigationItem[0],
							ToolBarItems = toolbarItems,
							PreferenceOptions = preferenceOptions,
							ActivityOptions = ActivityOptions()
						};
		}

		private IEnumerable<IOption> ActivityOptions()
		{
			return from a in _preferenceOptionsProvider.RetrieveActivityOptions().MakeSureNotNull()
				   select new Option
							  {
								  Value = a.Id.ToString(),
								  Text = a.Description.Name,
								  Color = a.DisplayColor.ToHtml()
							  };
		}

		private IEnumerable<IPreferenceOption> PreferenceOptions()
		{
			var shiftCategories =
				_preferenceOptionsProvider
					.RetrieveShiftCategoryOptions()
					.MakeSureNotNull()
					.Select(s => new PreferenceOption
										{
											Value = s.Id.ToString(),
											Text = s.Description.Name,
											Color = s.DisplayColor.ToHtml(),
											Extended = true
										})
					.ToArray();

			var dayOffs = _preferenceOptionsProvider
				.RetrieveDayOffOptions()
				.MakeSureNotNull()
				.Select(s => new PreferenceOption
									{
										Value = s.Id.ToString(),
										Text = s.Description.Name,
										Color = s.DisplayColor.ToHtml(),
										Extended = false
									})
				.ToArray();

			var absences = _preferenceOptionsProvider
				.RetrieveAbsenceOptions()
				.MakeSureNotNull()
				.Select(s => new PreferenceOption
									{
										Value = s.Id.ToString(),
										Text = s.Description.Name,
										Color = s.DisplayColor.ToHtml(),
										Extended = false
									})
				.ToArray();

			var options = new List<IPreferenceOption>();
			options.AddRange(shiftCategories);
			if (options.Count > 0 && dayOffs.Any())
				options.Add(new PreferenceOptionSplit());
			options.AddRange(dayOffs);
			if (options.Count > 0 && absences.Any())
				options.Add(new PreferenceOptionSplit());
			options.AddRange(absences);

			return options;
		}

		private static StudentAvailabilityNavigationItem createStudentAvailabilityNavigationItem()
		{
			return new StudentAvailabilityNavigationItem
					{
						Action = "Index",
						Controller = "Availability",
						Title = Resources.Availability,
						NavigationItems = new List<NavigationItem>(),
						ToolBarItems =
							new ToolBarItemBase[]
			       				{
			       					new ToolBarDatePicker
			       						{
			       							NextTitle = Resources.NextPeriod,
			       							PrevTitle = Resources.PreviousPeriod
			       						},
			       					new ToolBarSeparatorItem(),
			       					new ToolBarButtonItem {Title = Resources.Edit, ButtonType = "edit"},
			       					new ToolBarButtonItem {Title = Resources.Delete, ButtonType = "delete"},
			       					new ToolBarSeparatorItem()
			       				}
					};
		}

		private static SectionNavigationItem createWeekScheduleNavigationItem()
		{
			var toolBarItems = new List<ToolBarItemBase>
			                   	{
			                   		new ToolBarDatePicker
			                   			{
			                   				NextTitle = Resources.NextPeriod,
			                   				PrevTitle = Resources.PreviousPeriod
			                   			},
									new ToolBarButtonItem
										{
											Title = Resources.Today,
											ButtonType = "today"
										}
			                   	};
			return new SectionNavigationItem
					{
						Action = "Week",
						Controller = "Schedule",
						Title = Resources.Schedule,
						//Commented this until we add Month schedule view
						//NavigationItems =
						//    new[]
						//        {
						//            new NavigationItem
						//                {
						//                    Action = "Week",
						//                    Controller = "Schedule",
						//                    Title = Resources.Week
						//                }
						//        },
						ToolBarItems = toolBarItems
					};
		}
	}
}
