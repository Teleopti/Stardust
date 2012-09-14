﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
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
		private readonly IIdentityProvider _identityProvider;

		public PortalViewModelFactory(
			IPermissionProvider permissionProvider,
												IPreferenceOptionsProvider preferenceOptionsProvider,
												ILicenseActivator licenseActivator,
			IIdentityProvider identityProvider)
		{
			_permissionProvider = permissionProvider;
			_preferenceOptionsProvider = preferenceOptionsProvider;
			_licenseActivator = licenseActivator;
			_identityProvider = identityProvider;
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
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests))
			{
				navigationItems.Add(createRequestsNavigationItem());
			}
			return new PortalViewModel
			       	{
			       		NavigationItems = navigationItems,
			       		CustomerName = _licenseActivator.CustomerName,
			       		ShowChangePassword = showChangePassword(),
							ShowAsm =  _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OpenAsm)
			       	};
		}

		private bool showChangePassword()
		{
			var identity = _identityProvider.Current();
			if (identity == null)
				return false;
			return identity.TeleoptiAuthenticationType == AuthenticationTypeOption.Application;
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
			       		               				Options = new Option[] {}
			       		               			}
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
								Title = Resources.NewRequest,
								ButtonType = "addTextRequest"
							}
						}
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

			toolbarItems.Add(new ToolBarSeparatorItem());
			if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb))
			{
				toolbarItems.Add(new ToolBarButtonItem { Title = Resources.AddExtendedPreference, ButtonType = "add-extended" });
			}
			toolbarItems.Add(new ToolBarButtonItem {Title = Resources.Delete, ButtonType = "delete"});
			
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
