using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class FindMatching : Form
	{
		private readonly IPerson _sourcePerson;
		private readonly DateOnly _dateOnly;
		private IAvailableHourlyEmployeeFinder _finder;

		public FindMatching()
		{
			InitializeComponent();
			//if (!DesignMode)
			//    SetTexts();
		}

		public FindMatching(IPerson sourcePerson, DateOnly dateOnly, ISchedulingResultStateHolder stateHolder, ICollection<IPerson> filteredPersons) : this()
		{
			_sourcePerson = sourcePerson;
			_dateOnly = dateOnly;
            _finder = new AvailableHourlyEmployeeFinder(sourcePerson, dateOnly, stateHolder, filteredPersons);
		}

		public IPerson Selected()
		{
			if (listViewResult.SelectedItems.Count == 0)
				return null;

			return (IPerson) listViewResult.SelectedItems[0].Tag;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxFind"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void FindMatching_Load(object sender, EventArgs e)
		{
			labelInfo.Text = "xxFind replacement candidates for " + _sourcePerson.Name + " at " + _dateOnly.ToShortDateString(TeleoptiPrincipal.Current.Regional.Culture);
			IList<AvailableHourlyEmployeeFinderResult> result = _finder.Find();
			foreach (AvailableHourlyEmployeeFinderResult item in result)
			{
				if(item.Matching)
					addItem(item);
			}
			foreach (AvailableHourlyEmployeeFinderResult item in result)
			{
				if (!item.Matching)
					addItem(item);
			}

			if (listViewResult.Items.Count > 0)
				listViewResult.Items[0].Selected = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.ListViewItem+ListViewSubItemCollection.Add(System.String)")]
		private void addItem(AvailableHourlyEmployeeFinderResult item)
		{
			foreach (IStudentAvailabilityRestriction restriction in item.StudentAvailabilityDay.RestrictionCollection)
			{
				ListViewItem newItem = new ListViewItem(item.Person.Name.ToString());
				newItem.Tag = item.Person;
				if (item.Matching)
				{
					newItem.SubItems.Add("X");
				}
				else
				{
					newItem.SubItems.Add("");
				}

				newItem.SubItems.Add(restriction.StartTimeLimitation.StartTimeString);
				newItem.SubItems.Add(restriction.EndTimeLimitation.EndTimeString);
				newItem.SubItems.Add(restriction.WorkTimeLimitation.StartTimeString);
				newItem.SubItems.Add(restriction.WorkTimeLimitation.EndTimeString);

				listViewResult.Items.Add(newItem);
			}
		}

		private void buttonAssign_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
