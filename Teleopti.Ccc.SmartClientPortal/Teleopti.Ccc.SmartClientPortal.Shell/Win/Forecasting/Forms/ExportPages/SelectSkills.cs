using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ExportPages
{
    public partial class SelectSkills : BaseUserControl, IPropertyPageNoRoot<ExportSkillModel>
    {
        private IList<ListViewItem> _filteredItems;
        private IList<ListViewItem> _allItems;
        private IList<ISkill> _skills;
        private readonly ICollection<string> _errorMessages = new List<string>();

        public SelectSkills()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            setColors();
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            listViewSkills.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

        public void Populate(ExportSkillModel stateObj)
        {
            loadSkills();
            var listViewColumnSorter = new ListViewColumnSorter { Order = SortOrder.Ascending };
            listViewSkills.ListViewItemSorter = listViewColumnSorter;
            reloadSkillsListView(stateObj.ExportSkillToFileCommandModel.Skill);
        }

        public bool Depopulate(ExportSkillModel stateObj)
        {
            if (!validated()) return false;
            stateObj.ExportSkillToFileCommandModel.Skill = (ISkill)listViewSkills.SelectedItems[0].Tag;
            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return Resources.SelectSkill; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }

        private void loadSkills()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _skills = SkillRepository.DONT_USE_CTOR(uow).LoadAll().Where(x=>!(x is IChildSkill)).ToList();
            }
        }

        private void buildColumns()
        {
            var header = new ColumnHeader {Text = Resources.Skill, Width = 250};
            listViewSkills.Columns.Add(header);
        }

        private static void findChecked(ListViewItem lvi, ISkill skill, ISkill selectedSkill)
        {
            if (skill.Id.Equals(selectedSkill.Id))
                lvi.Selected = true;
        }

        private void reloadSkillsListView(ISkill selectedSkill)
        {
            listViewSkills.BeginUpdate();
            listViewSkills.ListViewItemSorter = null;
            listViewSkills.Items.Clear();
            listViewSkills.Columns.Clear();
            _filteredItems = new List<ListViewItem>();
            _allItems = new List<ListViewItem>();

            listViewSkills.RightToLeftLayout = (RightToLeft == RightToLeft.Yes);

            buildColumns();

            foreach (var skill in _skills)
            {
                var lvi = new ListViewItem { Tag = skill,  Text = skill.Name };
                if (selectedSkill != null)
                    findChecked(lvi, skill, selectedSkill);
                _allItems.Add(lvi);
            }

            _filteredItems = _allItems.ToList();
            listViewSkills.Items.AddRange(_filteredItems.ToArray());
            listViewSkills.EndUpdate();
        }

        private void textBoxExFilterTextChanged(object sender, EventArgs e)
        {
            filter(textBoxExFilter.Text);
        }

        private void filter(string filter)
        {
            listViewSkills.BeginUpdate();
            listViewSkills.Items.Clear();

            var filterItems = new FilterListViewItems(_allItems);
            _filteredItems = filterItems.Filter(filter);

            foreach (var s in _filteredItems)
            {
                listViewSkills.Items.Add(s);
            }
            listViewSkills.EndUpdate();
        }

        private bool validated()
        {
            if (listViewSkills.SelectedItems.Count != 0)
            {
                return true;
            }
            MessageBoxAdv.Show(Resources.YouHaveToSelectAtLeastOneSkill, Resources.Message,
                               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }
    }
}
