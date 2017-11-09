using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
	[Serializable]
	public class CommonNameDescriptionSetting : SettingValue, ICommonNameDescriptionSetting
	{
		public const string Key = "CommonNameDescription";

		public const string FirstName = "{FirstName}";
		public const string LastName = "{LastName}";
		public const string EmployeeNumber = "{EmployeeNumber}";
		private const string defaultCommonNameDescription = "{FirstName} {LastName}";

		public CommonNameDescriptionSetting()
		{
			AliasFormat = defaultCommonNameDescription;
		}

		public CommonNameDescriptionSetting(string aliasFormat)
		{
			AliasFormat = aliasFormat;
		}

		public virtual string AliasFormat { get; set; }

		public virtual string BuildFor(IPerson person)
		{
			var builded = AliasFormat;
			builded = builded.Replace(FirstName, person.Name.FirstName);
			builded = builded.Replace(LastName, person.Name.LastName);
			builded = builded.Replace(EmployeeNumber, person.EmploymentNumber);
			return builded;
		}

		public string BuildFor(ILightPerson lightPerson)
		{
			var builded = AliasFormat;
			builded = builded.Replace(FirstName, lightPerson.FirstName);
			builded = builded.Replace(LastName, lightPerson.LastName);
			builded = builded.Replace(EmployeeNumber, lightPerson.EmploymentNumber);
			return builded;
		}

		public string BuildFor(string firstName, string lastName, string employmentNumber)
		{
			return AliasFormat
				.Replace(FirstName, firstName)
				.Replace(LastName, lastName)
				.Replace(EmployeeNumber, employmentNumber);
		}

		public IEnumerable<string> BuildNamesFor(string firstName, string lastName, string employmentNumber)
		{
			return BuildFor(firstName, lastName, employmentNumber)
					.Replace("{", "").Replace("}", "")
					.Split(null)
					.Where(x => new[] {firstName, lastName, employmentNumber}.Contains(x))
				;
		}

		public string BuildSqlUpdateForAnalytics()
		{
			var sqlConcat = $"N'{AliasFormat.Replace("'", "''")}'"; // Replace to prevent sql injections
			sqlConcat = sqlConcat.Replace(FirstName, "' + ISNULL([first_name], '') + N'");
			sqlConcat = sqlConcat.Replace(LastName, "' + ISNULL([last_name], '') + N'");
			sqlConcat = sqlConcat.Replace(EmployeeNumber, "' + ISNULL([employment_number], '') + N'");
			sqlConcat = sqlConcat.Replace("N'' + ", "");
			sqlConcat = sqlConcat.Replace(" + N''", "");
			return $"UPDATE mart.dim_person SET person_name = SUBSTRING({sqlConcat}, 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit";
		}
	}
}