﻿using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridDescriptionNameColumn<T> : SFGridColumnBase<T>
    {
        public SFGridDescriptionNameColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { }

        private readonly int _textWidth;
        private readonly bool _isReadOnly;

        public SFGridDescriptionNameColumn(string bindingProperty, string headerText, int textWidth, bool isReadOnly)
            : base(bindingProperty, headerText)
        {
            _textWidth = textWidth;
            _isReadOnly = isReadOnly;
        }
        public override int PreferredWidth
        {
            get { return (_textWidth > 0) ? _textWidth : 150; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "DescriptionNameCell";
            e.Style.CellValue = GetDescription(currentItem);
            if (_isReadOnly) e.Style.ReadOnly = true;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (_isReadOnly)
                return;

            if (e.Style.CellValue.GetType() != typeof(Description))
                return;

            var description = (Description)e.Style.CellValue;
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, new Description(description.Name, description.ShortName));
        }

        private Description GetDescription(T currentItem)
        {
            Description description;
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            if (value.GetType() == typeof(Description))
                description = (Description)value;
            else
                description = new Description(value.ToString(), System.String.Empty);
            return description;
        }
    }
}