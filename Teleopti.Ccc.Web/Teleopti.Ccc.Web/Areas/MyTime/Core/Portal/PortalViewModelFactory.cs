using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
	public class PortalViewModelFactory : IPortalViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPreferenceOptionsProvider _preferenceOptionsProvider;
		private readonly ILicenseActivator _licenseActivator;
		private readonly IPrincipalProvider _principalProvider;

		public PortalViewModelFactory(IPermissionProvider permissionProvider, 
												IPreferenceOptionsProvider preferenceOptionsProvider, 
												ILicenseActivator licenseActivator,
												IPrincipalProvider principalProvider)
		{
			_permissionProvider = permissionProvider;
			_preferenceOptionsProvider = preferenceOptionsProvider;
			_licenseActivator = licenseActivator;
			_principalProvider = principalProvider;
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
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences))
			{
				navigationItems.Add(createPreferenceNavigationItem());
			}
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests))
			{
				navigationItems.Add(createRequestsNavigationItem());
			}
			var showPw = showChangePassword();
			return new PortalViewModel
			       	{
			       		NavigationItems = navigationItems,
							CustomerName = _licenseActivator.CustomerName,
							ShowChangePassword = showPw
			       	};
		}

		private bool showChangePassword()
		{
			var principal = _principalProvider.Current();
			return principal != null && ((TeleoptiIdentity)principal.Identity).TeleoptiAuthenticationType == AuthenticationTypeOption.Application;
		}

		private SectionNavigationItem createTeamScheduleNavigationItem()
		{
			return new SectionNavigationItem
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
			       		               			},
			       		               		new ToolBarSeparatorItem(),
			       		               		new ToolBarSelectBox
			       		               			{
			       		               				Type = "TeamPicker",
			       		               				Options = new SelectBoxOption[] {}
			       		               			},
			       		               	}
			       	};
		}

		private SectionNavigationItem createRequestsNavigationItem()
		{
			return new SectionNavigationItem
					{
						Action = "Index",
						Controller = "Requests",
						Title = Resources.Requests,
						NavigationItems = new NavigationItem[0],
						ToolBarItems = new ToolBarItemBase[]
						{
							new ToolBarButtonItem
							{
								Title = Resources.NewTextRequest,
								ButtonType = "addTextRequest"
							}
						}
					};
		}

		private SectionNavigationItem createPreferenceNavigationItem()
		{
			return new SectionNavigationItem
					{
						Action = "Index",
						Controller = "Preference",
						Title = Resources.Preference,
						NavigationItems = new NavigationItem[0],
						ToolBarItems = new ToolBarItemBase[]
			       		               	{
			       		               		new ToolBarDatePicker
			       		               			{
			       		               				NextTitle = Resources.NextPeriod,
			       		               				PrevTitle = Resources.PreviousPeriod
			       		               			},
											new ToolBarSeparatorItem(),
											new ToolBarSplitButton 
												{
													Title = Resources.Preference, 
													Options = PreferenceOptions()
												},
											new ToolBarSeparatorItem(),
					       					new ToolBarButtonItem {Title = Resources.Delete, ButtonType = "delete"}
			       		               	}
					};
		}

		private IEnumerable<ISplitButtonOption> PreferenceOptions()
		{
			var shiftCategories = (from s in _preferenceOptionsProvider.RetrieveShiftCategoryOptions().MakeSureNotNull()
			                       select new SplitButtonOption
			                              	{
			                              		Value = s.Id.ToString(),
			                              		Text = s.Description.Name,
			                              		Style = new StyleClassViewModel
			                              		        	{
			                              		        		Name = s.DisplayColor.ToStyleClass(),
			                              		        		ColorHex = s.DisplayColor.ToHtml(),
			                              		        	}
			                              	})
				.ToArray();
			var dayOffs = (from s in _preferenceOptionsProvider.RetrieveDayOffOptions().MakeSureNotNull()
			               select new SplitButtonOption
			                      	{
			                      		Value = s.Id.ToString(),
			                      		Text = s.Description.Name,
			                      		Style = new StyleClassViewModel
			                      		        	{
			                      		        		Name = s.DisplayColor.ToStyleClass(),
			                      		        		ColorHex = s.DisplayColor.ToHtml(),
			                      		        	}
			                      	})
				.ToArray();
			var absences = (from s in _preferenceOptionsProvider.RetrieveAbsenceOptions().MakeSureNotNull()
			                select new SplitButtonOption
			                       	{
			                       		Value = s.Id.ToString(),
			                       		Text = s.Description.Name,
			                       		Style = new StyleClassViewModel
			                       		        	{
			                       		        		Name = s.DisplayColor.ToStyleClass(),
			                       		        		ColorHex = s.DisplayColor.ToHtml(),
			                       		        	}
			                       	})
				.ToArray();

			var options = new List<ISplitButtonOption>();
			options.AddRange(shiftCategories);
			if (options.Count > 0 && dayOffs.Any())
				options.Add(new SplitButtonSplitter());
			options.AddRange(dayOffs);
			if (options.Count > 0 && absences.Any())
				options.Add(new SplitButtonSplitter());
			options.AddRange(absences);

			return options;
		}

		private static SectionNavigationItem createStudentAvailabilityNavigationItem()
		{
			return new SectionNavigationItem
					{
						Action = "Index",
						Controller = "StudentAvailability",
						Title = Resources.StudentAvailability,
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

		private SectionNavigationItem createWeekScheduleNavigationItem()
		{
			var toolBarItems = new List<ToolBarItemBase>
			                   	{
			                   		new ToolBarDatePicker
			                   			{
			                   				NextTitle = Resources.NextPeriod,
			                   				PrevTitle = Resources.PreviousPeriod
			                   			}
			                   	};
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests))
			{
				toolBarItems.Add(new ToolBarSeparatorItem());
				toolBarItems.Add(new ToolBarButtonItem
				                 	{
				                 		Title = Resources.NewTextRequest,
				                 		ButtonType = "addTextRequest"
				                 	});
			}
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
