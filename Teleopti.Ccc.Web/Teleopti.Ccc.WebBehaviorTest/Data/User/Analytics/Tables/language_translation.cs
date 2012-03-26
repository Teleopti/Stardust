using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables
{
	public static class language_translation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.language_translation");
			table.Columns.Add("Culture");
			table.Columns.Add("language_id", typeof(int));
			table.Columns.Add("ResourceKey");
			table.Columns.Add("term_language");
			table.Columns.Add("term_english");
			return table;
		}

		public static void AddTranslation(
			this DataTable dataTable,
			string Culture,
			int language_id,
			string ResourceKey,
			string term_language,
			string term_english)
		{
			var row = dataTable.NewRow();
			row["Culture"] = Culture;
			row["language_id"] = language_id;
			row["ResourceKey"] = ResourceKey;
			row["term_language"] = term_language;
			row["term_english"] = term_english;
			dataTable.Rows.Add(row);
		}
	}
}