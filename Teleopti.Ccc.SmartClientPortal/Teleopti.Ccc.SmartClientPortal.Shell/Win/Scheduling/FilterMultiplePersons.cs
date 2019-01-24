using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using SelectionMode = System.Windows.Forms.SelectionMode;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class FilterMultiplePersons : BaseDialogForm
	{
		private ITenantLogonDataManagerClient _tenantLogonDataManager;
		private readonly ArrayList _persons = new ArrayList();
		private List<IPerson> _searchablePersons;
		private ArrayList _userSelectedPersonList;
		private readonly List<String> _duplicateInputText;
		private bool _textChangedRunning;
		private IEnumerable<LogonInfoModel> _logonInfos;
		private AdvancedAgentsFilter _advancedAgentsFilter;

		public ArrayList UserSelectedPerson
		{
			get { return _userSelectedPersonList; }
			set { _userSelectedPersonList = value; }
		}

		public FilterMultiplePersons()
		{
			_advancedAgentsFilter = new AdvancedAgentsFilter();
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
			_duplicateInputText = new List<string>();
			initializeDefaultSearchGrid();
			initializeBothResultResultGrids();
		}

		private void initializeBothResultResultGrids()
		{
			initializeResultGrid(gridListControlResult);
			initializeResultGrid(gridListControlResult2);
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
			gridListControlDefaultSearch.SelectionMode = SelectionMode.MultiExtended;
		}

		private void fillGridListControlDefaultSearch()
		{
			IEnumerable<IPerson> found = _advancedAgentsFilter.Filter(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture, textBox1.Text, _searchablePersons, _logonInfos, false);

			gridListControlDefaultSearch.BeginUpdate();
			_persons.Clear();

			if (!string.IsNullOrEmpty(textBox1.Text))
			{
				foreach (IPerson person in found)
				{
					_persons.Add(new FilterMultiplePersonGridControlItem(person, getLogonInfoModelForPerson(person.Id.GetValueOrDefault())));
				}

				gridListControlDefaultSearch.DataSource = _persons;
				if (_persons.Count > 0)
					gridListControlDefaultSearch.ValueMember = "Person";

				gridListControlDefaultSearch.MultiColumn = true;

				gridListControlDefaultSearch.Grid.ColHiddenEntries.Add(new GridColHidden(0));
				gridListControlDefaultSearch.Grid.ColHiddenEntries.Add(new GridColHidden(6));

				if (_persons.Count > 0)
				{
					gridListControlDefaultSearch.ClearSelected();
					gridListControlDefaultSearch.SetSelected(0, true);
				}

			}

			gridListControlDefaultSearch.EndUpdate();

		}

		private LogonInfoModel getLogonInfoModelForPerson(Guid personId)
		{
			var model = _logonInfos.FirstOrDefault(l => l.PersonId.Equals(personId));
			if(model == null)
				return new LogonInfoModel();
			return model;
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
			if (e.KeyCode != Keys.Enter) return;
			if (gridListControlDefaultSearch.SelectedItem == null) return;

			var selected = selectedPersons();

			foreach (var person in selected)
			{
				addPersonInResultGridFromDefaultSearch(person);
			}
		}

		#endregion

		#region "Result grid"

		private void refurbishItemsInResultGrid(GridListControl gridListControl)
		{
			gridListControl.BeginUpdate();
			gridListControl.DataSource = _userSelectedPersonList;
			if (_userSelectedPersonList.Count > 0)
				gridListControl.ValueMember = "Person";

			gridListControl.MultiColumn = true;

			gridListControl.Grid.ColHiddenEntries.Add(new GridColHidden(0));
			gridListControl.Grid.ColHiddenEntries.Add(new GridColHidden(6));

			if (_userSelectedPersonList.Count > 0)
				gridListControl.SetSelected(0, true);
			gridListControl.EndUpdate();
		}

		private void initializeResultGrid(GridListControl gridListControl)
		{
			_userSelectedPersonList = new ArrayList();
			gridListControl.DataSource = _userSelectedPersonList;
			gridListControl.MultiColumn = true;
			gridListControl.Grid.QueryCellInfo += Grid_QueryCellInfo;
			gridListControl.BorderStyle = BorderStyle.None;
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
				e.Style.Text = UserTexts.Resources.LogOn;
				e.Handled = true;
			}

			if (e.RowIndex <= 0 && e.ColIndex == 4)
			{
				e.Style.Text = UserTexts.Resources.ApplicationLogon;
				e.Handled = true;
			}

			if (e.RowIndex <= 0 && e.ColIndex == 5)
			{
				e.Style.Text = UserTexts.Resources.Email;
				e.Handled = true;
			}

		}

		private void addPersonInResultGridFromDefaultSearch(IPerson person)
		{
			_searchablePersons.Remove(person);
			fillGridListControlDefaultSearch();
			_userSelectedPersonList.Add(new FilterMultiplePersonGridControlItem(person, getLogonInfoModelForPerson(person.Id.GetValueOrDefault())));
			refurbishItemsInResultGrid(gridListControlResult);
			refurbishItemsInResultGrid(gridListControlResult2);
			textBox1.Text = String.Empty;
			textBox1.Select();
		}

		#endregion

		public IPerson SelectedPerson()
		{
			if (gridListControlDefaultSearch.SelectedItem != null)
				return (IPerson)gridListControlDefaultSearch.SelectedValue;

			return null;
		}

		public void SetSearchablePersons(IEnumerable<IPerson> searchablePersons, ITenantLogonDataManagerClient tenantLogonDataManager, AdvancedAgentsFilter advancedAgentsFilter)
		{
			_tenantLogonDataManager = tenantLogonDataManager;
			_searchablePersons = searchablePersons.ToList();
			_advancedAgentsFilter = advancedAgentsFilter;
			loadLogonInfo();
			textBox1.Select();
		}

		private void loadLogonInfo()
		{
			var guids = _searchablePersons.Select(person => person.Id.GetValueOrDefault()).ToList();
			_logonInfos = _tenantLogonDataManager.GetLogonInfoModelsForGuids(guids);
		}

		private void textBox1TextChanged(object sender, EventArgs e)
		{
			fillGridListControlDefaultSearch();
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			if (gridListControlDefaultSearch.SelectedItem == null) return;
			var selected = selectedPersons();

			foreach (var person in selected)
			{
				addPersonInResultGridFromDefaultSearch(person);
			}
		}

		private IEnumerable<IPerson> selectedPersons()
		{
			var selectedPersons = new List<IPerson>();
			var rangeList = GridHelper.GetGridSelectedRanges(gridListControlDefaultSearch.Grid, true);

			foreach (GridRangeInfo gridRangeInfo in rangeList)
			{
				for (var row = gridRangeInfo.Top - 1; row <= gridRangeInfo.Bottom - 1; row++)
				{
					if (!gridRangeInfo.IsRows) continue;
					if (!(gridListControlDefaultSearch.Items.Count > row)) continue;
					if(row < 0) continue;
					var item = (FilterMultiplePersonGridControlItem)gridListControlDefaultSearch.Items[row];
					selectedPersons.Add(item.Person);
				}
			}

			return selectedPersons;
		}

		private void buttonAdvParse_Click(object sender, EventArgs e)
		{
			parse();
		}

		private void parse()
		{
			var inputText = textBoxCustomSearch.Text;
			var currentDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
			var inputTextArray = inputText.Split(new[] { currentDelimiter.First() }, StringSplitOptions.RemoveEmptyEntries);
			CultureInfo cultureInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			_duplicateInputText.Clear();
			var actualInput = new List<String>();
			actualInput.AddRange(inputTextArray);
			foreach (var expected in inputTextArray)
			{
				string lowerSearchText = expected.ToLower(cultureInfo);
				var personQuery = _advancedAgentsFilter.Filter(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture, lowerSearchText, _searchablePersons, _logonInfos, true);
				if (personQuery.Count == 1)
				{
					var gridColumnPerson = new FilterMultiplePersonGridControlItem(personQuery.First(), getLogonInfoModelForPerson(personQuery.First().Id.GetValueOrDefault()));
					if (_userSelectedPersonList.Contains(gridColumnPerson)) continue;
					_userSelectedPersonList.Add(new FilterMultiplePersonGridControlItem(personQuery.First(), getLogonInfoModelForPerson(personQuery.First().Id.GetValueOrDefault())));
					actualInput.Remove(expected);
				}
				else if (personQuery.Count > 1)
				{
					foreach (var person in personQuery)
					{
						var tempPerson = new FilterMultiplePersonGridControlItem(person, getLogonInfoModelForPerson(person.Id.GetValueOrDefault()));
						if (!_userSelectedPersonList.Contains(tempPerson))
						{
							_duplicateInputText.Add(expected);
							break;
						}
					}
					actualInput.Remove(expected);
				}

			}
			actualInput.AddRange(_duplicateInputText);
			textBoxCustomSearch.Text = String.Join(currentDelimiter.First().ToString(), actualInput);
			refurbishItemsInResultGrid(gridListControlResult);
			refurbishItemsInResultGrid(gridListControlResult2);
		}

		public HashSet<Guid> SelectedPersonGuids()
		{
			var selectedPersonGuid = new HashSet<Guid>();
			foreach (FilterMultiplePersonGridControlItem person in _userSelectedPersonList)
			{
				selectedPersonGuid.Add(person.Person.Id.Value);
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
			if (textBoxCustomSearch.Text == currentDelimiter)
				textBoxCustomSearch.Text = string.Empty;
			_textChangedRunning = false;
		}

		private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
		{

			if (e.KeyChar != '\r') return;
			if (textBox1.Text == string.Empty && _userSelectedPersonList.Count > 0)
			{
				buttonOk.PerformClick();
				return;
			}

			if (gridListControlDefaultSearch.SelectedItem == null) return;


			var selected = selectedPersons();

			foreach (var person in selected)
			{
				addPersonInResultGridFromDefaultSearch(person);
			}
		}

		private void tabControlAdv1_SelectedIndexChanged(object sender, EventArgs e)
		{
			var current = (sender as TabControlAdv);
			if (current == null) return;
			if (current.SelectedTab == tabPageAdvCustom)
			{
				buttonAdd.Visible = false;
			}
			else
			{
				buttonAdd.Visible = true;
			}
		}

		private void textBoxCustomSearchKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != '\r') return;
			parse();
		}

		private void textBox1_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				gridListControlDefaultSearch.Select();
				if (gridListControlDefaultSearch.SelectedIndex != -1 &&
					 gridListControlDefaultSearch.SelectedIndex < gridListControlDefaultSearch.Items.Count - 1)
				{
					gridListControlDefaultSearch.SetSelected(gridListControlDefaultSearch.SelectedIndex, false);
					gridListControlDefaultSearch.SetSelected(gridListControlDefaultSearch.SelectedIndex + 1, true);
					e.Handled = true;
				}
			}
		}

		private void gridListControlDefaultSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				if (gridListControlDefaultSearch.SelectedIndex != -1 &&
					gridListControlDefaultSearch.SelectedIndex < gridListControlDefaultSearch.Items.Count - 1)
				{
					gridListControlDefaultSearch.SetSelected(gridListControlDefaultSearch.SelectedIndex, false);
					gridListControlDefaultSearch.SetSelected(gridListControlDefaultSearch.SelectedIndex + 1, true);
					e.Handled = true;
					return;
				}
			}

			if (e.KeyCode == Keys.Up)
			{
				if (gridListControlDefaultSearch.SelectedIndex > 0)
				{
					gridListControlDefaultSearch.SetSelected(gridListControlDefaultSearch.SelectedIndex, false);
					gridListControlDefaultSearch.SetSelected(gridListControlDefaultSearch.SelectedIndex - 1, true);
					e.Handled = true;
				}
			}
		}
	}


	internal class FilterMultiplePersonGridControlItem
	{
		private IPerson _person;
		private readonly LogonInfoModel _logonInfoModel;
		
		public FilterMultiplePersonGridControlItem(IPerson person, LogonInfoModel logonInfoModel)
		{
			_person = person;
			_logonInfoModel = logonInfoModel;
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

		public string LogOn
		{
			get
			{
				if (_logonInfoModel != null)
					return _logonInfoModel.Identity;
				return string.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public string ApplicationLogOnName
		{
			get
			{
				if (_logonInfoModel != null)
					return _logonInfoModel.LogonName;
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
			var compareTo = (FilterMultiplePersonGridControlItem)o;
			if (Person == compareTo.Person) return true;
			return false;
		}

		public override int GetHashCode()
		{
			return Person.Id.GetHashCode();
		}
	}	
}
