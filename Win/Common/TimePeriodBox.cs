using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    public partial class TimePeriodBox : UserControl
    {
        private TimePeriod timePeriod;

        public TimePeriodBox()
        {
            InitializeComponent();
        }

        private void TimePeriodBox_Resize(object sender, EventArgs e)
        {
            Height = textBoxTimePeriod.Height;
        }

        public TimePeriod TimePeriod
        {
            get { return timePeriod; }
            set
            {
                timePeriod = value;
                textBoxTimePeriod.Text = timePeriod.ToString();
            }
        }

        //private override string Text
        //{
        //    get { return textBoxTimePeriod.Text; }
        //    set { textBoxTimePeriod.Text = value; }
        //}

        private void textBoxTimePeriod_TextChanged(object sender, EventArgs e)
        {
            TimePeriod tp;
            if (TimePeriod.TryParse(textBoxTimePeriod.Text, out tp))
            {
                timePeriod = tp;
                textBoxTimePeriod.ForeColor = Color.Empty;
            }
            else
            {
                textBoxTimePeriod.ForeColor = Color.Red;
            }
        }

        private void textBoxTimePeriod_Validated(object sender, EventArgs e)
        {
            TimePeriod tp;
            if (TimePeriod.TryParse(textBoxTimePeriod.Text, out tp))
            {
                timePeriod = tp;
                textBoxTimePeriod.ForeColor = Color.Empty;
                textBoxTimePeriod.Text = tp.ToString();
            }
        }


    }
}
