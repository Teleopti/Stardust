using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Parameters
{
	class ParameterComboPeriodType : ParameterCombo
	{
		private const int weekPeriod = 5;
		protected override void BindData()
		{
			_dropDown.DataSource = MyData.Tables[0];
			_dropDown.DataBind();

			foreach (ListItem myItem in _dropDown.Items)
			{
				if (myItem.Value == DefaultValue)
					myItem.Selected = true;
				if (myItem.Value == weekPeriod.ToString(CultureInfo.InvariantCulture) && hideWeekPeriodType())
					myItem.Enabled = false;
			}
		}

		private static bool hideWeekPeriodType()
		{
			return InvalidCultures.Contains(Thread.CurrentThread.CurrentCulture.Name);
		}

		private static List<string> InvalidCultures
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
