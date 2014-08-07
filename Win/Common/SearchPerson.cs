using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    public partial class SearchPerson : BaseDialogForm
    {
        private IPerson _selectedPerson;

        public SearchPerson(IEnumerable<IPerson> persons)
        {
            InitializeComponent();
            if(!DesignMode)
                SetTexts();

            searchPersonView1.SetPresenter(new SearchPersonPresenter(searchPersonView1));
            searchPersonView1.SetSearchablePersons(persons);
        }

        public IPerson SelectedPerson
        {
            get { return _selectedPerson; }
        }

        private void button1Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void searchPersonFormClosing(object sender, FormClosingEventArgs e)
        {
            _selectedPerson = searchPersonView1.SelectedPerson();
        }

        private void searchPersonLoad(object sender, System.EventArgs e)
        {
            searchPersonView1.Select();
        }

        private void searchPersonView1ItemDoubleClick(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
