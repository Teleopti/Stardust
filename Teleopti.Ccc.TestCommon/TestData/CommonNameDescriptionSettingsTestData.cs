using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public class CommonNameDescriptionSettingsTestData
	{
		private static readonly Dictionary<string, string> testData = new Dictionary<string, string>
		{
			{ "{EmployeeNumber} - {FirstName} {LastName}", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([employment_number], '') + N' - ' + ISNULL([first_name], '') + N' ' + ISNULL([last_name], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit" },
			{ "{EmployeeNumber}", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([employment_number], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit" },
			{ "{FirstName} {LastName} #123# {EmployeeNumber}", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([first_name], '') + N' ' + ISNULL([last_name], '') + N' #123# ' + ISNULL([employment_number], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"},
			{ "123{::][}{__ {FirstName} {LastName} #{Firstname}# ", "UPDATE mart.dim_person SET person_name = SUBSTRING(N'123{::][}{__ ' + ISNULL([first_name], '') + N' ' + ISNULL([last_name], '') + N' #{Firstname}# ', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"},
			{ "No names in reports", "UPDATE mart.dim_person SET person_name = SUBSTRING(N'No names in reports', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"},
			{ "{FirstName} '; Drop Database; ", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([first_name], '') + N' ''; Drop Database; ', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"},
			{ "{FirstName}{FirstName}", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([first_name], '') + ISNULL([first_name], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"},
			{ "", "UPDATE mart.dim_person SET person_name = SUBSTRING(N'', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"},
			{ "{FirstName}лаудем  伴年聞早 無巣目個 지에 그들을", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([first_name], '') + N'лаудем  伴年聞早 無巣目個 지에 그들을', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"},
			{
				// bug #47514 - 1
				"{LastName}, {FirstName} '{EmployeeNumber}'",
				"UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([last_name], '') + N', ' + ISNULL([first_name], '') + N' ''' + ISNULL([employment_number], '') + N'''', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"
			},
			{
				// bug #47514 - 2
				"'{LastName}', {FirstName} {EmployeeNumber}",
				"UPDATE mart.dim_person SET person_name = SUBSTRING(N'''' + ISNULL([last_name], '') + N''', ' + ISNULL([first_name], '') + N' ' + ISNULL([employment_number], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"
			},
			{
				// bug #47514 - 3
				"{LastName}, '{FirstName}' {EmployeeNumber}",
				"UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([last_name], '') + N', ''' + ISNULL([first_name], '') + N''' ' + ISNULL([employment_number], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"
			},
			{
				// bug #47514 - 4
				"'{LastName}', '{FirstName}' '{EmployeeNumber}'",
				"UPDATE mart.dim_person SET person_name = SUBSTRING(N'''' + ISNULL([last_name], '') + N''', ''' + ISNULL([first_name], '') + N''' ''' + ISNULL([employment_number], '') + N'''', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit"
			}
		};

		public static IEnumerable TestCasesUnit
		{
			get { return testData.Select(x => new TestCaseData(x.Key).Returns(x.Value)); }
		}

		public static IEnumerable TestCasesIntegration
		{
			get { return testData.Select(x => new TestCaseData(x.Key)); }
		}
	}
}
