using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory
{
	public class SettingsViewModelFactory : ISettingsViewModelFactory
	{
		private readonly IMappingEngine _mapper;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatPersisterAndProvider;

		public SettingsViewModelFactory(IMappingEngine mapper, ILoggedOnUser loggedOnUser, ISettingsPersisterAndProvider<NameFormatSettings> nameFormatPersisterAndProvider)
		{
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
			_nameFormatPersisterAndProvider = nameFormatPersisterAndProvider;
		}

		public SettingsViewModel CreateViewModel()
		{
			// as the settings view model requires values from the person, create the view model
			// using an automapper mapping.		
			var settingsViewModel = _mapper.Map<IPerson, SettingsViewModel>(_loggedOnUser.CurrentUser());
			var persistedNameFormatSettings = _nameFormatPersisterAndProvider.Get();

			settingsViewModel.NameFormats =	from Enum e in Enum.GetValues(typeof(AgentNameFormat))
							 select new NameFormatViewModel { id = Convert.ToInt32(e), text = e.GetDisplayName() };

			settingsViewModel.ChosenNameFormat = persistedNameFormatSettings != null ?
				settingsViewModel.NameFormats.FirstOrDefault(i => i.id == persistedNameFormatSettings.NameFormatId) :
				settingsViewModel.NameFormats.FirstOrDefault();
			
			return settingsViewModel;
		}
	}
}