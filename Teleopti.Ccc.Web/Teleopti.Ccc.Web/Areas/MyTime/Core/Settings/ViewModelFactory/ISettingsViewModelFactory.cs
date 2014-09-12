using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory
{
	public interface ISettingsViewModelFactory
	{
		SettingsViewModel CreateViewModel(IMappingEngine mapper, ILoggedOnUser loggedOnUser);
	}
}