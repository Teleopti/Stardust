using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

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

            this.BackColor = ColorHelper.FormBackgroundColor();

            searchPersonView1.SetPresenter(new SearchPersonPresenter(searchPersonView1));
            searchPersonView1.SetSearchablePersons(persons);
        }

        public IPerson SelectedPerson
        {
            get { return _selectedPerson; }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void SearchPerson_FormClosing(object sender, FormClosingEventArgs e)
        {
            _selectedPerson = searchPersonView1.SelectedPerson();
        }

        private void SearchPerson_Load(object sender, System.EventArgs e)
        {
            searchPersonView1.Select();
        }

        private void searchPersonView1_ItemDoubleClick(object sender, System.EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

    }
}
