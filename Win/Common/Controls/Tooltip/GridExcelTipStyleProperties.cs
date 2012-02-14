using System;
using System.ComponentModel;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Tooltip
{
    public class GridExcelTipStyleProperties : GridStyleInfoCustomProperties
    {
        // static initialization of property descriptors

        static Type t = typeof(GridExcelTipStyleProperties);

        private static StyleInfoProperty ExcelTipTextProperty = CreateStyleInfoProperty(t, "ExcelTipText");
		
        // default settings for all properties this object holds
        static GridExcelTipStyleProperties defaultObject;

        // initialize default settings for all properties in static ctor
        static GridExcelTipStyleProperties ()
        {
            // all properties must be initialized for the Default property
            defaultObject = new GridExcelTipStyleProperties(GridStyleInfo.Default);
            defaultObject.ExcelTipText = "";
        }

        /// <summary>
        /// Provides access to default values for this type
        /// </summary>
        public static GridExcelTipStyleProperties Default
        {
            get
            {
                return defaultObject;
            }
        }

        /// <summary>
        /// Force static ctor being called at least once
        /// </summary>
        public static void Initialize()
        {
        }

        // explicit cast from GridStyleInfo to GridExcelTipStyleProperties
        // (Note: this will only work for C#, Visual Basic does not support dynamic casts)

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
        /// Initializes a GridExcelTipStyleProperties object with an empty style object. Design
        /// time environment will use this ctor and later copy the values to a style object
        /// by calling style.CustomProperties.Add(gridExcelTipStyleProperties1)
        /// </summary>
        public GridExcelTipStyleProperties()
            : base()
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
                //TraceUtil.TraceCurrentMethodInfo();
                return (string)style.GetValue(ExcelTipTextProperty);
            }
            set
            {
                //TraceUtil.TraceCurrentMethodInfo(value);
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
        /// <summary>
        /// Gets if ExcelTipText state has been initialized for the current object.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasExcelTipText
        {
            get
            {
                return style.HasValue(ExcelTipTextProperty);
            }
        }		
    }
}