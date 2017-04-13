using System;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class DescriptionShortNameCellModel : GridTextBoxCellModel
    {

        private int _maxTextLength;

        public DescriptionShortNameCellModel(GridModel grid)
            : base(grid)
        {}

        protected DescriptionShortNameCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new DescriptionNameCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            if (text.Length > Domain.InterfaceLegacy.Domain.Description.MaxLengthOfShortName) return false;

            if (style.Tag != null)
            {
                if (int.TryParse(style.Tag.ToString(), out _maxTextLength))
                {
                    if (text.Length > _maxTextLength)
                        text = text.Substring(0, _maxTextLength);
                }
            }

            style.CellValue = new Description(((Description)style.CellValue).Name, text);
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            String ret = string.Empty;
            if (value is Description)
            {
                ret = ((Description)value).ShortName;
            }

            return ret;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {

            if (info == null)
                throw new ArgumentNullException("info");

            //Hmm...
            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }
}
