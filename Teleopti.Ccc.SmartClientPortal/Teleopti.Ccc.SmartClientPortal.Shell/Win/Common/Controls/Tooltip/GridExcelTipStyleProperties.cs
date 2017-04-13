using System;
using System.ComponentModel;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Tooltip
{
    public class GridExcelTipStyleProperties : GridStyleInfoCustomProperties
    {
        private static Type t = typeof (GridExcelTipStyleProperties);
        private static StyleInfoProperty ExcelTipTextProperty = CreateStyleInfoProperty(t, "ExcelTipText");
		
        /// <summary>
        /// Force static ctor being called at least once
        /// </summary>
        public static void Initialize()
        {
        }

        /// <summary>
        /// Explicit cast from GridStyleInfo to this custom propety object
        /// </summary>
        /// <returns>A new custom properties object.</returns>
        public static explicit operator GridExcelTipStyleProperties(GridStyleInfo style)
        {
            return new GridExcelTipStyleProperties(style);
        }

        /// <summary>
        /// Initializes a GridExcelTipStyleProperties object with a style object that holds all data
        /// </summary>
        public GridExcelTipStyleProperties(GridStyleInfo style)
            : base(style)
        {
        }

        /// <summary>
        /// Gets or sets ExcelTipText state
        /// </summary>
        [
            Description("Provides the ExcelTipText for this cell"),
            Browsable(true),
            Category("StyleCategoryBehavior")
        ]
        public string ExcelTipText
        {
            get
            {
                return (string)style.GetValue(ExcelTipTextProperty);
            }
            set
            {
                style.SetValue(ExcelTipTextProperty, value);
            }
        }
        /// <summary>
        /// Resets ExcelTipText state
        /// </summary>
        public void ResetExcelTipText()
        {
            style.ResetValue(ExcelTipTextProperty);
        }
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        private bool ShouldSerializeExcelTipText()
        {
            return style.HasValue(ExcelTipTextProperty);
        }
    }
}