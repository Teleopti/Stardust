using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core
{
	public interface IPersonNameProvider
	{
		string BuildNameFromSetting(IPerson person);
	}

	public class PersonNameProvider : IPersonNameProvider
	{
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;

		public PersonNameProvider(ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings)
		{
			_nameFormatSettings = nameFormatSettings;
		}

		public string BuildNameFromSetting(IPerson person)
		{
			string name = person.Name.FirstName + " " + person.Name.LastName;
			var persistedNameFormatSettings = _nameFormatSettings.Get();
			if (persistedNameFormatSettings != null)
			{
				if (persistedNameFormatSettings.NameFormatId == 0)
				{
					name = person.Name.FirstName + " " + person.Name.LastName;
				}
				else if (persistedNameFormatSettings.NameFormatId == 1)
				{
					name = person.Name.LastName + " " + person.Name.FirstName;
				}
			}

			return name;
		}
	}
}