using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class WeekdayTranslations : IAnalyticsDataSetup
	{
		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var englishCulture = CultureInfo.GetCultureInfo("en-US");

			using (var table = language_translation.CreateTable())
			{
				table.AddTranslation(userCulture.TwoLetterISOLanguageName, userCulture.LCID, "ResDayOfWeekMonday", Resources.ResDayOfWeekMonday, Resources.ResourceManager.GetString("ResDayOfWeekMonday", englishCulture));
				table.AddTranslation(userCulture.TwoLetterISOLanguageName, userCulture.LCID, "ResDayOfWeekTuesday", Resources.ResDayOfWeekTuesday, Resources.ResourceManager.GetString("ResDayOfWeekTuesday", englishCulture));
				table.AddTranslation(userCulture.TwoLetterISOLanguageName, userCulture.LCID, "ResDayOfWeekWednesday", Resources.ResDayOfWeekWednesday, Resources.ResourceManager.GetString("ResDayOfWeekWednesday", englishCulture));
				table.AddTranslation(userCulture.TwoLetterISOLanguageName, userCulture.LCID, "ResDayOfWeekThursday", Resources.ResDayOfWeekThursday, Resources.ResourceManager.GetString("ResDayOfWeekThursday", englishCulture));
				table.AddTranslation(userCulture.TwoLetterISOLanguageName, userCulture.LCID, "ResDayOfWeekFriday", Resources.ResDayOfWeekFriday, Resources.ResourceManager.GetString("ResDayOfWeekFriday", englishCulture));
				table.AddTranslation(userCulture.TwoLetterISOLanguageName, userCulture.LCID, "ResDayOfWeekSaturday", Resources.ResDayOfWeekSaturday, Resources.ResourceManager.GetString("ResDayOfWeekSaturday", englishCulture));
				table.AddTranslation(userCulture.TwoLetterISOLanguageName, userCulture.LCID, "ResDayOfWeekSunday", Resources.ResDayOfWeekSunday, Resources.ResourceManager.GetString("ResDayOfWeekSunday", englishCulture));

				Bulk.Insert(connection, table);

			}
		}
	}
}