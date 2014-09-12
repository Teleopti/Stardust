using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory
{
	public class SettingsViewModelFactory : ISettingsViewModelFactory
	{
		public SettingsViewModelFactory()
		{
		}

		public SettingsViewModel CreateViewModel(IMappingEngine mapper, ILoggedOnUser loggedOnUser)
		{
			// as the settings view model requires values from the person, create the view model
			// using an automapper mapping.		
			var settingsViewModel = mapper.Map<IPerson, SettingsViewModel>(loggedOnUser.CurrentUser());

			var nameFormates = new List<NameFormatViewModel>();
			nameFormates.Add(new NameFormatViewModel() { text = "[" + Resources.FirstName + "] [" + Resources.LastName + "]", id = 0 });
			nameFormates.Add(new NameFormatViewModel() { text = "[" + Resources.LastName + "] [" + Resources.FirstName + "]", id = 1 });

			settingsViewModel.NameFormats = nameFormates;
			settingsViewModel.ChosenNameFormat = nameFormates.First();
			return settingsViewModel;
		}
	}
}