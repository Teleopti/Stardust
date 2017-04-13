﻿using System;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class DescriptionNameCellModel : GridTextBoxCellModel
    {
        public DescriptionNameCellModel(GridModel grid) : base(grid)
        {}

        protected DescriptionNameCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new DescriptionNameCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            if(text.Length > Domain.InterfaceLegacy.Domain.Description.MaxLengthOfName || string.IsNullOrWhiteSpace(text))
                return false;

            style.CellValue = new Description(text, ((Description)style.CellValue).ShortName);
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            String ret = string.Empty;
            if (value is Description)
                ret = ((Description) value).Name;

            return ret;
        }

    }

    public class DescriptionNameCellModelRenderer : GridTextBoxCellRenderer
    {
        public DescriptionNameCellModelRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        /// <summary>
        /// Allows custom formatting of a cell by changing its style object.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// 	<see cref="M:Syncfusion.Windows.Forms.Grid.GridCellRendererBase.OnPrepareViewStyleInfo(Syncfusion.Windows.Forms.Grid.GridPrepareViewStyleInfoEventArgs)"/> is called from <see cref="E:Syncfusion.Windows.Forms.Grid.GridControlBase.PrepareViewStyleInfo"/>
        /// in order to allow custom formatting of
        /// a cell by changing its style object.
        /// <para/>
        /// Set the cancel property true if you want to avoid
        /// the assoicated cell renderers object <see cref="M:Syncfusion.Windows.Forms.Grid.GridCellRendererBase.OnPrepareViewStyleInfo(Syncfusion.Windows.Forms.Grid.GridPrepareViewStyleInfoEventArgs)"/>
        /// method to be called.<para/>
        /// Changes made to the style object will not be saved in the grid nor cached. This event
        /// is called every time a portion of the grid is repainted and the specified cell belongs
        /// to the invalidated region of the window that needs to be redrawn.<para/>
        /// Changes to the style object done at this time will also not be reflected when accessing
        /// cells though the models indexer. See <see cref="E:Syncfusion.Windows.Forms.Grid.GridModel.QueryCellInfo"/>.<para/>
        /// 	<note type="note">Do not change base style or cell type at this time.</note>
        /// </remarks>
        /// <seealso cref="T:Syncfusion.Windows.Forms.Grid.GridPrepareViewStyleInfoEventHandler"/>
        /// <seealso cref="M:Syncfusion.Windows.Forms.Grid.GridControlBase.GetViewStyleInfo(System.Int32,System.Int32)"/>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            // This is the place to override settings deririved from the grid
            e.Style.HorizontalAlignment = GridHorizontalAlignment.Left;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}