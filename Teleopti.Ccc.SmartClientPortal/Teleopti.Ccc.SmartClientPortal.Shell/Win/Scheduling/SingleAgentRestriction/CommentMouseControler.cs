#region Copyright Syncfusion Inc. 2001 - 2009
//
//  Copyright Syncfusion Inc. 2001 - 2009. All rights reserved.
//
//  Use of this code is subject to the terms of our license.
//  A copy of the current license can be obtained at any time by e-mailing
//  licensing@syncfusion.com. Any infringement will be prosecuted under
//  applicable laws. 
//
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction
{
    /// <summary>
    /// Summary description for CommentMouseController.
    /// </summary>
    public class CommentMouseController : IMouseController, IDisposable
    {
        private const int blackArrowSize = 5;

        private GridControlBase owner;
        private int lastHitTestCode;
        private const int HitComment = 101;

        private CellComment commentWindow;
        private int cornerSize = 8;
        private Rectangle redrawRect = Rectangle.Empty;

        private Brush cornerBrush1;

        private int CommentCornerSize
        {
            get { return cornerSize; }
        }

        public CommentMouseController(GridControlBase owner)
        {
            this.owner = owner;
            commentWindow = new CellComment();
            commentWindow.CommentCorner = new Size(0, 0);
            owner.Controls.Add(commentWindow);

            owner.CellDrawn += grid_CellDrawn;
            cornerBrush1 = Brushes.Red; //default color

            GridExcelTipStyleProperties.Initialize();
        }

        public string Name
        {
            get { return "Comment"; }
        }

        public Cursor Cursor
        {
            get
            {
                return Cursors.Default;
            }
        }

        
        #region Mouse Overrides
        public void MouseHoverEnter()
        {

        }

        /// <summary>
        /// User is moving the mouse over the hot-test area
        /// </summary>
        /// <param name="e"></param>
        public void MouseHover(MouseEventArgs e)
        {
            if (!commentWindow.Visible)// && !inDraw)
            {
                //translate the point to top right corner...
                int rowIndex, colIndex;
                Point pt = new Point(e.X, e.Y);

                owner.PointToRowCol(pt, out rowIndex, out colIndex, -1);
                bool hidden;
                int clientRow = owner.ViewLayout.RowIndexToVisibleClient(rowIndex, out hidden);
                int clientCol = owner.ViewLayout.ColIndexToVisibleClient(colIndex, out hidden);
                pt = owner.ViewLayout.ClientRowColToPoint(clientRow, clientCol, GridCellSizeKind.ActualSize);

                pt.X += owner.Model.ColWidths[colIndex];
                GridExcelTipStyleProperties style = (GridExcelTipStyleProperties)owner.Model[rowIndex, colIndex];
                //commentWindow.InitializeComment(rowIndex, pt, style);
                commentWindow.InitializeComment(rowIndex, pt, style);
                Point ploc = commentWindow.Location;
                commentWindow.Location = new Point(10000, 10000);
                commentWindow._comment.Visible = true;
                commentWindow._editTextBox.Visible = false;

                //show the windows
                commentWindow.Show();
                commentWindow.Update();
                owner.Update();

                //draw the pointer
                Graphics g = Graphics.FromHwnd(owner.Handle);
                Point pt1 = new Point(pt.X + commentWindow.CommentCorner.Width, pt.Y - commentWindow.CommentCorner.Height);
                redrawRect = new Rectangle(pt.X, pt.Y - commentWindow.CommentCorner.Height, commentWindow.CommentCorner.Width + 1, commentWindow.CommentCorner.Height + 1);
                g.DrawLine(Pens.Black, pt, pt1);
                Point topLeft = new Point(pt.X, pt.Y - blackArrowSize + 1);
                Point bottomLeft = new Point(pt.X, pt.Y + 1);
                Point bottomRight = new Point(pt.X + blackArrowSize - 1, pt.Y + 1);
                g.FillPolygon(Brushes.Black, new [] { topLeft, bottomLeft, bottomRight, topLeft });
                g.Dispose();
                commentWindow.Location = ploc;
            }
        }

        /// <summary>
        /// Called when the hovering ends, either when user has moved mouse away from hittest area
        /// or when the user has pressed a mouse button.
        /// </summary>
        public void MouseHoverLeave(EventArgs e)
        {
            if (commentWindow._comment.Visible)
            {
                commentWindow.Hide();
                owner.Invalidate(redrawRect);
            }
        }

        public void MouseDown(MouseEventArgs e)
        {
        }

        /// <summary>
        /// User has dragged mouse. If mouse is down, set current position.
        /// </summary>
        /// <param name="e"></param>
        public void MouseMove(MouseEventArgs e)
        {

        }

        /// <summary>
        /// User has release mouse button. Stop automatic scrolling.
        /// </summary>
        /// <param name="e"></param>
        public void MouseUp(MouseEventArgs e)
        {
        }

        #endregion

        public void CancelMode()
        {
            if (commentWindow.Visible)
            {
                commentWindow.Hide();
                owner.Invalidate(redrawRect);
            }
        }

        public int HitTest(MouseEventArgs mouseEventArgs, IMouseController controller)
        {
            lastHitTestCode = GridHitTestContext.None;

            Point pt = new Point(mouseEventArgs.X, mouseEventArgs.Y);
            int rowIndex, colIndex;
            owner.PointToRowCol(pt, out rowIndex, out colIndex);
            Rectangle rect = GetCorner(rowIndex, colIndex);
            if (rect.Contains(pt) && !commentWindow._editTextBox.Visible)
            {
                GridStyleInfo style = owner.Model[rowIndex, colIndex];
                if (isExcelTipCell(style))
                {
                    lastHitTestCode = HitComment;
                }
            }

            return lastHitTestCode;
        }

        Rectangle GetCorner(int row, int col)
        {
            Rectangle bounds = owner.RangeInfoToRectangle(GridRangeInfo.Cell(row, col), GridRangeOptions.None);
            bounds = new Rectangle(bounds.X + bounds.Width - CommentCornerSize - 1, bounds.Y, CommentCornerSize, CommentCornerSize);
            bounds.Intersect(owner.ClientRectangle);
            return bounds;
        }

        private static bool isExcelTipCell(GridStyleInfo style)
        {
            GridExcelTipStyleProperties style1 = new GridExcelTipStyleProperties(style);
            return style1.HasExcelTipText;
        }

        private void grid_CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (isExcelTipCell(e.Style) && (!owner.CurrentCell.HasCurrentCellAt(e.RowIndex, e.ColIndex)
                                            || !owner.CurrentCell.IsEditing))
            {
                Point topLeft = new Point(e.Bounds.X + e.Bounds.Width - CommentCornerSize, e.Bounds.Y);
                Point topRight = new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y);
                Point bottomRight = new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y + CommentCornerSize);
                e.Graphics.FillPolygon(cornerBrush1, new [] { topLeft, topRight, bottomRight, topLeft });
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                commentWindow.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class GridExcelTipStyleProperties : GridStyleInfoCustomProperties
    {
        // static initialization of property descriptors
        static Type t = typeof(GridExcelTipStyleProperties);

        readonly static StyleInfoProperty ExcelTipTextProperty = CreateStyleInfoProperty(t, "ExcelTipText");

        // default settings for all properties this object holds
        static GridExcelTipStyleProperties defaultObject;

        // initialize default settings for all properties in static ctor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GridExcelTipStyleProperties()
        {
            // all properties must be initialized for the Default property
            defaultObject = new GridExcelTipStyleProperties(GridStyleInfo.Default);
            defaultObject.ExcelTipText = "";
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
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
        [EditorBrowsable(EditorBrowsableState.Never)]
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