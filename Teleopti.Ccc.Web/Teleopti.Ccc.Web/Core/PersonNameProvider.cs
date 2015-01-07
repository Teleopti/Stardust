using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core
{
	public interface IPersonNameProvider
	{
		string BuildNameFromSetting(Name name);
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

		public string BuildNameFromSetting(string firstName, string lastName)
		{
			string firstLast = firstName + " " + lastName;
			
			var persistedNameFormatSettings = _nameFormatSettings.Get();
			if (persistedNameFormatSettings != null)
			{
				if (persistedNameFormatSettings.NameFormatId == 0)
				{
					return firstLast;
				}
				if (persistedNameFormatSettings.NameFormatId == 1)
				{
					return lastName + " " + firstName;
				}
			}

			return firstLast;
		}
	}
}