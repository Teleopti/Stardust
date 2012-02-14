using Syncfusion.Windows.Forms.Grid;
using Teleopti.Messaging.Management.Controls;

namespace Teleopti.Messaging.Management.Model
{
    public class PerformanceGridModel : GridModel
    {
        public PerformanceGridModel()
        {
            CommandStack.Enabled = false;
            Rows.DefaultSize = 17;
            Cols.DefaultSize = 65;
            RowHeights[0] = 21;
            ColWidths[0] = 35;
            RowHeights.ResetModified();
            ColWidths.ResetModified();
            Options.ExcelLikeCurrentCell = false;
            Options.ExcelLikeSelectionFrame = false;
            Options.AllowDragSelectedCols = false;
            Options.AllowDragSelectedRows = false;
            CommandStack.Enabled = false;
            Options.FloatCellsMode = GridFloatCellsMode.None;

            BaseStylesMap.RegisterStandardStyles();
        }

        private bool _useGridNonVirtualDataCache;

        /// <summary>
        /// Property UseGridNonVirtualDataCache (bool)
        /// </summary>
        public bool UseGridNonVirtualDataCache
        {
            get
            {
                return _useGridNonVirtualDataCache;
            }
            set
            {
                if (_useGridNonVirtualDataCache != value)
                {
                    ResetVolatileData();

                    // Replace volatile data cache with a permanent array of objects that stays in memory 
                    // (but also eliminates support for QueryCellInfo and SaveCellInfo in gridModel)
                    _useGridNonVirtualDataCache = value;
                    if (value)
                        VolatileData = new GridNonVirtualDataCache(Model);
                    else
                        VolatileData = new GridVolatileData(Model);
                }
            }
        }
    }

}
