using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Schedule;

namespace Teleopti.Ccc.Win.Meetings.Overview
{
    public partial class CalendarAndTextPanel : UserControl
    {
        public event EventHandler DateChanged;
        public CalendarAndTextPanel()
        {
            InitializeComponent();
            monthCalendarAdv1.Culture = CultureInfo.CurrentCulture;
        }

        void MonthCalendarAdv1DateSelected(object sender, EventArgs e)
        {
            if (monthCalendarAdv1.SelectedDates.Length > 0)
                monthCalendarAdv1.Value = monthCalendarAdv1.SelectedDates[0];
        }

        void MonthCalendarAdv1DateChanged(object sender, EventArgs e)
        {
            OnDateChanged(e);
        }

        public void OnDateChanged(EventArgs e)
        {
        	var handler = DateChanged;
            if(handler != null)
                handler(this, e);
        }

        public void SetText(string text)
        {
            textBox1.Text = text;
        }

        public DateTime SelectedDate
        {
            get { return  monthCalendarAdv1.Value; }
            set { monthCalendarAdv1.Value = value; }
        }

        public void SelectDateRange(DateSelections dateSelections)
        {
            var dateTimes = new List<DateTime>();
            if(dateSelections != null)
            {
                foreach (DateTime dateTime in dateSelections)
                {
                    dateTimes.Add(dateTime);
                }
            }
            else
            {
                dateTimes.Add(monthCalendarAdv1.Value);
            }
            
            monthCalendarAdv1.SelectedDates = dateTimes.ToArray();
        }

    }

   
}
