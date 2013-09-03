using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Teleopti.Ccc.Win.Common.Controls.Drawing
{
    /// <summary>
    /// Rectangle control.
    /// </summary>
    public partial class RectangleControl : TransparentUserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleControl"/> class.
        /// </summary>
        public RectangleControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the top line display info.
        /// </summary>
        /// <value>The top line display info.</value>
        [Category("LinesDisplayInfo")]
        [TypeConverter(typeof(LineDisplayInfoConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LineDisplayInfo TopLineDisplayInfo
        {
            get { return topLine.LineDisplayInfo; }
        }

        /// <summary>
        /// Gets the left line display info.
        /// </summary>
        /// <value>The left line display info.</value>
        [Category("LinesDisplayInfo")]
        [TypeConverter(typeof(LineDisplayInfoConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LineDisplayInfo LeftLineDisplayInfo
        {
            get { return leftLine.LineDisplayInfo; }
        }

        /// <summary>
        /// Gets the right line display info.
        /// </summary>
        /// <value>The right line display info.</value>
        [Category("LinesDisplayInfo")]
        [TypeConverter(typeof(LineDisplayInfoConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LineDisplayInfo RightLineDisplayInfo
        {
            get { return rightLine.LineDisplayInfo; }
        }

        /// <summary>
        /// Gets the bottom line display info.
        /// </summary>
        /// <value>The bottom line display info.</value>
        [Category("LinesDisplayInfo")]
        [TypeConverter(typeof(LineDisplayInfoConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LineDisplayInfo BottomLineDisplayInfo
        {
            get { return bottomLine.LineDisplayInfo; }
        }
    }
}
