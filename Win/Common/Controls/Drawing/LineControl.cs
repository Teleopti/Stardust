using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls.Drawing
{
    /// <summary>
    /// Line control.
    /// </summary>
    public partial class LineControl : TransparentUserControl
    {
        private LineDisplayInfo _lineDisplayInfo = new LineDisplayInfo();

        /// <summary>
        /// Initializes a new instance of the <see cref="LineControl"/> class.
        /// </summary>
        public LineControl()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.None;
        }

        /// <summary>
        /// Gets or sets the line display info.
        /// </summary>
        /// <value>The line display info.</value>
        [TypeConverter(typeof(LineDisplayInfoConverter))]
        public LineDisplayInfo LineDisplayInfo
        {
            get 
            {
                return _lineDisplayInfo; 
            }
            set 
            {
                _lineDisplayInfo = value;
                Refresh();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawLine(e.Graphics, new Point(_lineDisplayInfo.Padding.Left, _lineDisplayInfo.Padding.Top), new Point(Width - _lineDisplayInfo.Padding.Right, Height - _lineDisplayInfo.Padding.Bottom));
        }

        /// <summary>
        /// Draws an arrow bettwen two points.
        /// </summary>
        /// <param name="gr">The gr.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        private void DrawLine(Graphics gr, Point start, Point end)
        {
            if (_lineDisplayInfo.LineVisible)
            {
                using (Pen pen = new Pen(_lineDisplayInfo.LineColor, _lineDisplayInfo.LineWidth))
                {
                    if (LineDisplayInfo.RightArrow)
                        pen.StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                    else
                        pen.StartCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
                    if (LineDisplayInfo.LeftArrow)
                        pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                    else
                        pen.EndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
                    gr.DrawLine(pen, start, end);
                }
            }
        }

        /// <summary>
        /// Gets or sets padding within the control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Windows.Forms.Padding"/> representing the control's internal spacing characteristics.</returns>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// </PermissionSet>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Padding Padding
        {
           get{ return base.Padding;}
           set{ base.Padding = value; }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control.
        /// </summary>
        /// <value></value>
        /// <returns>The foreground <see cref="T:System.Drawing.Color"/> of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultForeColor"/> property.</returns>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// </PermissionSet>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }
    }

    /// <summary>
    /// Property editor displayed info about line.
    /// </summary>
    public class LineDisplayInfo
    {
        private bool _rightArrow;
        private bool _leftArrow;
        private int _lineWidth = 1;
        private Padding _padding = new Padding(5,5,5,5);
        private Color _lineColor = SystemColors.ControlText;
        private bool _visible = true;

        /// <summary>
        /// Gets or sets the width of the line.
        /// </summary>
        /// <value>The width of the line.</value>
        [DefaultValue(1)]
        public int LineWidth
        {
            get { return _lineWidth; }
            set
            {
                _lineWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the line has a right arrow.
        /// </summary>
        /// <value><c>true</c> if [right arrow]; otherwise, <c>false</c>.</value>
        public bool RightArrow
        {
            get { return _rightArrow; }
            set { _rightArrow = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the line has a left arrow.
        /// </summary>
        /// <value><c>true</c> if [left arrow]; otherwise, <c>false</c>.</value>
        public bool LeftArrow
        {
            get { return _leftArrow; }
            set { _leftArrow = value; }
        }

        /// <summary>
        /// Gets or sets the padding.
        /// </summary>
        /// <value>The padding.</value>
        [DefaultValue(typeof(Padding), "5;5;5;5")]
        public Padding Padding
        {
            get { return _padding; }
            set { _padding = value; }
        }

        /// <summary>
        /// Gets or sets the color of the line.
        /// </summary>
        /// <value>The color of the line.</value>
        [DefaultValue(typeof(Color), "SystemColors.ControlText")]
        public Color LineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the line is visible.
        /// </summary>
        /// <value><c>true</c> if [line visible]; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        public bool LineVisible
        {
            get { return _visible; }
            set { _visible = value; }
        }

    }

    /// <summary>
    /// Converter class to manage the display of the class in the designer editor. 
    /// </summary>
    public class LineDisplayInfoConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this object supports properties, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <returns>
        /// true if <see cref="M:System.ComponentModel.TypeConverter.GetProperties(System.Object)"/> should be called to find the properties of this object; otherwise, false.
        /// </returns>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of properties for the type of array specified by the value parameter, using the specified context and attributes.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="value">An <see cref="T:System.Object"/> that specifies the type of array for which to get properties.</param>
        /// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used as a filter.</param>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> with the properties that are exposed for this data type, or null if there are no properties.
        /// </returns>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(LineDisplayInfo));
        }
    
    }
}
