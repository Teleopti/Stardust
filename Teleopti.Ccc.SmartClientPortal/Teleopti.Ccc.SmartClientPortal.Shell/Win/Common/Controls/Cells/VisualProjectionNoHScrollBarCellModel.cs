using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
	[Serializable]
	public class VisualProjectionNoHScrollBarCellModel : GridStaticCellModel
	{

		public VisualProjectionNoHScrollBarCellModel(GridModel grid, TimeZoneInfo timeZoneInfo)
			: base(grid)
		{
			TimeZoneInfo = timeZoneInfo;
		}

		protected VisualProjectionNoHScrollBarCellModel(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public TimeZoneInfo TimeZoneInfo { get; set; }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{

			if (info == null)
				throw new ArgumentNullException("info");

			info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
			base.GetObjectData(info, context);
		}

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			return new VisualProjectionNoHScrollBarCellRenderer(control, this);
		}


		public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
		{
			return false;
		}

		public override bool ApplyText(GridStyleInfo style, string text)
		{
			return ApplyFormattedText(style, text, -1);
		}

		public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
		{
			return string.Empty;
		}

	}

	public class VisualProjectionNoHScrollBarCellRenderer : GridStaticCellRenderer
	{
		readonly IDictionary<int, IList<ToolTipData>> _toolTipLists = new Dictionary<int, IList<ToolTipData>>();
		private readonly ToolTip _toolTip = new ToolTip();
		private int _lastRow;

		public VisualProjectionNoHScrollBarCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
			: base(grid, cellModel)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		protected override void OnMouseHover(int rowIndex, int colIndex, MouseEventArgs e)
		{
			if (_lastRow != rowIndex)
			{
				_toolTip.SetToolTip(Grid, "");
				_toolTip.Hide(Grid);
				_lastRow = rowIndex;
			}

			var theToolTip = GetToolTip(rowIndex, e.X);
			if (theToolTip != _toolTip.GetToolTip(Grid))
			{
				_toolTip.Hide(Grid);
				if (!String.IsNullOrEmpty(theToolTip))
					_toolTip.Show(theToolTip, Grid);
			}
			base.OnMouseMove(rowIndex, colIndex, e);
		}

		private string GetToolTip(int rowIndex, float x)
		{
			string ret = "";
			IList<ToolTipData> tipDatas;
			if (_toolTipLists.TryGetValue(rowIndex, out tipDatas))
				foreach (var tipData in tipDatas)
				{
					if (tipData.FromX <= x && tipData.ToX >= x)
						ret = tipData.TheToolTip;
				}

			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
		{
			if (style.Tag == null) return;
			var timePeriod = (DateTimePeriod)style.Tag;
			var pixelConverter = new LengthToTimeCalculator(new DateTimePeriod(timePeriod.StartDateTime.AddHours(-1), timePeriod.EndDateTime.AddHours(1)), clientRectangle.Width);

			var cellModel = style.CellModel as VisualProjectionNoHScrollBarCellModel;
			TimeZoneInfo timeZoneInfo = (cellModel != null && cellModel.TimeZoneInfo != null)
				? cellModel.TimeZoneInfo
				: TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;

			foreach (var interval in timePeriod.AffectedHourCollection())
			{
				var position = pixelConverter.PositionFromDateTime(interval.StartDateTime, Grid.IsRightToLeft());
				var x = (int)Math.Round(position, 0);
				if (x > 0)
					g.DrawLine(new Pen(Color.LightGray, 1), clientRectangle.Left + x, clientRectangle.Top,
							   clientRectangle.Left + x, clientRectangle.Bottom);
			}

			if (_toolTipLists.ContainsKey(rowIndex))
				_toolTipLists.Remove(rowIndex);

			IList<ToolTipData> tipDatas = new List<ToolTipData>();
			var projectionCellValue = (ProjectionCellValue) style.CellValue;

			foreach (var layer in projectionCellValue.VisualLayers)
			{
				var rect = GetLayerRectangle(pixelConverter, layer.Period, clientRectangle);
				if (!rect.IsEmpty)
				{
					var rect2 = rect;
					rect2.Inflate(1, 1);
					using (Brush brush = new LinearGradientBrush(rect2, Color.WhiteSmoke, layer.Payload.ConfidentialDisplayColor(projectionCellValue.AssignedPerson), 90, false))
					{
						g.FillRectangle(brush, rect);
						var tipData = new ToolTipData(rect.X, rect.X + rect.Width, layer.Payload.ConfidentialDescription(projectionCellValue.AssignedPerson) + "  " + layer.Period.TimePeriod(timeZoneInfo).ToShortTimeString());
						tipDatas.Add(tipData);
					}
				}
			}
			_toolTipLists.Add(rowIndex, tipDatas);
		}

		private RectangleF GetLayerRectangle(LengthToTimeCalculator pixelConverter, DateTimePeriod period, RectangleF clientRect)
		{
			var x1 = pixelConverter.PositionFromDateTime(period.StartDateTime, Grid.IsRightToLeft());
			var x2 = pixelConverter.PositionFromDateTime(period.EndDateTime, Grid.IsRightToLeft());

			if (Grid.IsRightToLeft())
			{
				var tmp = x1;
				x1 = x2;
				x2 = tmp;
			}

			if (x2 - x1 < 1)
				return RectangleF.Empty;
			return new RectangleF((float)(clientRect.Left + x1), clientRect.Top, (float)(x2 - x1), clientRect.Height);
		}
	}

	public class ToolTipData
	{
		private readonly float _fromX;
		private readonly float _toX;
		private readonly string _theToolTip;

		public ToolTipData(float fromX, float toX, string theToolTip)
		{
			_fromX = fromX;
			_toX = toX;
			_theToolTip = theToolTip;
		}

		public float FromX { get { return _fromX; } }
		public float ToX { get { return _toX; } }
		public string TheToolTip { get { return _theToolTip; } }
	}
}
