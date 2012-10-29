using System;
using System.Drawing;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Meetings
{
    public class MeetingPanel : UserControl
    {
        private bool _mouseDownClicked;
        private bool _resizeCursorMode;
        private bool _resizeMode;
        private Point _mousePosition;
        //private readonly IList<Control> _controlsHooked;
        //private Control _parentHooked;

        internal MeetingPanel()
        {
            //InitializeComponent();
        }

        public event EventHandler Dragged;

        public event MouseEventHandler MouseMoveInsidePanel;

        //internal int ScrollOffset { set; get; }

        protected override void OnPaint(PaintEventArgs e)
        {
            DoPaint(e);
        }

        private void DoPaint(PaintEventArgs e)
        {
            var pointX = new PointF(0, 0);
            var pointY = new PointF(0, Height);
            var pointP = new PointF(Width, Height);
            var pointQ = new PointF(Width, 0);

            //var grid = (GridControl)Parent;
            //if (RightToLeft==RightToLeft.Yes)
            //{
            //    var widthToHide = grid.Right - grid.Margin.Right - grid.ColWidths[0] - Right;
            //    if (widthToHide < 0)
            //    {
            //        var rect = e.ClipRectangle;
            //        rect.Width += widthToHide;
            //        e.Graphics.Clip = new Region(rect);
            //    }
            //}
            //else
            //{
            //    var widthToHide = Location.X - grid.ColWidths[0];
            //    if (widthToHide < 0)
            //    {
            //        var rect = e.ClipRectangle;
            //        rect.Width += widthToHide;
            //        rect.Location = new Point(rect.Location.X - widthToHide, rect.Location.Y);
            //        e.Graphics.Clip = new Region(rect);
            //    }
            //}

            using (var bluePen = new Pen(Brushes.Blue, 10))
            {
                e.Graphics.DrawLine(bluePen, pointX, pointY);
            }

            using (var redPen = new Pen(Brushes.Red, 10))
            {
                e.Graphics.DrawLine(redPen, pointP, pointQ);
            }

            using (var blackPen = new Pen(Brushes.Black, 3))
            {
                pointX.X = 5;
                pointQ.X = pointQ.X - 5;
                pointY.X = 5;
                pointP.X = pointP.X - 5;
                e.Graphics.DrawLine(blackPen, pointX, pointQ);
                e.Graphics.DrawLine(blackPen, pointY, pointP);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!_resizeCursorMode)
                {
                    _mouseDownClicked = true;
                    _mousePosition = new Point(e.X, e.Y);
                }
                else
                {
                    _resizeMode = true;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _mouseDownClicked = false;
            _resizeCursorMode = false;
            _resizeMode = false;

        	var handler = Dragged;
            if (handler != null)
            {
            	handler(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_mouseDownClicked && MouseMoveInsidePanel!=null) MouseMoveInsidePanel.Invoke(this,e);

            var grid = (GridControl)Parent;

        	int leftBound = grid.ColWidths[0];
            int secondColumnWidth = grid.ColWidths[1];
			
            if (!_resizeMode)
            {
                HandlePanelMove(e, leftBound, secondColumnWidth);
                HandleCursorResizeMode(e);
            }
            else
            {
                HandleResize(e, secondColumnWidth);
            }
            
        }

        private void HandleResize(MouseEventArgs e, int secondColumnWidth)
        {
            int defaultPanelWidth = secondColumnWidth;
            if (e.X > 1)
            {
                int width;
                if (e.X > secondColumnWidth)
                {
                    width = e.X > secondColumnWidth ? defaultPanelWidth : e.X;
                }
                else
                {
                    int remainingWidth = (secondColumnWidth - Location.X) - Size.Width;
                    if (remainingWidth <= 0)
                    {
                        width = (secondColumnWidth - Location.X);
                    }
                    else
                    {
                        width = e.X > defaultPanelWidth ? defaultPanelWidth : e.X;
                    }
                }
                Width = width;
            }
        }

        private void HandleCursorResizeMode(MouseEventArgs e)
        {
            _resizeCursorMode = false;
            if (e.Location.X >= (Size.Width - 4) && e.Location.X <= (Size.Width))
            {
                Cursor.Current = Cursors.VSplit;
                _resizeCursorMode = true;
            }
        }

    	private void HandlePanelMove(MouseEventArgs e, int leftBound, int secondColumnWidth)
        {
            Cursor = Cursors.Hand;
            if (_mouseDownClicked)
            {
                int xCordOfThePanel;
                int yCordOfThePanel;

                Point pointToClient = Parent.PointToClient(PointToScreen(new Point((e.X - _mousePosition.X), Location.Y)));
                int remainingWidth = (secondColumnWidth - Location.X);

				if ((pointToClient.X < leftBound) ||
                    (remainingWidth < Size.Width))
                {
                    if (remainingWidth < Size.Width)
                    {
                        xCordOfThePanel = (secondColumnWidth - Size.Width);
                    }
                    else
                    {
						xCordOfThePanel = leftBound;
                    }
                    yCordOfThePanel = Location.Y;
                }
                else
                {
                    remainingWidth = secondColumnWidth - pointToClient.X;
                    if (remainingWidth > secondColumnWidth)
                    {
                        xCordOfThePanel = (secondColumnWidth - Size.Width);
                    }
                    else
                    {
                        if (remainingWidth > Size.Width)
                        {
                            xCordOfThePanel = pointToClient.X;
                        }
                        else
                        {
                            if ((Size.Width - remainingWidth) != 0)
                                xCordOfThePanel = (Location.X + 1);
                            else
                                xCordOfThePanel = (secondColumnWidth - Size.Width);
                        }
                    }
                    yCordOfThePanel = Location.Y;
                }
                Location = new Point(xCordOfThePanel, yCordOfThePanel);
                Parent.Invalidate();
            }
        }

        //private void RemoveControl(Control control)
        //{
        //    if (_controlsHooked.Contains(control))
        //    {
        //        control.Paint -= OnControlPaint;
        //        _controlsHooked.Remove(control);
        //    }
        //}

        //private void AddControl(Control control)
        //{
        //    if (!_controlsHooked.Contains(control))
        //    {
        //        control.Paint += OnControlPaint;
        //        _controlsHooked.Add(control);
        //    }
        //}

        //private void OnMeetingPanelParentChanged(object sender, EventArgs e)
        //{
        //    UnhookParent();
        //    if (Parent != null)
        //    {
        //        foreach (Control c in Parent.Controls)
        //        {
        //            AddControl(c);
        //        }
        //        Parent.ControlAdded += OnParentControlAdded;
        //        Parent.ControlRemoved += OnParentControlRemoved;
        //        _parentHooked = Parent;
        //    }
        //}

        //private void UnhookParent()
        //{
        //    if (_parentHooked != null)
        //    {
        //        foreach (Control c in _parentHooked.Controls)
        //        {
        //            RemoveControl(c);
        //        }
        //        _parentHooked.ControlAdded -= OnParentControlAdded;
        //        _parentHooked.ControlRemoved -= OnParentControlRemoved;
        //        _parentHooked = null;
        //    }
        //}

        //private void OnParentControlRemoved(object sender, ControlEventArgs e)
        //{
        //    RemoveControl(e.Control);
        //}

        //private void OnControlPaint(object sender, PaintEventArgs e)
        //{
        //    int indexa = Parent.Controls.IndexOf(this), indexb = Parent.Controls.IndexOf((Control) sender);
        //    //if above invalidate on paint
        //    if (indexa < indexb)
        //    {
        //        Invalidate();
                
        //    }
        //}

        //private void OnParentControlAdded(object sender, ControlEventArgs e)
        //{
        //    AddControl(e.Control);
        //}

        //private void InitializeComponent()
        //{
        //    SuspendLayout();
        //    // 
        //    // MeetingPanel
        //    // 
        //    BackColor = Color.Transparent;
        //    Margin = new Padding(0);
        //    Name = "MeetingPanel";
        //    ResumeLayout(false);

        //}
    }
}
