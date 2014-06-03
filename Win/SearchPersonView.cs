using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using System.Collections;
using Syncfusion.Windows.Forms.Grid;
using System.Drawing;

namespace Teleopti.Ccc.Win
{
    public partial class SearchPersonView : BaseUserControl, ISearchPersonView
    {
        private SearchPersonPresenter _presenter;
        ArrayList _persons = new ArrayList();
        
        public SearchPersonView()
        {
            InitializeComponent();
            GridStyleInfoExtensions.ResetDefault();
            if(!DesignMode)
                SetTexts();

            gridListControl1.DataSource = _persons;
            gridListControl1.MultiColumn = true;
            gridListControl1.Grid.MouseDoubleClick += new MouseEventHandler(Grid_MouseDoubleClick);
            gridListControl1.Grid.QueryCellInfo += new GridQueryCellInfoEventHandler(Grid_QueryCellInfo);
            gridListControl1.Grid.KeyDown += new KeyEventHandler(Grid_KeyDown);
            
        }

        void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
	            if (gridListControl1.SelectedItem == null) return;
                var person = (IPerson)gridListControl1.SelectedValue;

                if (person != null)
                {
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
                e.Style.Text = UserTexts.Resources.Note;
                e.Handled = true;
            }
        }

        private void Grid_MouseDoubleClick(object sender, MouseEventArgs e)
        {
	        if (gridListControl1.SelectedItem == null) return;
            var person = (IPerson)gridListControl1.SelectedValue;

            if (person != null)
            {
            	var handler = ItemDoubleClick;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs> ItemDoubleClick;

        public void SetPresenter(SearchPersonPresenter presenter)
        {
            _presenter = presenter;
        }

        public IPerson SelectedPerson()
        { 
            if (gridListControl1.SelectedItem != null)
                return (IPerson)gridListControl1.SelectedValue;
          
            return null;
        }

        public void SetSearchablePersons(IEnumerable<IPerson> searchablePersons)
        {
            _presenter.SetSearchablePersons(searchablePersons);
            textBox1.Select();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _presenter.OnTextBox1TextChanged();
        }

        public void FillGridListControl()
        {
            IEnumerable<IPerson> found = _presenter.Search(textBox1.Text);

            gridListControl1.BeginUpdate();
            _persons.Clear();

            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                foreach (IPerson person in found)
                {
                    _persons.Add(new GridControlItem(person));
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
    }

    internal class GridControlItem
    {
        private IPerson _person;
        
        public GridControlItem(IPerson person)
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
        public string Note
        {
            get { return _person.Note; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public IPerson Person
        {
            get { return _person; }
        }     
    }
}
