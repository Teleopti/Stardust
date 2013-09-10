using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.Time;

namespace Teleopti.Ccc.Win.Common.Controls
{
    [Browsable(true), Category("Teleopti Controls")]
    public partial class TimeDurationPickerView : Syncfusion.Windows.Forms.Tools.ComboBoxAdv, WinCode.Common.Time.ITimeDurationPickerView
    {
        private TimeDurationPickerPresenter _presenter;

        public TimeDurationPickerView()
        {
            _presenter = new TimeDurationPickerPresenter(this);
            InitializeComponent();
        }

        public TimeDurationPickerView(IContainer container)
        {
            _presenter = new TimeDurationPickerPresenter(this);
            container.Add(this);
            InitializeComponent();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (!string.IsNullOrEmpty(SelectedText))
            {
                _presenter.Interval = TimeSpan.Parse(SelectedText);
            }
        }

        [Browsable(true), Category("Custom settings"), DefaultValue("00:30")]
        public TimeSpan Interval
        {
            get { return _presenter.Interval; }
            set { _presenter.Interval = value; }
        }

        public TimeSpan Duration
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return TimeSpan.Zero;
                }
                return TimeSpan.Parse(Text);
            }
        }

        public void SetTimeList(IList<TimeSpanDataBoundItem> timeSpans)
        {
            DisplayMember = "FormattedText";
            DataSource = timeSpans;
            //foreach (TimeSpan timeSpan in timeSpans)
            //{
            //    Items.Add(timeSpan);
            //}
        }
    }
}
