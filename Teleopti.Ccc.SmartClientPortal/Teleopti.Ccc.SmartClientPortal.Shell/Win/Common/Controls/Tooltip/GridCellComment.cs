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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Tooltip
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class GridCellComment : Form
    {
        internal Label _comment;
        internal RichTextBox _editTextBox;
        private int _dx = 10;
        private int _dy = 10;
        private int _minCommentHeight = 18;
        private int _minCommentWidth = 80;
        private Point[] _points;

        
        public GridCellComment()
        {
            int width = _minCommentWidth;
            int height = _minCommentHeight;

            _comment = new Label();
            _editTextBox = new RichTextBox();
            _editTextBox.Visible = false;
            _editTextBox.Multiline = true;
            _editTextBox.BorderStyle = BorderStyle.None;

            SuspendLayout();

            _comment.Anchor = (((AnchorStyles.Top | AnchorStyles.Bottom)
                                | AnchorStyles.Left)
                               | AnchorStyles.Right);
            _comment.Location = new Point(_dx, _dy);
            _comment.Name = "Comment:";
            _comment.Size = new Size(width - 2*_dx, height - 2*_dy);
            _comment.TabIndex = 0;
            _comment.Text = "Author:";
            _comment.Font = new Font(Font, FontStyle.Bold);
            _comment.Dock = DockStyle.Fill;
			
            AutoScaleBaseSize = new Size(5, 13);
            ClientSize = new Size(width, height);
            ControlBox = false;
            Controls.AddRange(new Control[]
                                  {
                                      _comment,
                                      _editTextBox
                                  });
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "GridCellComment";
            ResumeLayout(false);
            CausesValidation = false;
            Visible = false;
            TopLevel = false;

            GraphicsPath myGraphicsPath = new
                GraphicsPath();

            _points = new[]
                             {
                                 new Point(0, _dy), new Point(0, height - _dy),
                                 new Point(_dx, height), new Point(width - _dx, height),
                                 new Point(width, height - _dy), new Point(width, _dy),
                                 new Point(width - _dx, 0), new Point(_dx, 0),
                                 new Point(0, _dy)
                             };
            myGraphicsPath.AddPolygon(_points);

            BackColor = SystemColors.Info;

            _dx = 10;
            _dy = 10;
        }


        public string CommentText
        {
            get { return _comment.Text; }
            set
            {
                _comment.Text = value;
                _editTextBox.Text = value;
            }
        }

        public Size CommentCorner
        {
            get { return new Size(_dx, _dy); }
            set
            {
                _dx = value.Width;
                _dy = value.Height;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }



        public void InitializeComment(int row, Point pt, GridExcelTipStyleProperties style)
        {
            int xPad = 15;
            int yPad = 5;
            int dy1 = 10;
            int dx1 = 10;

            if (row == 0)
            {
                dy1 = 0;
            }

            CommentCorner = new Size(dx1, dy1);

            Point panelPoint = new Point((int) .5*dy1, yPad);

            CommentText = style.ExcelTipText;

            pt.Offset(dx1, - dy1);
            Location = pt;

			SizeF sz = TextRenderer.MeasureText(_comment.CreateGraphics(), _comment.Text, _comment.Font, _comment.ClientSize, TextFormatFlags.WordBreak);

            int height = (int) (2*yPad + sz.Height); //10 is fudge
            int width = (int) (2*xPad + sz.Width);

            Size = new Size(Math.Max(width, _minCommentWidth), Math.Max(height, _minCommentHeight));

            _comment.Location = panelPoint;
            //_comment.Size = new Size(width - 2*panelPoint.X, height - 2*panelPoint.Y);
        }
    }
}