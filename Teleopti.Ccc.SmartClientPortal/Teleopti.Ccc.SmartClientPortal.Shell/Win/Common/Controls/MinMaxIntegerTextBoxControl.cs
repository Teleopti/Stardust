using System.ComponentModel;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    /// <summary>
    /// Control with integer
    /// textboxes where min
    /// cannot exceed max
    /// </summary>
    public partial class MinMaxIntegerTextBoxControl : BaseUserControl
    {
        public MinMaxIntegerTextBoxControl()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// Returns ok if min is <= max
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                return integerTextBoxMinDays.IntegerValue <= integerTextBoxMaxDays.IntegerValue;
            }
        }

        [Browsable(true), Localizable(true)]
        public string LabelFromText
        {
            get { return labelFrom.Text; }
            set { labelFrom.Text = value; }
        }

        [Browsable(true), Localizable(true)]
        public string LabelToText
        {
            get { return labelTo.Text; }
            set { labelTo.Text = value; }
        }

        [Browsable(true), Localizable(true)]
        public string LabelMinDaysText
        {
            get { return labelMinDays.Text; }
            set { labelMinDays.Text = value; }
        }

        [Browsable(true), Localizable(true)]
        public string LabelMaxDaysText
        {
            get { return labelMaxDays.Text; }
            set { labelMaxDays.Text = value; }
        }

        [Browsable(true)]
        public int MinTextBoxValue
        {
            get { return (int)integerTextBoxMinDays.IntegerValue; }
            set { integerTextBoxMinDays.IntegerValue = value; }
        }

        [Browsable(true)]
        public int MaxTextBoxValue
        {
            get { return (int)integerTextBoxMaxDays.IntegerValue; }
            set { integerTextBoxMaxDays.IntegerValue = value; }
        }
        public override bool HasHelp
        {
            get{return false;}
        }
    }
}
