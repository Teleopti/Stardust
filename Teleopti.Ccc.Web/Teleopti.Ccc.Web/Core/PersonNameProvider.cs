using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;

namespace Teleopti.Ccc.Web.Core
{
	public interface IPersonNameProvider
	{
		string BuildNameFromSetting(Name name);
		string BuildNameFromSetting(Name name, NameFormatSettings setting);
		string BuildNameFromSetting(string firstName, string lastName, NameFormatSettings setting);
		string BuildNameFromSetting(string firstName, string lastName);
	}

	public class PersonNameProvider : IPersonNameProvider
	{
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;

		public PersonNameProvider(ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings)
		{
			_nameFormatSettings = nameFormatSettings;
		}

		public string BuildNameFromSetting(Name name)
		{
			return BuildNameFromSetting(name.FirstName, name.LastName);
		}

		public string BuildNameFromSetting(Name name, NameFormatSettings setting)
		{
			return BuildNameFromSetting(name.FirstName, name.LastName, setting);
		}

		public string BuildNameFromSetting(string firstName, string lastName, NameFormatSettings setting)
		{
			return buildNameBaseOnSetting(firstName, lastName, setting);
		}

		public string BuildNameFromSetting(string firstName, string lastName)
		{
			var persistedNameFormatSettings = _nameFormatSettings.Get();
			var nameStringBaseOnSetting = buildNameBaseOnSetting(firstName, lastName, persistedNameFormatSettings);
			return nameStringBaseOnSetting;
		}

		private string buildNameBaseOnSetting(string firstName, string lastName, NameFormatSettings setting)
		{
			if (setting != null)
			{
				if (setting.NameFormatId == 0)
				{
					return firstName + " " + lastName;
				}
				if (setting.NameFormatId == 1)
				{
					return lastName + " " + firstName;
				}	
			}
			string defaultNameString = firstName + " " + lastName;
			return defaultNameString;
		}
	}
}