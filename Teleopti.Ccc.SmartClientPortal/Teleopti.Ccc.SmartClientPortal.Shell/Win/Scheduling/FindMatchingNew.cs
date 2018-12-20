using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class FindMatchingNew : BaseRibbonForm
	{
		private readonly IPerson _sourcePerson;
		private readonly DateOnly _dateOnly;
		private readonly IAvailableHourlyEmployeeFinder _finder;

		public FindMatchingNew()
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
		}

		public FindMatchingNew(IRestrictionExtractor restrictionExtractor, IPerson sourcePerson, DateOnly dateOnly, IScheduleDayForPerson scheduleDayForPerson, ICollection<IPerson> filteredPersons) : this()
		{
			_sourcePerson = sourcePerson;
			_dateOnly = dateOnly;
            _finder = new AvailableHourlyEmployeeFinder(restrictionExtractor, sourcePerson, dateOnly, scheduleDayForPerson, filteredPersons, UserTimeZone.Make());
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
			Text = "xxFind replacement candidates for " + _sourcePerson.Name + " at " + _dateOnly.ToShortDateString(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture);
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

		private void addItem(AvailableHourlyEmployeeFinderResult item)
		{
			ListViewItem newItem = new ListViewItem(item.Person.Name.ToString());
			newItem.Tag = item.Person;
			newItem.SubItems.Add(item.WorkTimesYesterday);
			newItem.SubItems.Add(item.Availability);
			
			if (item.Matching)
			{
				newItem.SubItems.Add(Resources.Yes);
			}
			else
			{
				newItem.SubItems.Add(Resources.No);
			}

			newItem.SubItems.Add(item.WorkTimesTomorrow);

			if (item.NightRestOk)
			{
				newItem.SubItems.Add(Resources.Yes);
			}
			else
			{
				newItem.SubItems.Add(Resources.No);
			}

			if (item.NightRestOk)
			{
				newItem.ImageIndex = item.Matching ? 0 : 1;
			}
			else
			{
				newItem.ImageIndex = 2;
			}

			listViewResult.Items.Add(newItem);

		}

		private void buttonAssign_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
