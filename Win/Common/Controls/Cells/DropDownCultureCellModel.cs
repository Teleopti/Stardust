using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
	public class DropDownCultureCellModel : GridComboBoxCellModel
	{

		public DropDownCultureCellModel(GridModel grid)
			: base(grid)
		{
		}

		protected DropDownCultureCellModel(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			return (GridCellRendererBase)new GridComboBoxCellRenderer(control, (GridCellModelBase)this);
		}

		public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
		{
			if (text.Length > 0 && (style.ChoiceList == null || style.ChoiceList.Count == 0) && (style.DataSource != null && style.DisplayMember != style.ValueMember))
			{
				bool exclusive;
				FillWithChoices(GetInternalListBox(), style, out exclusive);
				GetInternalListBox().DisplayMember = style.DisplayMember;
				GetInternalListBox().ValueMember = style.ValueMember;
				if (string.IsNullOrEmpty(style.DisplayMember) && !string.IsNullOrEmpty(style.ValueMember))
					GetInternalListBox().DisplayMember = style.ValueMember;
				else if (!string.IsNullOrEmpty(style.DisplayMember) && string.IsNullOrEmpty(style.ValueMember) && style.CellValueType == typeof(string))
					GetInternalListBox().ValueMember = style.DisplayMember;
				GetInternalListBox().DataSource = style.DataSource;
				int stringExact = GetInternalListBox().FindStringExact(text);
				if (stringExact != -1)
				{
					style.CellValue = GetInternalListBox().GetItemValue(stringExact);
					return true;
				}

				// so far it is the same code as in the base, basically to support the original behaviour

				// new staff
				var culture = WinCode.PeopleAdmin.Culture.GetLanguageInfoByDisplayName(text);
				if (culture.Id != 0)
				{
					style.CellValue = GetInternalListBox().GetItemValue(culture);
					return true;
				}

				// from here it is again the same code as in the base
				if (exclusive)
					return false;
			}
			return base.ApplyFormattedText(style, text, textInfo);
		}

		//private object TryFindListBoxItemByCultureNameAndDisplayText(string text)
		//{
		//	var result = WinCode.PeopleAdmin.Culture.GetLanguageInfoByDisplayName(text);


		//	CultureInfo[] infos = CultureInfo.GetCultures(CultureTypes.AllCultures);
		//	var culture = infos.FirstOrDefault(i => i.DisplayName == text);

		//	if (culture != null)
		//		//return GetInternalListBox().FindValue(culture);
		//		return FindByCulture(culture);
			
		//	culture = infos.FirstOrDefault(i => i.Name == text);

		//	if (culture != null)
		//		return GetInternalListBox().FindValue(culture);

		//	return -1;

		//}

		private int FindByCulture(CultureInfo culture)
		{
			foreach (var item in GetInternalListBox().Items)
			{
				var what = item.ToString();
			}
			return 1;
		}

		public override bool ApplyText(GridStyleInfo style, string text)
		{
			return ApplyFormattedText(style, text, -1);
		}
	}
}
