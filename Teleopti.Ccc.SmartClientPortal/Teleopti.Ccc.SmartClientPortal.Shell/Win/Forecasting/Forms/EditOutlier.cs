using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
	public partial class EditOutlier : BaseDialogForm
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

			_alreadyAddedOutliers = new List<IOutlier>(outliers);
		}

		public IOutlier Outlier
		{
			get { return _outlier; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
		public bool OutlierDateIsAvailable(out string outlierNameOnDate)
		{
			foreach (var outlier1 in _alreadyAddedOutliers)
			{
				var outlier = (Outlier) outlier1;
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

		private void editOutlierLoad(object sender, EventArgs e)
		{
			if (_outlier==null || _workload==null) throw new InvalidOperationException("The Configure method must be used first!");

			listBoxDateProviders.Items.Clear();
			var dateSelection = new OutlierSpecificDatesSelection(_outlier, this);

			string containingOutlierName;

			if (!OutlierDateIsAvailable(out containingOutlierName))
			{
				dateSelection.RefreshSelectedDates();
				MessageDateContainsOutlier(containingOutlierName);
			}

			prepareDateListBox(dateSelection);
			if (_outlier.Dates.Count>0)
				dateSelection.SetCurrentDate(_outlier.Dates[0]);
		}

		private void prepareDateListBox(OutlierSpecificDatesSelection dateSelection) {
			var item = new ListBoxItem<BaseUserControl>(dateSelection, Resources.SpecificDatesSelection);
			listBoxDateProviders.Items.Add(item);
			listBoxDateProviders.SelectedIndex = 0;
			listBoxDateProviders.Enabled = false;
			textBoxTemplateName.Text = _outlier.Description.Name;
		}

		public void MessageDateContainsOutlier(string outlierName)
		{
			CultureInfo culture =
				  TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture;

			ViewBase.ShowInformationMessage(string.Format(culture, Resources.TheSpecialEventParameterContainsThisDate, outlierName),Resources.DateAlreadyExists);
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

		private void textBoxTemplateNameTextChanged(object sender, EventArgs e)
		{
			buttonAdvOK.Enabled = nameIsValid();
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			_outlier.Description = new Description(textBoxTemplateName.Text);
			DialogResult = DialogResult.OK;
			
			Close();
		}

		private void listBoxDateProvidersSelectedIndexChanged(object sender, EventArgs e)
		{
			var item = listBoxDateProviders.SelectedItem as ListBoxItem<BaseUserControl>;
			if (item == null) return;

			splitContainerAdvContent.Panel2.Controls.Clear();
			item.Value.Dock = DockStyle.Fill;
			splitContainerAdvContent.Panel2.Controls.Add(item.Value);
		}

		private void editOutlierShown(object sender, EventArgs e)
		{
			Refresh();
		}

		private void buttonAdvOkMouseLeave(object sender, EventArgs e)
		{
			buttonAdvOK.Refresh();
		}

		private void buttonAdvCancelMouseLeave(object sender, EventArgs e)
		{
			buttonAdvCancel.Refresh();
		}
	}
}
