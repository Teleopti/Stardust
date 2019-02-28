using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
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
    public partial class SelectMultisiteSkills : BaseUserControl, IPropertyPageNoRoot<ExportSkillModel>
    {
        private IList<ListViewItem> _filteredItems;
        private IList<ListViewItem> _allItems;
        private IList<IMultisiteSkill> _skills;
        private readonly ICollection<string> _errorMessages = new List<string>();

        public SelectMultisiteSkills()
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
            loadMultisiteSkills();
            var listViewColumnSorter = new ListViewColumnSorter {Order = SortOrder.Ascending};
            listViewSkills.ListViewItemSorter = listViewColumnSorter;
            reloadSkillsListView(stateObj.ExportMultisiteSkillToSkillCommandModel);
        }

        public bool Depopulate(ExportSkillModel stateObj)
        {
            if (!validated()) return false;
            saveSelected(stateObj.ExportMultisiteSkillToSkillCommandModel);
            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return Resources.SelectMultisiteSkills; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }

        private void loadMultisiteSkills()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _skills = MultisiteSkillRepository.DONT_USE_CTOR(uow).LoadAll().ToList();
            }
        }
        
        private void saveSelected(ExportMultisiteSkillToSkillCommandModel commandDto)
        {
            foreach (var item in _allItems)
            {
                var skill = (IMultisiteSkill)item.Tag;
                if (item.Checked)
                {
                    //If exists in list, dont do anything
                    var models = commandDto.MultisiteSkillSelectionModels.Where(s => s.MultisiteSkillModel.Id.Equals(skill.Id));

                    if (models.IsEmpty()) //Else add
                    {
                        var multisiteSkillSelectionModel = new MultisiteSkillSelectionModel();
                        multisiteSkillSelectionModel.MultisiteSkillModel = new MultisiteSkillModel(skill.Id.GetValueOrDefault());
                        commandDto.MultisiteSkillSelectionModels.Add(multisiteSkillSelectionModel);
                    }
                }
                else
                {
                    var models = commandDto.MultisiteSkillSelectionModels.Where(s => s.MultisiteSkillModel.Id.Equals(skill.Id));
                    if (!models.IsEmpty())
                    {
                        commandDto.MultisiteSkillSelectionModels.Remove(models.First());
                    }
                }
            }
        }

        private void buildColumns()
        {
            var columnHeader1 = new ColumnHeader {Text = Resources.Export, Width = 120};

            var columnHeader2 = new ColumnHeader
                                    {
                                        Text = Resources.Skill,
                                        Width = listViewSkills.ClientSize.Width - columnHeader1.Width
                                    };

            listViewSkills.Columns.Add(columnHeader1);
            listViewSkills.Columns.Add(columnHeader2);
        }

        private static void findChecked(ListViewItem lvi, IMultisiteSkill multisiteSkill, ExportMultisiteSkillToSkillCommandModel commandDto)
        {
            foreach (var dto in commandDto.MultisiteSkillSelectionModels)
            {
                if (dto.MultisiteSkillModel.Id.Equals(multisiteSkill.Id))
                {
                    lvi.Checked = true;
                }
            }
        }

        private void reloadSkillsListView(ExportMultisiteSkillToSkillCommandModel commandDto)
        {
            listViewSkills.BeginUpdate();
            listViewSkills.ListViewItemSorter = null;
            listViewSkills.Items.Clear();
            listViewSkills.Columns.Clear();
            _filteredItems = new List<ListViewItem>();
            _allItems = new List<ListViewItem>();

            listViewSkills.RightToLeftLayout = (RightToLeft == RightToLeft.Yes);

            buildColumns();

            foreach (var multisiteSkill in _skills)
            {
                var lvi = new ListViewItem {Tag = multisiteSkill, Text = string.Empty};
                lvi.SubItems.Add(multisiteSkill.Name);
                findChecked(lvi, multisiteSkill, commandDto);
                _allItems.Add(lvi);
            }
            _filteredItems = _allItems.ToList();
            listViewSkills.Items.AddRange(_filteredItems.ToArray());
            listViewSkills.EndUpdate();
        }

        private bool validated()
        {
            if (listViewSkills.CheckedItems.Count != 0)
            {
                return true;
            }
            ViewBase.ShowWarningMessage(Resources.YouHaveToSelectAtLeastOneSkill, Resources.Message);
            return false;
        }

        private void textBoxExFilter_TextChanged(object sender, EventArgs e)
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
    }
}
