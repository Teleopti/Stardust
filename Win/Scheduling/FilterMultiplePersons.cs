using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class FilterMultiplePersons : BaseDialogForm
	{
		private ArrayList _persons = new ArrayList();
		private List<IPerson> _searchablePersons;
		private ArrayList _userSelectedPersonList;
		private List<String > _duplicateInputText;
		private bool _textChangedRunning = false;

		public ArrayList  UserSelectedPerson 
		{ 
			get { return _userSelectedPersonList; }
			set { _userSelectedPersonList = value; }
		}

		public FilterMultiplePersons()
		{
			InitializeComponent();
			if(!DesignMode )
				SetTexts();
			_duplicateInputText = new List<string>();
			initializeDefaultSearchGrid();
			initializeResultGrid();
		}

		#region "Default search grid"

		private void initializeDefaultSearchGrid()
		{
			gridListControlDefaultSearch.DataSource = _persons;
			gridListControlDefaultSearch.MultiColumn = true;
			gridListControlDefaultSearch.Grid.MouseDoubleClick += gridMouseDoubleClick;
			gridListControlDefaultSearch.Grid.QueryCellInfo += Grid_QueryCellInfo;
			gridListControlDefaultSearch.Grid.KeyDown += gridKeyDown;
			gridListControlDefaultSearch.BorderStyle = BorderStyle.None;
		}

		private void fillGridListControlDefaultSearch()
		{
			IEnumerable<IPerson> found = Search(textBox1.Text);

			gridListControlDefaultSearch.BeginUpdate();
			_persons.Clear();

			if (!string.IsNullOrEmpty(textBox1.Text))
			{
				foreach (IPerson person in found)
				{
					_persons.Add(new FilterMultiplePersonGridControlItem(person));
				}

				gridListControlDefaultSearch.DataSource = _persons;
				if (_persons.Count > 0)
					gridListControlDefaultSearch.ValueMember = "Person";

				gridListControlDefaultSearch.MultiColumn = true;

				gridListControlDefaultSearch.Grid.ColHiddenEntries.Add(new GridColHidden(0));
				gridListControlDefaultSearch.Grid.ColHiddenEntries.Add(new GridColHidden(5));

				if (_persons.Count > 0)
					gridListControlDefaultSearch.SetSelected(0, true);

			}

			gridListControlDefaultSearch.EndUpdate();

		}

		private void gridMouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (gridListControlDefaultSearch.SelectedItem == null) return;
			var person = (IPerson)gridListControlDefaultSearch.SelectedValue;

			if (person != null)
			{
				addPersonInResultGridFromDefaultSearch(person);
			}
		}

		private void gridKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if (gridListControlDefaultSearch.SelectedItem == null) return;
				var person = (IPerson)gridListControlDefaultSearch.SelectedValue;

				if (person != null)
				{
					addPersonInResultGridFromDefaultSearch(person);
				}
			}
		}

		#endregion

		#region "Result grid"

		private void refurbishItemsInResultGrid()
		{
			gridListControlResult.BeginUpdate();
			gridListControlResult.DataSource = _userSelectedPersonList;
			if (_userSelectedPersonList.Count > 0)
				gridListControlResult.ValueMember = "Person";

			gridListControlResult.MultiColumn = true;

			gridListControlResult.Grid.ColHiddenEntries.Add(new GridColHidden(0));
			gridListControlResult.Grid.ColHiddenEntries.Add(new GridColHidden(5));

			if (_userSelectedPersonList.Count > 0)
				gridListControlResult.SetSelected(0, true);
			gridListControlResult.EndUpdate();
		}

		private void initializeResultGrid()
		{
			_userSelectedPersonList = new ArrayList();
			gridListControlResult.DataSource = _userSelectedPersonList;
			gridListControlResult.MultiColumn = true;
			gridListControlResult.Grid.QueryCellInfo += Grid_QueryCellInfo;
			gridListControlResult.BorderStyle = BorderStyle.None;
		}
		#endregion
		
		#region "Grid Common functions"

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

			if (e.RowIndex <= 0 && e.ColIndex == 4)
			{
				e.Style.Text = UserTexts.Resources.Email;
				e.Handled = true;
			}

		}

		private void addPersonInResultGridFromDefaultSearch(IPerson person)
		{
			_searchablePersons.Remove(person);
			fillGridListControlDefaultSearch();
			_userSelectedPersonList.Add(new FilterMultiplePersonGridControlItem(person));
			refurbishItemsInResultGrid();
		}
		
		#endregion

		public IPerson SelectedPerson()
		{
			if (gridListControlDefaultSearch.SelectedItem != null)
				return (IPerson) gridListControlDefaultSearch.SelectedValue;

			return null;
		}

		public void SetSearchablePersons(IEnumerable<IPerson> searchablePersons)
		{
			_searchablePersons = searchablePersons.ToList();
			textBox1.Select();
		}

		private void textBox1TextChanged(object sender, EventArgs e)
		{
			fillGridListControlDefaultSearch();
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
						 ) || 
						 person.Email.ToLower(cultureInfo).Contains(lowerSearchText )
					 select person).ToList();

			return personQuery;

		}
		
		private void buttonAdd_Click(object sender, EventArgs e)
		{
			var person = (IPerson)gridListControlDefaultSearch.SelectedValue;
			addPersonInResultGridFromDefaultSearch(person );
		}

		private void buttonAdvParse_Click(object sender, EventArgs e)
		{
			var inputText = textBoxCustomSearch.Text;
			var currentDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
			var inputTextArray = inputText.Split(new[] { currentDelimiter.First() }, StringSplitOptions.RemoveEmptyEntries);
			CultureInfo cultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
			_duplicateInputText.Clear();
			var actualInput = new List<String>();
			actualInput.AddRange(inputTextArray);
			foreach (var expected in inputTextArray)
			{
				string lowerSearchText = expected.ToLower(cultureInfo);
				var personQuery = Search(lowerSearchText);
				if (personQuery.Count == 1 )
				{
					var gridColumnPerson = new FilterMultiplePersonGridControlItem(personQuery.First());
					if (_userSelectedPersonList.Contains(gridColumnPerson)) continue;
					_userSelectedPersonList.Add(new FilterMultiplePersonGridControlItem(personQuery.First()));
					actualInput.Remove(expected);
				}
				else if (personQuery.Count > 1)
				{
					foreach (var person in personQuery)
					{
						var tempPerson = new FilterMultiplePersonGridControlItem(person);
						if (!_userSelectedPersonList.Contains(tempPerson))
						{
							_duplicateInputText.Add(expected );
							break;
						}
					}
					actualInput.Remove(expected);
				}
				
			}
			actualInput.AddRange(_duplicateInputText);
			textBoxCustomSearch.Text = String.Join(currentDelimiter.First().ToString(), actualInput);
			//if (_duplicateInputText.Count > 0)
			//{
			//	checkBoxAdvShowDuplicateRecipient.Visible = true;
			//	textBox2.Visible = true;
			//	textBox2.Text = string.Join(currentDelimiter.First().ToString(), _duplicateInputText);
			//	splitContainer1.SplitterDistance = 143;
			//}
			//else
			//{
			//	checkBoxAdvShowDuplicateRecipient.Visible = false;
			//	splitContainer1.SplitterDistance = 92;
			//}

			refurbishItemsInResultGrid();

		}

		public HashSet<Guid> SelectedPersonGuids()
		{
			var selectedPersonGuid = new HashSet<Guid>();
			foreach (FilterMultiplePersonGridControlItem person in _userSelectedPersonList)
			{
				selectedPersonGuid.Add(person.Person.Id.Value );
			}
			return selectedPersonGuid;
		}

		private void textBoxCustomSearch_TextChanged(object sender, EventArgs e)
		{
			if (_textChangedRunning)
				return;

			_textChangedRunning = true;

			var pasted = textBoxCustomSearch.Text;
			//should be loggedon culture, problem to be solved is separator could be more than one char
			var currentDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
			var preprocessed =
				pasted.Replace("\r\n", currentDelimiter)
					.Replace("\n\r", currentDelimiter)
					.Replace("\r", currentDelimiter)
					.Replace("\n", currentDelimiter) 
					.Replace("\t", currentDelimiter);
			textBoxCustomSearch.Text = preprocessed;
			_textChangedRunning = false;
		}


		private void tabControlAdv1_SelectedIndexChanged(object sender, EventArgs e)
		{
			
			var current = (sender as TabControlAdv);
			if (current == null) return;
			if (current.SelectedTab  == tabPageAdvCustom)
			{
				buttonAdd.Visible = false;
				splitContainer1.SplitterDistance = 92;
			}
			else
			{
				buttonAdd.Visible = true;
				splitContainer1.SplitterDistance = 227;
			}
		}

		private void checkBoxAdvShowUnresolved_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxAdvShowDuplicateRecipient.Checked)
			{
				splitContainer1.SplitterDistance = 143;
				textBox2.Visible = true;
			}
			else
			{
				splitContainer1.SplitterDistance = 92;
				textBox2.Visible = false;
			}

		}

		private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
		{

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
		public string EmailAddress
		{
			get
			{
				if (_person.Email != null)
					return _person.Email;
				return string.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public IPerson Person
		{
			get { return _person; }
		}

		public override bool Equals(object o)
		{
			var compareTo = (FilterMultiplePersonGridControlItem ) o;
			if (Person == compareTo.Person ) return true;
			return false;
		}

		public override int GetHashCode()
		{
			return Person.Id.GetHashCode();
		}
	}

}
