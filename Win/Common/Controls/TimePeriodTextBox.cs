using System;
using System.Drawing;
using System.Windows.Forms;
using TimePeriod=Teleopti.Interfaces.Domain.TimePeriod;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public class TimePeriodTextBox : TextBox
    {
        public TimePeriodTextBox()
        {
            AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
        }

        public TimePeriod TimePeriod
        {
            get
            {
                TimePeriod tp;
                if (!TimePeriod.TryParse(Text, out tp))
                {
                    tp = new TimePeriod();
                }
                return tp;
            }
            set
            {
                Text = value.ToString();
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            ForeColor = Color.Empty;
            TimePeriod tp;
            if (TimePeriod.TryParse(Text, out tp))
            {
                string suggestion = Text + " (" + tp + ")";
                AutoCompleteStringCollection acsc = new AutoCompleteStringCollection();
                acsc.Add(suggestion);
                AutoCompleteCustomSource = acsc;
            }

        }

        protected override void OnValidated(EventArgs e)
        {
            base.OnValidated(e);
            TimePeriod tp;
            string currentText = Text;
            if (currentText.Contains("("))
            {
                int startSub = currentText.IndexOf("(", StringComparison.OrdinalIgnoreCase) + 1;
                int lengthOfSub = currentText.IndexOf(")", StringComparison.OrdinalIgnoreCase) - startSub;
                currentText = currentText.Substring(startSub, lengthOfSub);
            }
            if (TimePeriod.TryParse(currentText, out tp))
            {
                //base.TextChanged
                Text = tp.ToString();
                ForeColor = Color.Empty;
            }
            else
            {
                ForeColor = Color.Red;
            }
        }
    }
}
