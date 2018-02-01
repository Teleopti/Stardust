using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AdvancedAgentsFilter
	{
		public ICollection<IPerson> Filter(string filterText, IList<IPerson> persons, IEnumerable<LogonInfoModel> logonInfoModels)
		{
			var cultureInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			var lowerSearchText = filterText.ToLower(cultureInfo);
			var guids = (from logonInfoModel in logonInfoModels where logonContains(logonInfoModel, lowerSearchText, cultureInfo) select logonInfoModel.PersonId).ToList();
			var result = filterMultiple(cultureInfo, lowerSearchText, persons, guids);

			return result;
		}

		private static ICollection<IPerson> filterMultiple(CultureInfo cultureInfo, string lowerSearchText, IEnumerable<IPerson> persons, ICollection<Guid> guids)
		{
			ICollection<IPerson> personQuery =
			(from
					person in persons
				where
					person.Name.ToString(NameOrderOption.LastNameFirstName).ToLower(cultureInfo).Contains(lowerSearchText) ||
					person.Name.ToString(NameOrderOption.LastNameFirstName).ToLower(cultureInfo).Replace(",", "").Contains(lowerSearchText) ||
					person.Name.ToString(NameOrderOption.FirstNameLastName).ToLower(cultureInfo).Contains(lowerSearchText) ||
					person.EmploymentNumber.ToLower(cultureInfo).Contains(lowerSearchText) ||
					person.Email.ToLower(cultureInfo).Contains(lowerSearchText) ||
					guids.Contains(person.Id.GetValueOrDefault())

				select person).ToList();

			return personQuery;
		}

		private static bool logonContains(LogonInfoModel model, string lowerSearchText, CultureInfo cultureInfo)
		{
			var logon = "";
			if (!string.IsNullOrEmpty(model.Identity))
				logon = model.Identity.ToLower(cultureInfo);
			if (!string.IsNullOrEmpty(model.LogonName))
				logon += model.LogonName.ToLower(cultureInfo);
			return logon.Contains(lowerSearchText);
		}
	}
}
