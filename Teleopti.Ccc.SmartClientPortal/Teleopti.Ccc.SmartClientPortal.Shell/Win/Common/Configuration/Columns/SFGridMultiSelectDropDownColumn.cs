using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
	public class SFGridMultiSelectDropDownColumn<T, TItem> : SFGridColumnBase<T>
	{
		private readonly IList<TItem> _comboItems;
		private readonly string _displayMember;
		private readonly Type _baseClass;

		private readonly DisabledObjectSpecification<T> _disabledObjectSpecification =
			new DisabledObjectSpecification<T>("IsTrackerDisabled");

		public SFGridMultiSelectDropDownColumn(string bindingProperty, string headerText, IList<TItem> comboItems, string displayMember, Type baseClass)
			: this(bindingProperty, headerText, null, comboItems, displayMember, baseClass)
		{
		}

		public SFGridMultiSelectDropDownColumn(string bindingProperty, string headerText, string groupHeaderText, IList<TItem> comboItems, string displayMember, Type baseClass)
			: base(bindingProperty, headerText)
		{
			GroupHeaderText = groupHeaderText;
			_comboItems = comboItems;
			_displayMember = displayMember;
			_baseClass = baseClass;
			UseDisablePropertyCheck = false;
		}

		public override int PreferredWidth => 200;

		public event EventHandler<GridQueryCellInfoEventArgs> QueryComboItems;

		public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
		{
			e.Style.CellType = "MultiSelectCellModel";
			OnQueryComboItems(e);
			e.Style.DisplayMember = _displayMember;
			e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
			e.Style.DropDownStyle = GridDropDownStyle.AutoComplete;
			if (!UseDisablePropertyCheck) return;
			if (!_disabledObjectSpecification.IsSatisfiedBy(currentItem)) return;
			e.Style.Enabled = false;
			e.Style.BackColor = ColorHelper.DisabledCellColor;
		}

		protected void OnQueryComboItems(GridQueryCellInfoEventArgs e)
		{
			var handler = QueryComboItems;
			if (handler != null)
			{
				handler(this, e);
			}
			else
			{
				e.Style.DataSource = _comboItems;
			}
		}

		public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
		{
			if (!e.Style.Enabled) return;
			if (e.Style.CellValue != null)
			{
				var tItemType = typeof(TItem);
				var cellValueType = e.Style.CellValue.GetType();
				if (Array.Exists(cellValueType.GetNestedTypes(), t => t.Equals(tItemType)) ||
					Array.Exists(cellValueType.GetInterfaces(), t => t.Equals(tItemType)) ||
					cellValueType.IsAssignableFrom(tItemType))
				{
					PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
				}
				else
				{
					SetValueByDisplayMemberValue(currentItem, e.Style.CellValue.ToString().Split(','));
				}
			}
			else
			{
				SetValueByDisplayMemberValue(currentItem, string.Empty);
			}
		}

		private void SetValueByDisplayMemberValue(T currentItem, params string[] stringValues)
		{
			var listValidValues = new List<string>(stringValues.Length);

			foreach (var stringValue in stringValues)
			{
				if (tryGetItemByDisplayMember(stringValue, out _))
				{
					listValidValues.Add(stringValue);
				}
			}

			if (listValidValues.Any())
				PropertyReflectorHelper.SetValue(currentItem, BindingProperty, string.Join(",", listValidValues));
		}

		private bool tryGetItemByDisplayMember(string displayMember, out TItem comboItem)
		{
			foreach (var comboItem1 in from comboItem1 in _comboItems
				let itemName = PropertyReflectorHelper.GetValue(comboItem1, _displayMember).ToString()
				where itemName == displayMember
				select comboItem1)
			{
				comboItem = comboItem1;
				return true;
			}

			comboItem = _comboItems[0];
			return false;
		}

		protected override void ApplyIgnoreCell(GridStyleInfo style)
		{
			base.ApplyIgnoreCell(style);
			if (style.CellType != "IgnoreCell" && _baseClass != null)
			{
				style.CellValueType = _baseClass;
			}
		}

		internal bool UseDisablePropertyCheck { get; set; }
	}
}