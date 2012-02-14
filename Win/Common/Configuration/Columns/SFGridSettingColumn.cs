using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSettings;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    /// <summary>
    /// Grid column for text values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 10/20/2008
    /// </remarks>
    public class SFGridSettingColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _maxLength;

        public SFGridSettingColumn(string bindingProperty, int maxLength, string headerText)
            : base(bindingProperty, headerText)
        {
            _maxLength = maxLength;
        }

        public override int PreferredWidth
        {
            get { return _maxLength; }
        }

        public override void GetCellValue(Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e, System.Collections.ObjectModel.ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellValue = ((ISetting) currentItem).Value;
        }

        public override void SaveCellValue(Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs e, System.Collections.ObjectModel.ReadOnlyCollection<T> dataItems, T currentItem)
        {
            ISetting systemSetting = (ISetting) currentItem;
            systemSetting.Value = e.Style.CellValue;
        }
    }
}
