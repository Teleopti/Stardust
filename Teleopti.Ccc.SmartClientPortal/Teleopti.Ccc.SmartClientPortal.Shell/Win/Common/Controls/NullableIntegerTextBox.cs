using System;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    /// <summary>
    /// Textbox that only accepts integers
    /// and null values. 
    /// Max value is 999 but can be changed.
    /// </summary>
    public class NullableIntegerTextBox:TextBox
    {
        public NullableIntegerTextBox()
        {
            Setup();
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!((Char.IsDigit(e.KeyChar)) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        private void Setup()
        {
            MaxLength = 3;
            TextAlign = HorizontalAlignment.Right;
        }

        public int? IntegerValue()
        {
            if (TextLength>0)
            {
                int value;
                if (int.TryParse(Text, out value))
                    return value;
            }
            return null;
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
                if ((int.TryParse(value, out intValue) && intValue < 1000) || string.IsNullOrEmpty(value))
                    base.Text = value;
            }
        }
    }
}
