using System;
using System.Globalization;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
	public class NumericTextBox : TextBox
	{
		public NumericTextBox()
		{
			CurrentCulture = CultureInfo.CurrentCulture;
			MaxValue = double.MaxValue;
			MinValue = double.MinValue;
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			var numberFormatInfo = CurrentCulture.NumberFormat;
			var decimalSeparator = numberFormatInfo.NumberDecimalSeparator;

			var keyInput = e.KeyChar.ToString();

			if (!(Char.IsDigit(e.KeyChar) || e.KeyChar == '\b' || keyInput.Equals(decimalSeparator)))
			{
				e.Handled = true;
			}
		}

		public bool IsValid()
		{
			double result;
			if (Double.TryParse(Text, NumberStyles.Any, CurrentCulture, out result))
			{
				if (result <= MaxValue && result >= MinValue)
					return true;
			}
			return false;
		}

		public double DoubleValue
		{
			get
			{
				double result;
				Double.TryParse(Text, NumberStyles.Any, CurrentCulture, out result);
				return result;
			}
			set
			{
				Text = value.ToString(CurrentCulture);
			}
		}

		public double MinValue { get; set; }
		public double MaxValue { get; set; }

		public CultureInfo CurrentCulture { get; set; }
	}
}
