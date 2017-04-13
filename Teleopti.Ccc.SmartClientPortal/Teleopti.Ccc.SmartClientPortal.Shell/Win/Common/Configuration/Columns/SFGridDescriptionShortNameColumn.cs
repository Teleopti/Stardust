using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    public class SFGridDescriptionShortNameColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _textWidth;
        private readonly bool _isReadOnly;
        private readonly int _maxCharacterLength = Description.MaxLengthOfShortName;

        public override int PreferredWidth
        {
            get { return (_textWidth > 0) ? _textWidth : 150; }
        }

        public SFGridDescriptionShortNameColumn(string bindingProperty, string headerText, int textWidth, bool isReadOnly, int maxCharacterLength)
            : base(bindingProperty, headerText)
        {
            _textWidth = textWidth;
            _isReadOnly = isReadOnly;
            _maxCharacterLength = maxCharacterLength;
        }

        private Description GetDescription(T currentItem, string text)
        {
            Description description;
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);

            if (value is Description)
                description = (Description)value;
            else
                description = new Description(value.ToString(), text);
            return description;
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "DescriptionShortNameCellModel";
            e.Style.Tag = _maxCharacterLength;
            e.Style.MaxLength = _maxCharacterLength;
            e.Style.CellValue = GetDescription(currentItem, string.Empty);
            if (_isReadOnly) e.Style.ReadOnly = true;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            if (_isReadOnly)
                return;

            var description = (Description)e.Style.CellValue;
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, description);
        }
    }
}
