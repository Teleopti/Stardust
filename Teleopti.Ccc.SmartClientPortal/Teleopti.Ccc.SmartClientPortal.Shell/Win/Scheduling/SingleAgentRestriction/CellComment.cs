using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction
{
    public partial class CellComment : Form
    {
        internal Label _comment;
		internal RichTextBox _editTextBox;
		private int _dx = 10;
		private int _dy = 10;


		public string CommentText 
		{
            get { return _comment.Text; }
            set
            {
                _comment.Text = value;
			_editTextBox.Text = value;}
		}

        public Size CommentCorner
		{
			get{return new Size(_dx, _dy);}
			set{_dx = value.Width; _dy = value.Height;}
		}
		
		private int minCommentWidth = 80;
		private int minCommentHeight = 18;

		public CellComment()
		{
			int width = minCommentWidth;
			int height = minCommentHeight;

            _comment = new Label();
			_editTextBox = new RichTextBox();
			_editTextBox.Visible = false;
			_editTextBox.Multiline = true;
			_editTextBox.BorderStyle = BorderStyle.None;

			SuspendLayout();
			// 
			// comment
			// 
            _comment.Anchor = (((AnchorStyles.Top | AnchorStyles.Bottom) 
				| AnchorStyles.Left) 
				| AnchorStyles.Right);
            _comment.Location = new Point(_dx, _dy);
            _comment.Name = "comment";
            _comment.Size = new Size(width - 2 * _dx, height - 2 * _dy);
            _comment.TabIndex = 0;
            _comment.Text = "Author:";
            _comment.Font = new Font(Font, FontStyle.Regular);
            _comment.Dock = DockStyle.Fill;
			// 
			// GridCellComment
			// 
			AutoScaleBaseSize = new Size(5, 13);
			ClientSize = new Size(width, height);
			ControlBox = false;
			Controls.AddRange(new Control[] {_comment, _editTextBox});
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Name = "GridCellComment";
			ResumeLayout(false);
			CausesValidation = false;
			Visible = false;
			TopLevel = false;

            Point[] points = new[]{new Point(0, _dy), new Point(0, height - _dy), 
											new Point(_dx, height), new Point(width - _dx, height),
											new Point(width, height - _dy), new Point(width , _dy),
											new Point(width - _dx, 0), new Point(_dx , 0),
											new Point(0, _dy)};
			using (System.Drawing.Drawing2D.GraphicsPath myGraphicsPath  = new
				System.Drawing.Drawing2D.GraphicsPath())
			{
                myGraphicsPath.AddPolygon(points);  
			}
			


			BackColor = SystemColors.Info;
			_dx = 10;
			_dy = 10;
		}

		public void InitializeComment(int row, Point pt, GridExcelTipStyleProperties style)
		{
			int xPad = 15;
			int yPad = 5;
			int dy1 = 10;
			int dx1 = 10;

			if(row == 0)
			{
				dy1 = 0;
			}

			CommentCorner = new Size(dx1, dy1);

			Point panelPoint = new Point((int) .5 * dy1, yPad);

			CommentText = style.ExcelTipText;

			pt.Offset(dx1, - dy1);
			Location = pt;

			Graphics g = Graphics.FromHwnd(Handle);
			SizeF sz = g.MeasureString(CommentText, Font);
			int height = (int)(2 * yPad + sz.Height ); //10 is fudge
			int width = (int)(2 * xPad  + sz.Width );

			Size = new Size(Math.Max(width, minCommentWidth), Math.Max(height, minCommentHeight));

			_comment.Location = panelPoint;
			_comment.Size = new Size(width - 2 * panelPoint.X, height - 2 * panelPoint.Y);
		}
    }
}
