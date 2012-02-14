using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls
{
    /// <summary>
    /// Textbox that shows percent values
    /// from -100% to 100%
    /// </summary>
    public class TeleoptiPercentTextBox: TextBox
    {
        private readonly Color _posColor = Color.Black;
        private readonly Color _negColor = Color.Red;
        private string _percentSign = "%";
        private string _negativeSign = "-";
        private double _doubleValue;
        private CultureInfo _cultureInfo;
    	private double _minimum = -100;
    	private double _maximum = 100d;
    	private double _defaultValue;

        public void Setup(CultureInfo cultureInfo)
        {
            TextAlign = HorizontalAlignment.Left;
            _cultureInfo = cultureInfo;
            _percentSign = cultureInfo.NumberFormat.PercentSymbol;
            _negativeSign = cultureInfo.NumberFormat.NegativeSign;
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!((Char.IsNumber(e.KeyChar)) || e.KeyChar == '\b' || e.KeyChar == '-'))
            {
                e.Handled = true;
            }
        }
        protected override void OnLeave(EventArgs e)
        {
            string tempText = ParseText();
            int correctIntValue = PercentValue(tempText);
            if (!AllowNegativePercentage)
                ForeColor = correctIntValue >= 0 ? _posColor : _negColor;
            Text = tempText + _percentSign;
        }

        /// <summary>
        /// Removes starting zeros and
        /// missplaced negative signs
        /// </summary>
        /// <returns></returns>
        private string ParseText()
        {
            string tempText = Text.Replace(_percentSign, "");
            tempText = tempText.Replace(" ", "");
            if (string.IsNullOrEmpty(tempText))
                tempText = (_doubleValue * 100).ToString(_cultureInfo);

            if (tempText.Contains(_negativeSign))
            {
                bool hasNegSignFirst = tempText.StartsWith(_negativeSign,true, _cultureInfo);
                string temp = string.Empty;
                if (hasNegSignFirst)
                    temp = _negativeSign;
                tempText = temp + tempText.Replace(_negativeSign, string.Empty).TrimStart('0');
            }
            return tempText;
        }

        public int PercentValue(string text)
        {
            if (text.Length > 0)
            {
                int value;
                if (int.TryParse(text, out value))
                    return value;
            }
            return (int)_doubleValue*100;
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                int intValue;
                string strippedText = value.Replace(_percentSign, string.Empty);
				if ((int.TryParse(strippedText, out intValue) && (intValue <= Maximum && intValue >= Minimum) || string.IsNullOrEmpty(strippedText)))
                {
                    base.Text = value.Length > 2 ? value.TrimStart('0') : value;
                    if (intValue < 0)
                        _doubleValue = Math.Abs(intValue * 0.01) * -1;
                    else
                        _doubleValue = intValue * 0.01;
                }
                else
                {
                    ForeColor = _posColor;
					base.Text = _defaultValue + _percentSign;
                    _doubleValue = _defaultValue;
                }
            }
        }

    	public double DefaultValue	
    	{
			get { return _defaultValue; }
			set { _defaultValue = value; }
    	}

    	public double DoubleValue
        {
            get { return _doubleValue; }
            set
            {
                if (!AllowNegativePercentage)
                    ForeColor = value >= 0 ? _posColor : _negColor;
                Text = (value * 100) + _percentSign;
                _doubleValue = value;
            }
        }

        public bool AllowNegativePercentage { get; set; }

        public double Minimum
    	{
    		get { return _minimum; }
			set { _minimum = value; }
    	}
		
		public double Maximum
    	{
    		get { return _maximum; }
			set { _maximum = value; }
    	}
    }
}
