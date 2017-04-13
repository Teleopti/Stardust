using System;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
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
			var renderer = new GridComboBoxCellRenderer(control, this);
			((GridComboBoxListBoxPart) renderer.ListBoxPart).DropDownRows = 25;
			return renderer;
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

				var culture = WinCode.PeopleAdmin.Culture.GetLanguageInfoByDisplayName(text);
				if (culture.Id != 0)
				{
					style.CellValue = GetInternalListBox().GetItemValue(culture);
					return true;
				}

				if (exclusive)
					return false;
			}
			return base.ApplyFormattedText(style, text, textInfo);
		}

		public override bool ApplyText(GridStyleInfo style, string text)
		{
			return ApplyFormattedText(style, text, -1);
		}
	}
}