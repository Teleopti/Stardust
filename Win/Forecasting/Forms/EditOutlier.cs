using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public partial class EditOutlier : BaseRibbonForm
    {
        private IWorkload _workload;
        private IOutlier _outlier;
        private IOutlier _originalOutlier;
        private readonly string nameEmptyText = Resources.EnterNameHere;
        private readonly IList<IOutlier> _alreadyAddedOutliers;

        public EditOutlier(IEnumerable<IOutlier> outliers)
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColors();
            _alreadyAddedOutliers = new List<IOutlier>(outliers);
        }

        public IOutlier Outlier
        {
            get { return _outlier; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        public bool OutlierDateIsAvailable(out string outlierNameOnDate)
        {
            foreach (Outlier outlier in _alreadyAddedOutliers)
            {
                if (outlier.Equals(_outlier) || outlier.Equals(_originalOutlier)) continue;
                foreach (DateOnly dateTime in outlier.Dates)
                {
                    foreach (DateOnly date in _outlier.Dates)
                    {
                        if (date == dateTime)
                        {
                            _outlier.RemoveDate(date);
                            outlierNameOnDate = outlier.Description.Name;
                            return false;
                        }
                    }
                }
            }
            outlierNameOnDate = string.Empty;
            return true;
        }

        private void SetColors()
        {
            gradientPanel1.BackColor = ColorHelper.WizardPanelBackgroundColor();

            splitContainerAdvContent.BackColor = ColorHelper.WizardPanelSeparator();          
            splitContainerAdvContent.Panel1.BackColor = ColorHelper.WizardPanelBackgroundColor();
            splitContainerAdvContent.Panel2.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

        public void Initialize(IOutlier outlier)
        {
            _originalOutlier = outlier;
            _outlier = outlier;
            _workload = _outlier.Workload;
        }

        public void Initialize(IWorkload workload, IList<DateOnly> dates)
        {
            _originalOutlier = null;
            _workload = workload;
            _outlier = new Outlier(_workload, new Description(Resources.NewOutlier, Resources.NewOutlier));

        	dates.ForEach(d => _outlier.AddDate(d));
        }

        private void EditOutlier_Load(object sender, EventArgs e)
        {
            if (_outlier==null || _workload==null) throw new InvalidOperationException("The Initialize method must be used first!");

            listBoxDateProviders.Items.Clear();
            OutlierSpecificDatesSelection dateSelection = new OutlierSpecificDatesSelection(_outlier, this);

            string containingOutlierName = string.Empty;

            if (!OutlierDateIsAvailable(out containingOutlierName))
            {
                dateSelection.RefreshSelectedDates();
                MessageDateContainsOutlier(containingOutlierName);
            }

            PrepareDateListBox(dateSelection);
            if (_outlier.Dates.Count>0)
                dateSelection.SetCurrentDate(_outlier.Dates[0]);
        }

        private void PrepareDateListBox(OutlierSpecificDatesSelection dateSelection) {
            ListBoxItem<BaseUserControl> item = new ListBoxItem<BaseUserControl>(dateSelection, Resources.SpecificDatesSelection);
            listBoxDateProviders.Items.Add(item);
            listBoxDateProviders.SelectedIndex = 0;
            listBoxDateProviders.Enabled = false;
            textBoxTemplateName.Text = _outlier.Description.Name;
        }

        public void MessageDateContainsOutlier(string outlierName)
        {
            CultureInfo culture =
                  TeleoptiPrincipal.Current.Regional.UICulture;

            ShowInformationMessage(string.Format(culture, Resources.TheSpecialEventParameterContainsThisDate, outlierName),Resources.DateAlreadyExists);
        }

        private bool nameIsValid()
        {
            if (String.IsNullOrEmpty(textBoxTemplateName.Text.Trim()))
            {
                textBoxTemplateName.Text = nameEmptyText;
                textBoxTemplateName.SelectAll();
            }

            return textBoxTemplateName.Text != nameEmptyText;
        }

        private void textBoxTemplateName_TextChanged(object sender, EventArgs e)
        {
            buttonAdvOK.Enabled = nameIsValid();
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonAdvOK_Click(object sender, EventArgs e)
        {
            _outlier.Description = new Description(textBoxTemplateName.Text);
            DialogResult = DialogResult.OK;
            
            Close();
        }

        private void listBoxDateProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBoxItem<BaseUserControl> item = listBoxDateProviders.SelectedItem as ListBoxItem<BaseUserControl>;
            if (item == null) return;

            splitContainerAdvContent.Panel2.Controls.Clear();
            item.Value.Dock = DockStyle.Fill;
            splitContainerAdvContent.Panel2.Controls.Add(item.Value);
        }
    }
}
