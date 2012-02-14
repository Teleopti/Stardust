using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
	[Serializable]
	public class NullableNumericCellModel : GridTextBoxCellModel, INumericCellModelWithDecimals
	{
		private int _numberOfDecimals;
		private GridHorizontalAlignment _horizontalAlignement = GridHorizontalAlignment.Right;
		private double _minValue = double.MinValue;
		private double _maxValue = double.MaxValue;

		protected NullableNumericCellModel(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public NullableNumericCellModel(GridModel grid)
			: base(grid)
		{
		}

		public int NumberOfDecimals
		{
			get { return _numberOfDecimals; }
			set
			{
				if (value < 0)
				{
					_numberOfDecimals = 0;
				}
				else
				{
					_numberOfDecimals = value;
				}
			}
		}

		public GridHorizontalAlignment HorizontalAlignment
		{
			get { return _horizontalAlignement; }
			set { _horizontalAlignement = value; }
		}

		public double MinValue
		{
			get { return _minValue; }
			set { _minValue = value; }
		}

		public double MaxValue
		{
			get { return _maxValue; }
			set { _maxValue = value; }
		}

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			return new NullableNumericCellRenderer(control, this);
		}

		public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
		{
			if (string.IsNullOrEmpty(text))
			{
				style.CellValue = null;
				return true;
			}
			// Make sure value in cell can be coverted to a double
			double d;
			if (!double.TryParse(text, out d))
				return false;
			if(double.IsNegativeInfinity(d))
			{
				style.CellValue = null;
				return true;
			}
			if (d < _minValue || d > _maxValue)
				return false;

			style.HorizontalAlignment = _horizontalAlignement;
			style.CellValue = d;
			return true;
		}

		public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
		{
			CultureInfo ci = style.GetCulture(true);
			if (value == null)
				return "";
			double d;
			if (!double.TryParse(value.ToString(), out d))
				return "";

			var nfi = (NumberFormatInfo)ci.NumberFormat.Clone();
			nfi.NumberDecimalDigits = NumberOfDecimals;

			if (textInfo == GridCellBaseTextInfo.CopyText)
			{
				return GetFormattedTextForCopyPasteExcel(nfi, d);
			}
			return ((decimal)d).ToString("N", nfi);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static string GetFormattedTextForCopyPasteExcel(NumberFormatInfo numberFormatInfo, double value)
		{
			string fixedText = ((decimal)value).ToString("N", numberFormatInfo).Replace((char)160, (char)32);
			//fixedText = fixedText.Replace(" ", "");
			return fixedText;
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{

			if (info == null)
				throw new ArgumentNullException("info");

			info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
			base.GetObjectData(info, context);
		}
	}

	public class NullableNumericCellRenderer : GridTextBoxCellRenderer
	{
		public NullableNumericCellRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
			: base(grid, cellModel)
		{
		}

		public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
		{
			e.Style.HorizontalAlignment = ((NullableNumericCellModel)Model).HorizontalAlignment;//GridHorizontalAlignment.Right;
			e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
			e.Style.WrapText = false;
		}
	}
}