#region Copyright Syncfusion Inc. 2001 - 2008
//
//  Copyright Syncfusion Inc. 2001 - 2008. All rights reserved.
//
//  Use of this code is subject to the terms of our license.
//  A copy of the current license can be obtained at any time by e-mailing
//  licensing@syncfusion.com. Any infringement will be prosecuted under
//  applicable laws. 
//
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Tooltip
{
    /// <summary>
    /// Summary description for CommentMouseController.
    /// </summary>
    public class CommentMouseController : IMouseController, IDisposable
    {
        private const int _blackArrowSize = 5;

        private ContextMenu _contextMenu;
        private int _rightClickRow, _rightClickCol;

        private GridControlBase _owner;
        private IAnnotatableGrid annotatableGrid;

        private int _lastHitTestCode; // = GridHitTestContext.None;
        private const int HitComment = 101;
		
        private GridCellComment _commentWindow;
        private int _cornerSize = 8;
        private Rectangle _redrawRect = Rectangle.Empty;
		
        private int _commentRow;
        private int _commentCol;

        private bool _contextMenuEnabled;

        private Brush _cornerBrush;
        private int _originalContextMenuItemCount;

        public bool ContextMenuEnabled
        {
            get{return _contextMenuEnabled;}
            set
            {
                if(value != _contextMenuEnabled)
                {
                    _contextMenuEnabled = value;
                    if(_contextMenuEnabled)
                    {
                        _owner.MouseDown += new MouseEventHandler(grid_ContextMouseDown);
                    }
                    else
                    {
                        _owner.MouseDown -= new MouseEventHandler(grid_ContextMouseDown);
                    }
                }
            }
        }

        public int CommentCornerSize
        {
            get{return _cornerSize;}
            set{_cornerSize = value;}
        }

        public CommentMouseController(GridControlBase owner)
        {
            _owner = owner;
            annotatableGrid = (IAnnotatableGrid) _owner;
            _commentWindow = new GridCellComment();
            _commentWindow.CommentCorner = new Size(0,0);
            _owner.Controls.Add(_commentWindow);

            _owner.CellDrawn += new GridDrawCellEventHandler(grid_CellDrawn);

            _owner.TopRowChanging += new GridRowColIndexChangingEventHandler(grid_Scrolling);
            _owner.LeftColChanging += new GridRowColIndexChangingEventHandler(grid_Scrolling);
            _owner.MouseDown += new MouseEventHandler(grid_MouseDown);

            _cornerBrush = Brushes.Red; //default color

            GridExcelTipStyleProperties.Initialize();
        }

        public string Name 
        { 
            get{return "Comment"; }
        }

        public Cursor Cursor 
        { 
            get
            {
                return Cursors.Default;
            }
        }

        #region Context Menu Code

        private void grid_ContextMouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                Point pt = new Point(e.X, e.Y);
                if(_owner.PointToRowCol(pt, out _rightClickRow, out _rightClickCol))
                {
                    MenuItem menu;
                    if (annotatableGrid.IsAnnotatableCell(_rightClickCol, mainHeaderRow))
					{
                        if (_owner.ContextMenu != null)
                        {
                            _originalContextMenuItemCount = _owner.ContextMenu.MenuItems.Count;
                            _owner.ContextMenu.Collapse += new EventHandler(ContextMenu_Collapse);
                        }

                        _contextMenu = new ContextMenu();
                        MenuItem separator = new MenuItem("-");
                        _contextMenu.MenuItems.Add(separator);

                        if (isExcelTipCell(_owner.Model[mainHeaderRow, _rightClickCol]))
                        {
                            menu = new MenuItem(UserTexts.Resources.RemoveNote, new EventHandler(remove_Comment));
                            MenuItem menu1 = new MenuItem(UserTexts.Resources.EditNote, new EventHandler(edit_Comment));
                            _contextMenu.MenuItems.Add(menu);
                            _contextMenu.MenuItems.Add(menu1);
                        }
                        else
                        {
                            menu = new MenuItem(UserTexts.Resources.AddNote, new EventHandler(add_Comment));
                            _contextMenu.MenuItems.Add(menu);
                        }

                        if (_owner.ContextMenu != null)
                        {
                            // Merge original context menu with the comment menu items
                            _owner.ContextMenu.MergeMenu(_contextMenu);
                        }
					}
                }
            }
        }

        private void ContextMenu_Collapse(object sender, EventArgs e)
        {
            _owner.ContextMenu.Collapse -= new EventHandler(ContextMenu_Collapse);

            // Remove comment menu items
            int mergedMenuItemCount = _owner.ContextMenu.MenuItems.Count;
            for (int itemIndex = _originalContextMenuItemCount; itemIndex < mergedMenuItemCount; itemIndex++)
            {
                _owner.ContextMenu.MenuItems.RemoveAt(_originalContextMenuItemCount);
            }
        }

        private static int mainHeaderRow
        {
            get { return 1; }
        }

        private void edit_Comment(object sender, EventArgs e)
        {
            StartEditing();
        }

        private void StartEditing()
        {
            GridExcelTipStyleProperties style = new GridExcelTipStyleProperties(_owner.Model[mainHeaderRow, _rightClickCol]);

            Point pt = _owner.ViewLayout.ClientRowColToPoint(mainHeaderRow, _rightClickCol, GridCellSizeKind.ActualSize);
            pt.X += _owner.Model.ColWidths[_rightClickCol];
            _commentWindow.InitializeComment(mainHeaderRow, pt, style);

            EditComment(mainHeaderRow, _rightClickCol);
        }

        private void add_Comment(object sender, EventArgs e)
        {
            GridExcelTipStyleProperties style = new GridExcelTipStyleProperties(_owner.Model[mainHeaderRow, _rightClickCol]);
            style.ExcelTipText = "";
            StartEditing();
        }

        private void remove_Comment(object sender, EventArgs e)
        {
            if (annotatableGrid.IsAnnotatableCell(_rightClickCol, mainHeaderRow))
            {
                IAnnotatable annotatable = annotatableGrid.GetAnnotatableObject(_rightClickCol);
                annotatable.Annotation = string.Empty;
            }
            GridExcelTipStyleProperties style = new GridExcelTipStyleProperties(_owner.Model[mainHeaderRow, _rightClickCol]);
            style.ResetExcelTipText();
        }
        #endregion

        #region Mouse Overrides
        public void MouseHoverEnter()
        {

        }

        //private bool inDraw = false;
        /// <summary>
        /// User is moving the mouse over the hot-test area
        /// </summary>
        /// <param name="e"></param>
        public void MouseHover(MouseEventArgs e)
        {
            if(!_commentWindow.Visible)// && !inDraw)
            {
                //inDraw = true;

                //translate the point to top right corner...
                int rowIndex, colIndex;
                Point pt = new Point(e.X, e.Y);
				
                _owner.PointToRowCol(pt, out rowIndex, out colIndex, -1); 
                bool hidden; 
                int clientRow = _owner.ViewLayout.RowIndexToVisibleClient(rowIndex, out hidden); 
                int clientCol = _owner.ViewLayout.ColIndexToVisibleClient(colIndex, out hidden); 
                pt = _owner.ViewLayout.ClientRowColToPoint(clientRow, clientCol, GridCellSizeKind.ActualSize); 

                pt.X += _owner.Model.ColWidths[colIndex];
		
                GridExcelTipStyleProperties style = (GridExcelTipStyleProperties) _owner.Model[rowIndex, colIndex];
                _commentWindow.InitializeComment(rowIndex, pt, style);
                Point ploc = _commentWindow.Location;
                _commentWindow.Location = new Point(10000, 10000);
                _commentWindow._comment.Visible = true;
                _commentWindow._editTextBox.Visible = false;

                //show the windows
                _commentWindow.Show();
                _commentWindow.Update();
                _owner.Update();
				
                //draw the pointer
                Graphics g = Graphics.FromHwnd(_owner.Handle);
                Point pt1 = new Point(pt.X + _commentWindow.CommentCorner.Width, pt.Y - _commentWindow.CommentCorner.Height);
                _redrawRect = new Rectangle(pt.X, pt.Y - _commentWindow.CommentCorner.Height, _commentWindow.CommentCorner.Width + 1, _commentWindow.CommentCorner.Height + 1);
                g.DrawLine(Pens.Black, pt, pt1);
                Point topLeft = new Point(pt.X, pt.Y - _blackArrowSize + 1);
                Point bottomLeft = new Point(pt.X, pt.Y + 1);
                Point bottomRight = new Point(pt.X + _blackArrowSize - 1, pt.Y + 1);
                g.FillPolygon(Brushes.Black, new Point[] { topLeft, bottomLeft, bottomRight, topLeft });
                g.Dispose();
//Console.WriteLine("draw the pointer" + Environment.TickCount.ToString());  
				
                //inDraw = false;
                _commentWindow.Location = ploc;
            }
        }
 
        /// <summary>
        /// Called when the hovering ends, either when user has moved mouse away from hittest area
        /// or when the user has pressed a mouse button.
        /// </summary>
        public void MouseHoverLeave(EventArgs e)
        {
            if(_commentWindow._comment.Visible)
            {
                _commentWindow.Hide();
                _owner.Invalidate(_redrawRect);
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
            if(_commentWindow.Visible)
            {
                _commentWindow.Hide();
                _owner.Invalidate(_redrawRect);
            }
        }

        public int HitTest(MouseEventArgs mouseEventArgs, IMouseController controller)
        {
            _lastHitTestCode = GridHitTestContext.None;

            Point pt = new Point(mouseEventArgs.X, mouseEventArgs.Y);
            int rowIndex, colIndex;
            _owner.PointToRowCol(pt, out rowIndex, out colIndex);
            Rectangle rect = GetCorner(rowIndex, colIndex);
            if(rect.Contains(pt) && !_commentWindow._editTextBox.Visible)
            {
                GridStyleInfo style = _owner.Model[rowIndex, colIndex];
                if(isExcelTipCell(style))
                {
                    _lastHitTestCode = HitComment;
                }
            }

            return _lastHitTestCode;
        }

        Rectangle GetCorner(int row, int col)
        {
            Rectangle bounds = _owner.RangeInfoToRectangle(GridRangeInfo.Cell(row, col), GridRangeOptions.None);
            bounds = new Rectangle(bounds.X + bounds.Width - CommentCornerSize - 1, bounds.Y , CommentCornerSize, CommentCornerSize);
            bounds.Intersect(_owner.ClientRectangle);
            return bounds;
        }

        private bool isExcelTipCell(GridStyleInfo style)
        {
            if (annotatableGrid.IsAnnotatableCell(style.CellIdentity.ColIndex, style.CellIdentity.RowIndex))
            {
                IAnnotatable annotatable = annotatableGrid.GetAnnotatableObject(style.CellIdentity.ColIndex);
                if (annotatable != null)
                {
                    return (!string.IsNullOrEmpty(annotatable.Annotation));
                }
            }
            return false;
        }

        private void grid_CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if(isExcelTipCell(e.Style) && (!_owner.CurrentCell.HasCurrentCellAt(e.RowIndex, e.ColIndex) 
                                           || !_owner.CurrentCell.IsEditing))
            {
                Point topLeft = new Point(e.Bounds.X + e.Bounds.Width - CommentCornerSize, e.Bounds.Y);
                Point topRight = new Point(e.Bounds.X + e.Bounds.Width , e.Bounds.Y);
                Point bottomRight = new Point(e.Bounds.X + e.Bounds.Width , e.Bounds.Y + CommentCornerSize);
                if (e.RowIndex == mainHeaderRow && annotatableGrid.IsAnnotatableCell(e.ColIndex, e.RowIndex))
                {
                    IAnnotatable annotatable = annotatableGrid.GetAnnotatableObject(e.ColIndex);
                    if (!string.IsNullOrEmpty(annotatable.Annotation))
                    {
                        e.Graphics.FillPolygon(_cornerBrush, new Point[] { topLeft, topRight, bottomRight, topLeft });
                    }
                }
            }
        }


        #region Comment Edit Code

        private void EditComment(int row, int col)
        {
            _commentRow = row;
            _commentCol = col;

            Point pt = _commentWindow._comment.Location;
            pt.Offset(5, 5);
            _commentWindow._editTextBox.Location = pt;
					
            _commentWindow._editTextBox.Size = _commentWindow._comment.Size - new Size(5, 5);
            //_commentWindow.Show();
            _commentWindow._comment.Hide();
					
            //make sure there is space for some more lines...
            int diff =  Math.Max(_commentWindow.Height, 60) - _commentWindow.Height;
            if(diff > 0)
            {
                _commentWindow.Height += diff;
                _commentWindow._editTextBox.Height += diff;

            }

            //make wide enough
            _commentWindow._editTextBox.Width = Math.Max(_commentWindow.Width - 10, _commentWindow._editTextBox.Width);
            _commentWindow._editTextBox.MaxLength = 512;
            _commentWindow._editTextBox.WordWrap = true;

            _commentWindow._editTextBox.Show();
            _commentWindow.ActiveControl = _commentWindow._editTextBox;

            _commentWindow.Leave += new EventHandler(textBox_Leave);
            _commentWindow._editTextBox.TextChanged += new EventHandler(textbox_Changed);

            //_commentWindow.Parent = null;
            //_commentWindow.TopLevel = true;
            _commentWindow.Show();
            _commentWindow._editTextBox.Focus();
            _owner.WantKeys = false;
        }


        private void textBox_Leave(object sender, EventArgs e)
        {
            if(_commentWindow._editTextBox.Modified)
            {
                IAnnotatable annotatable = annotatableGrid.GetAnnotatableObject(_commentCol);
                annotatable.Annotation = _commentWindow._editTextBox.Text;
                GridExcelTipStyleProperties style = new GridExcelTipStyleProperties(_owner.Model[_commentRow, _commentCol]);
                style.ExcelTipText = _commentWindow._editTextBox.Text;
                _commentWindow._editTextBox.Modified = false;
            }

            _commentWindow.Leave -= new EventHandler(textBox_Leave);
            _commentWindow._editTextBox.TextChanged -= new EventHandler(textbox_Changed);

            _commentWindow._editTextBox.Hide();
            _commentWindow.Hide();
            _owner.Invalidate(_redrawRect);
            _owner.WantKeys = true;
        }
        private void textbox_Changed(object sender, EventArgs e)
        {
            _commentWindow._editTextBox.Modified = true;
        }

        private void grid_Scrolling(object sender, GridRowColIndexChangingEventArgs e)
        {
            CancelMode();
        }

        private void grid_MouseDown(object sender, MouseEventArgs e)
        {
            int rowIndex, colIndex;
            _owner.PointToRowCol(new Point(e.X, e.Y), out rowIndex, out colIndex);
            if((rowIndex != _commentRow || colIndex != _commentRow) 
               && _commentWindow._editTextBox.Visible)
                textBox_Leave(sender, e);
        }

        #endregion

        public void Dispose()
        {
            if (_contextMenu!=null)_contextMenu.Dispose();
            if (_commentWindow!=null)_commentWindow.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}