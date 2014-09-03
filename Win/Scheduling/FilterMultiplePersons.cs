﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class FilterMultiplePersons : BaseDialogForm
	{
		//private SearchPersonPresenter _presenter;
		private ArrayList _persons = new ArrayList();
		private List<IPerson> _searchablePersons;
		private ArrayList _userSelectedPersonList;
		private ArrayList _parsedPersonList;

		public FilterMultiplePersons()
		{
			InitializeComponent();
			if(!DesignMode )
				SetTexts();
			gridListControl1.DataSource = _persons;
			gridListControl1.MultiColumn = true;
			gridListControl1.Grid.MouseDoubleClick += gridMouseDoubleClick;
			gridListControl1.Grid.QueryCellInfo += Grid_QueryCellInfo;
			gridListControl1.Grid.KeyDown += gridKeyDown;
			gridListControl1.BorderStyle = BorderStyle.None;

			_userSelectedPersonList = new ArrayList();
			gridListControlSelectedItems.DataSource = _userSelectedPersonList;
			gridListControlSelectedItems.MultiColumn = true;
			gridListControlSelectedItems.Grid.QueryCellInfo += Grid_QueryCellInfo;
			gridListControlSelectedItems.BorderStyle = BorderStyle.None;

			_parsedPersonList = new ArrayList();
			gridListControlCustomSearch.DataSource = _parsedPersonList;
			gridListControlCustomSearch.MultiColumn = true;
			gridListControlCustomSearch.Grid.QueryCellInfo += Grid_QueryCellInfo;
			gridListControlCustomSearch.BorderStyle = BorderStyle.None;

			
		}

		#region "Grid related functions"
		private void gridKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if (gridListControl1.SelectedItem == null) return;
				var person = (IPerson)gridListControl1.SelectedValue;

				if (person != null)
				{
					addPersonInFinalGrid(person);
					var handler = ItemDoubleClick;
					if (handler != null)
					{
						handler(this, EventArgs.Empty);
					}
				}
			}
		}

		private void Grid_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.RowIndex <= 0 && e.ColIndex == 1)
			{
				e.Style.Text = UserTexts.Resources.Name;
				e.Handled = true;
			}

			if (e.RowIndex <= 0 && e.ColIndex == 2)
			{
				e.Style.Text = UserTexts.Resources.EmployeeNumber;
				e.Handled = true;
			}

			if (e.RowIndex <= 0 && e.ColIndex == 3)
			{
				e.Style.Text = UserTexts.Resources.ApplicationLogon;
				e.Handled = true;
			}
		}

		private void gridMouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (gridListControl1.SelectedItem == null) return;
			var person = (IPerson)gridListControl1.SelectedValue;

			if (person != null)
			{
				addPersonInFinalGrid(person);
				var handler = ItemDoubleClick;
				if (handler != null)
					handler(this, EventArgs.Empty);
			}
		}

		public event EventHandler<EventArgs> ItemDoubleClick;
		#endregion

		public IPerson SelectedPerson()
		{
			if (gridListControl1.SelectedItem != null)
				return (IPerson) gridListControl1.SelectedValue;

			return null;
		}

		public void SetSearchablePersons(IEnumerable<IPerson> searchablePersons)
		{
			_searchablePersons = searchablePersons.ToList();
			textBox1.Select();
		}

		private void textBox1TextChanged(object sender, EventArgs e)
		{
			FillGridListControl();
		}

		public ICollection<IPerson> Search(string searchText)
		{
			CultureInfo cultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
			string lowerSearchText = searchText.ToLower(cultureInfo);
			ICollection<IPerson> personQuery =
					(from
						person in _searchablePersons 
					 where
						 person.Name.ToString(NameOrderOption.LastNameFirstName).ToLower(cultureInfo).Contains(lowerSearchText) ||
						 person.Name.ToString(NameOrderOption.LastNameFirstName).ToLower(cultureInfo).Replace(",", "").Contains(lowerSearchText) ||
						 person.Name.ToString(NameOrderOption.FirstNameLastName).ToLower(cultureInfo).Contains(lowerSearchText) ||
						 person.EmploymentNumber.ToLower(cultureInfo).Contains(lowerSearchText) ||
						 (
							(person.ApplicationAuthenticationInfo != null) &&
							person.ApplicationAuthenticationInfo.ApplicationLogOnName.ToLower(cultureInfo).Contains(lowerSearchText)
						 )
					 select person).ToList();

			return personQuery;

		}

		public void FillGridListControl()
		{
			IEnumerable<IPerson> found = Search(textBox1.Text);

			gridListControl1.BeginUpdate();
			_persons.Clear();

			if (!string.IsNullOrEmpty(textBox1.Text))
			{
				foreach (IPerson person in found)
				{
					_persons.Add(new FilterMultiplePersonGridControlItem(person));
				}

				gridListControl1.DataSource = _persons;
				//Ola: in some installations you get an error here if the list is empty
				//on my machine it just jumps out with no error. I think this will fix bug 11978
				// it only happens when the FIRST search returns an empty list
				if (_persons.Count > 0)
					gridListControl1.ValueMember = "Person";

				gridListControl1.MultiColumn = true;

				gridListControl1.Grid.ColHiddenEntries.Add(new GridColHidden(0));
				gridListControl1.Grid.ColHiddenEntries.Add(new GridColHidden(4));

				if (_persons.Count > 0)
					gridListControl1.SetSelected(0, true);

			}

			gridListControl1.EndUpdate();

		}

		private void fillDuplicatesItemsGrid()
		{
			gridListControlCustomSearch.BeginUpdate();
			gridListControlCustomSearch.DataSource = _parsedPersonList;
			if (_parsedPersonList.Count > 0)
					gridListControlCustomSearch.ValueMember = "Person";
			gridListControlCustomSearch.MultiColumn = true;
			gridListControlCustomSearch.Grid.ColHiddenEntries.Add(new GridColHidden(0));
			gridListControlCustomSearch.Grid.ColHiddenEntries.Add(new GridColHidden(4));
			if (_parsedPersonList.Count > 0)
					gridListControlCustomSearch.SetSelected(0, true);
			gridListControlCustomSearch.EndUpdate();
		}

		private void addPersonInFinalGrid(IPerson person)
		{
			_searchablePersons.Remove(person);
			FillGridListControl();
			_userSelectedPersonList.Add(new FilterMultiplePersonGridControlItem(person));
			gridListControlSelectedItems.BeginUpdate();
			gridListControlSelectedItems.DataSource = _userSelectedPersonList;
			if (_userSelectedPersonList.Count > 0)
				gridListControlSelectedItems.ValueMember = "Person";

			gridListControlSelectedItems.MultiColumn = true;

			gridListControlSelectedItems.Grid.ColHiddenEntries.Add(new GridColHidden(0));
			gridListControlSelectedItems.Grid.ColHiddenEntries.Add(new GridColHidden(4));

			if (_userSelectedPersonList.Count > 0)
				gridListControlSelectedItems.SetSelected(0, true);
			gridListControlSelectedItems.EndUpdate();

		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			var person = (IPerson)gridListControl1.SelectedValue;
			addPersonInFinalGrid(person );
		}

		private void buttonAdvParse_Click(object sender, EventArgs e)
		{
			var inputText = textBoxCustomSearch.Text;
			var inputTextArray = inputText.Split(',');
			CultureInfo cultureInfo = TeleoptiPrincipal.Current.Regional.Culture;

			_parsedPersonList.Clear();
			var actualInput = new List<String>();
			actualInput.AddRange(inputTextArray);
			foreach (var expected in inputTextArray)
			{
				string lowerSearchText = expected.ToLower(cultureInfo);
				ICollection<IPerson> personQuery =
					(from
						person in _searchablePersons 
					 where
						 person.Name.ToString(NameOrderOption.LastNameFirstName).ToLower(cultureInfo).Contains(lowerSearchText) ||
						 person.Name.ToString(NameOrderOption.LastNameFirstName).ToLower(cultureInfo).Replace(",", "").Contains(lowerSearchText) ||
						 person.Name.ToString(NameOrderOption.FirstNameLastName).ToLower(cultureInfo).Contains(lowerSearchText) ||
						 person.EmploymentNumber.ToLower(cultureInfo).Contains(lowerSearchText) ||
						 (
							(person.ApplicationAuthenticationInfo != null) &&
							person.ApplicationAuthenticationInfo.ApplicationLogOnName.ToLower(cultureInfo).Contains(lowerSearchText)
						 )
					 select person).ToList();
				if (personQuery.Count == 1)
				{
					_userSelectedPersonList.Add(new FilterMultiplePersonGridControlItem(personQuery.First()));
					actualInput.Remove(expected);
				}
				else if (personQuery.Count > 1)
				{
					foreach (var person in personQuery)
					{
						var tempPerson = new FilterMultiplePersonGridControlItem(person);
						if (!_userSelectedPersonList.Contains(tempPerson ))
							_parsedPersonList.Add(tempPerson );
					}
					actualInput.Remove(expected);
				}
				
			}
			textBoxCustomSearch.Text = String.Join(",", actualInput);

			fillDuplicatesItemsGrid();

			gridListControlSelectedItems.BeginUpdate();
			gridListControlSelectedItems.DataSource = _userSelectedPersonList;
			if (_userSelectedPersonList.Count > 0)
				gridListControlSelectedItems.ValueMember = "Person";

			gridListControlSelectedItems.MultiColumn = true;

			gridListControlSelectedItems.Grid.ColHiddenEntries.Add(new GridColHidden(0));
			gridListControlSelectedItems.Grid.ColHiddenEntries.Add(new GridColHidden(4));

			if (_userSelectedPersonList.Count > 0)
				gridListControlSelectedItems.SetSelected(0, true);
			gridListControlSelectedItems.EndUpdate();
		}
	}


	internal class FilterMultiplePersonGridControlItem
	{
		private IPerson _person;

		public FilterMultiplePersonGridControlItem(IPerson person)
		{
			_person = person;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public string Name
		{
			get { return _person.Name.ToString(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public string EmploymentNumber
		{
			get { return _person.EmploymentNumber; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public string ApplicationLogOnName
		{
			get
			{
				if (_person.ApplicationAuthenticationInfo!=null)
					return  _person.ApplicationAuthenticationInfo.ApplicationLogOnName;
				return string.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public IPerson Person
		{
			get { return _person; }
		}
	}

}
