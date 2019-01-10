using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.PeopleAdmin;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views
{
	 public partial class PersonFinderView : BaseDialogForm, IPersonFinderView
	 {
		  private readonly PersonFinderPresenter _presenter;
		  private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();

		  public PersonFinderView(DateOnly dateOnly)
		  {
				InitializeComponent();
				SetTexts();
			dateTimePickerAdv1.SetCultureInfoSafe(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);
			  dateTimePickerAdv1.Value = dateOnly.Date;
				dateTimePickerAdv1.Calendar.TodayButton.Text = Resources.Today;
				listView1.ListViewItemSorter = new ListViewColumnSorter();
				IPersonFinderReadOnlyRepository repository = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
				IPeoplePersonFinderSearchCriteria personFinderSearchCriteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.All, string.Empty, (int)RowsPerPage.Ten , DateOnly.Today,2,0);
				IPersonFinderModel model = new PersonFinderModel(repository, personFinderSearchCriteria);
				_presenter = new PersonFinderPresenter(this, model);
				addFields();
				addRowsPerPage();
				addSettingValues();
				UpdatePageOfStatusText();
				UpdateButtonOkStatus();
		  }

		  public IList<Guid> SelectedPersonGuids => (from ListViewItem item in listView1.SelectedItems where item.Tag != null select (Guid) item.Tag).ToList();

		 public void AddRows(IList<IPersonFinderDisplayRow> rows)
		  {
				if(rows == null) throw new ArgumentNullException(nameof(rows));

				listView1.Items.Clear();
				
				foreach (var row in rows)
				{
					 if (row.PersonId == Guid.Empty) continue;
					 string dateString = string.Empty;
					 if (row.TerminalDate != DateTime.MinValue)
						  dateString = row.TerminalDate.ToShortDateString();
					var item =
						new ListViewItem(new[]
							{row.FirstName, row.LastName, row.EmploymentNumber, row.Note, dateString}) {Tag = row.PersonId};
					if (row.Grayed)
					 {
						  item.ForeColor = Color.Gray;
						  item.Tag = null;
					 }
					 listView1.Items.Add(item);
				}

				listView1.Sort();
		  }

		  private void addFields()
		  {
				var bindableList = LanguageResourceHelper.TranslateEnumToList<PersonFinderField>();

				comboBox1.DisplayMember = "Value";
				comboBox1.ValueMember = "Key";
				comboBox1.DataSource = bindableList;
		  }

		  private void addRowsPerPage()
		  {
				var bindableList = LanguageResourceHelper.TranslateEnumToList<RowsPerPage>();

				comboBox2.DisplayMember = "Value";
				comboBox2.ValueMember = "Key";
				comboBox2.DataSource = bindableList;    
		  }

		  private void addSettingValues()
		  {
				var setting = attemptDatabaseConnectionLoadSettings();
				comboBox2.SelectedValue = setting.NumberOfRowsPerPage;
		  }

		  public void UpdatePageOfStatusText()
		  {
				var currentPage = _presenter.Model.SearchCriteria.CurrentPage;
				if (_presenter.Model.SearchCriteria.TotalPages == 0) currentPage = 0;

				var pages = string.Format(CultureInfo.CurrentCulture, Resources.PageOf, currentPage, _presenter.Model.SearchCriteria.TotalPages);
				toolStripStatusLabelTotalPages.Text = pages;
		  }

		  public void UpdatePreviousNextStatus()
		  {
				linkLabel1.Enabled = new PersonFinderPreviousCommand(_presenter.Model).CanExecute();
				linkLabel2.Enabled = new PersonFinderNextCommand(_presenter.Model).CanExecute();
		  }

		  public void UpdateButtonOkStatus()
		  {
				buttonOk.Enabled = SelectedPersonGuids.Count > 0;   
		  }

		  public IPeoplePersonFinderSearchCriteria PersonFinderSearchCriteria => new PeoplePersonFinderSearchCriteria((PersonFinderField)comboBox1.SelectedValue, textBox1.Text, (int)comboBox2.SelectedValue, new DateOnly(dateTimePickerAdv1.Value), _presenter.SortColumn, (int)_presenter.SortOrder);

		 public IPeoplePersonFinderSearchCriteria PersonFinderSearchCriteriaNextPrevious
		  {
				get
				{
					 var crit = _presenter.Model.SearchCriteria;
					 var currentPage = _presenter.Model.SearchCriteria.CurrentPage;
					 return new PeoplePersonFinderSearchCriteria(crit.Field, crit.SearchValue, (int)comboBox2.SelectedValue, crit.TerminalDate, crit.SortColumn, crit.SortDirection) { CurrentPage = currentPage };
				}
		  }

		  private void TextBoxSearchValueTextChanged(object sender, EventArgs e)
		  {
				// bug 18150 Don't allow the ' character because it breaks the search string
				var currentText = textBox1.Text;
				var selectionStart = textBox1.SelectionStart;
				var selectionLength = textBox1.SelectionLength;

				int nextAsterisk;
				while ((nextAsterisk = currentText.IndexOf("'",StringComparison.OrdinalIgnoreCase)) != -1)
				{
					 if (nextAsterisk < selectionStart)
					 {
						  selectionStart--;
					 }
					 else if (nextAsterisk < selectionStart + selectionLength)
					 {
						  selectionLength--;
					 }

					 currentText = currentText.Remove(nextAsterisk, 1);
				}

				if (textBox1.Text != currentText)
				{
					 textBox1.Text = currentText;
					 textBox1.SelectionStart = selectionStart;
					 textBox1.SelectionLength = selectionLength;
				}
				_presenter.Model.SearchCriteria = PersonFinderSearchCriteria;
				button1.Enabled = new PersonFinderFindCommand(_presenter.Model).CanExecute();
				listView1.SelectedItems.Clear();
		  }

		  public void AttemptDatabaseConnectionFind(IExecutableCommand command)
		  {
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					 _dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() => _presenter.Find(command));
				}
		  }

		  private PersonFinderSettings attemptDatabaseConnectionLoadSettings()
		  {
				var setting = new PersonFinderSettings();

				_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
				{
					 using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
					 {
						  var repository = new PersonalSettingDataRepository(uow);
						  setting = repository.FindValueByKey("PersonFinderSettings", new PersonFinderSettings());
					 }
				});

				return setting;
		  }

		  private void attemptDatabaseConnectionSaveSettings()
		  {
				_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
				{
					 using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
					 {
						  var repository = new PersonalSettingDataRepository(uow);
						  var setting = repository.FindValueByKey("PersonFinderSettings", new PersonFinderSettings());
						  setting.NumberOfRowsPerPage = (RowsPerPage) comboBox2.SelectedValue;
						  setting.TerminalDate = dateTimePickerAdv1.Value;
						  repository.PersistSettingValue(setting);
						  uow.PersistAll();
					 }
				});     
		  }

		  private void ButtonFindClick(object sender, EventArgs e)
		  {
				_presenter.ButtonFindClick(new PersonFinderFindCommand(_presenter.Model));
		  }

		  private void LinkLabelPreviousLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		  {
				_presenter.LinkLabelPreviousLinkClicked(new PersonFinderPreviousCommand(_presenter.Model));
		  }

		  private void LinkLabelNextLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		  {
				_presenter.LinkLabelNextLinkClicked(new PersonFinderNextCommand(_presenter.Model));
		  }

		  private void ComboBoxRowsPerPageSelectedIndexChanged(object sender, EventArgs e)
		  {
				_presenter.ComboBoxRowsPerPageSelectedIndexChanged(new PersonFinderFindCommand(_presenter.Model));
		  }

		  private void ListViewColumnClick(object sender, ColumnClickEventArgs e)
		  {
				//var sorter = (ListViewColumnSorter)listView1.ListViewItemSorter;
				_presenter.ListViewColumnClick(e.Column);
		  }

		  private void ListViewSelectedIndexChanged(object sender, EventArgs e)
		  {
				UpdateButtonOkStatus();   
		  }

		  private void TextBoxSearchValueKeyUp(object sender, KeyEventArgs e)
		  {
				if(e.KeyCode == Keys.Enter && button1.Enabled) _presenter.ButtonFindClick(new PersonFinderFindCommand(_presenter.Model));
		  }

		 private void ButtonOkClick(object sender, EventArgs e)
		  {
				attemptDatabaseConnectionSaveSettings(); 
		  }

		 private void PersonSelectedDoubleClick(object sender, EventArgs e)
		 {
			  OnDoubleClickSelectedPeople(null);
		 }

		  public event EventHandler<EventArgs> DoubleClickSelectedPeople;

		  public void OnDoubleClickSelectedPeople(EventArgs e)
		  {
			DoubleClickSelectedPeople?.Invoke(this, e);
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		 
	 }
}
