using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	class ParameterComboPeriodType : ParameterCombo
	{
		private const int weekPeriod = 5;

		public ParameterComboPeriodType(UserReportParams userReportParams)
			: base(userReportParams)
		{
		}

		protected override void BindData()
		{
			DropDown.DataSource = MyData.Tables[0];
			DropDown.DataBind();

			foreach (ListItem myItem in DropDown.Items)
			{
				if (myItem.Value == DefaultValue)
					myItem.Selected = true;
				if (myItem.Value == weekPeriod.ToString(CultureInfo.InvariantCulture) && hideWeekPeriodType())
					myItem.Enabled = false;
			}
		}

		private static bool hideWeekPeriodType()
		{
			return invalidCultures.Contains(Thread.CurrentThread.CurrentCulture.Name);
		}

		private static List<string> invalidCultures
		{
			get
			{
				return new List<string>
				{
					"ar",
					"ar-AE",
					"ar-BH",
					"ar-DZ",
					"ar-EG",
					"ar-IQ",
					"ar-JO",
					"ar-KW",
					"ar-LB",
					"ar-LY",
					"ar-MA",
					"ar-OM",
					"ar-QA",
					"ar-SA",
					"ar-SY",
					"ar-TN",
					"ar-YE"
				};
			}
		}
	}

}
