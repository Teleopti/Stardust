using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
	public partial class OutlierSpecificDatesSelection : BaseUserControl
	{
		private readonly IOutlier _outlier;
		private readonly EditOutlier _editOutlier;
		public OutlierSpecificDatesSelection()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			dateSelectionControl1.ShowTabArea = false;
			dateSelectionControl1.SelectTab(2);
		}


		public OutlierSpecificDatesSelection(IOutlier outlier, EditOutlier editOutlier) : this()
		{
			_outlier = outlier;
			_editOutlier = editOutlier;
			RefreshSelectedDates();
		}

		private void buttonAdvAddClick(object sender, EventArgs e)
		{
			foreach (DateOnlyPeriod datePair in dateSelectionControl1.GetCurrentlySelectedDates())
			{
				DateOnly currentDate = datePair.StartDate;
				while (currentDate <= datePair.EndDate)
				{
					_outlier.AddDate(currentDate);

					string containingOutlierName;
					if (!_editOutlier.OutlierDateIsAvailable(out containingOutlierName))
					{
						_outlier.RemoveDate(currentDate);
						_editOutlier.MessageDateContainsOutlier(containingOutlierName);
					}
					currentDate = currentDate.AddDays(1);
				}
			}
			RefreshSelectedDates();
		}
		
		public void SetCurrentDate(DateOnly currentDate)
		{
			dateSelectionControl1.SetInitialDates(new DateOnlyPeriod(currentDate,currentDate));
		}

		public void RefreshSelectedDates()
		{
			listBoxSelectedDates.DataSource = null;
			listBoxSelectedDates.FormatString = "d";
			listBoxSelectedDates.DataSource = _outlier.Dates.OrderBy(d => d).Select(d => new TupleItem(d.ToShortDateString(),d)).ToList();
		}

		private void buttonAdvRemoveClick(object sender, EventArgs e)
		{
			if (listBoxSelectedDates.SelectedItem == null) return;
			_outlier.RemoveDate((DateOnly)((TupleItem)listBoxSelectedDates.SelectedItem).ValueMember);
			RefreshSelectedDates();
		}


	}
}
