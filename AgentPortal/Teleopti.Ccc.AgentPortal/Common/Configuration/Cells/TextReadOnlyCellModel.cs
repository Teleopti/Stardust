using System;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Cells
{
    [Serializable]
    public class TextReadOnlyCellModel : GridStaticCellModel
    {
        protected TextReadOnlyCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TextReadOnlyCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return true;
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