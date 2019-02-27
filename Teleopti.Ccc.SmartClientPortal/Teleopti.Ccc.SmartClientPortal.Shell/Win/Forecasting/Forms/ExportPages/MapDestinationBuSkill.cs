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
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ExportPages
{
    public partial class MapDestinationBuSkill : BaseDialogForm
    {
        private DestinationSkillModel _model;
        private IList<ListViewItem> _filteredItems;
        private IList<ListViewItem> _allItems;

        public MapDestinationBuSkill()
        {
            InitializeComponent();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
        }

        public MapDestinationBuSkill(DestinationSkillModel model)
        {
            _model = model;
            InitializeComponent();
            SetTexts();
            SetColors();
        }

        private void MapDestinationBuSkill_Load(object sender, EventArgs e)
        {
            listViewSkills.BeginUpdate();

            _allItems = new List<ListViewItem>();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                using (uow.DisableFilter(QueryFilter.BusinessUnit))
                {
                    var skills = SkillRepository.DONT_USE_CTOR(uow).LoadInboundTelephonySkills(_model.Skill.DefaultResolution);                       

                    foreach (var skill in skills)
                    {
                        var lvi = new ListViewItem();
                        lvi.Tag = skill;
                        lvi.Text = skill.BusinessUnit.Name;
                        lvi.SubItems.Add(skill.Name);
                        _allItems.Add(lvi);
                    }
                    var emptyItem = new ListViewItem {Tag = null, Text = UserTexts.Resources.Empty};
                    emptyItem.SubItems.Add(UserTexts.Resources.Empty);
                    _allItems.Add(emptyItem);
                    _filteredItems = _allItems.ToList();
                    listViewSkills.Items.AddRange(_filteredItems.ToArray());
                }
            }
            listViewSkills.EndUpdate();
        }

        public ISkill SelectedSkill()
        {
            if (listViewSkills.SelectedItems.Count == 0)
                return null;

            var selectedSkill = (ISkill) listViewSkills.SelectedItems.OfType<ListViewItem>().First().Tag;
            return selectedSkill;
        }

        private void buttonAdvOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            if (listViewSkills.SelectedItems.Count == 0)
            {
                Close();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void textBoxExFilter_TextChanged(object sender, EventArgs e)
        {
            Filter(textBoxExFilter.Text);
        }

        void Filter(string filter)
        {
            listViewSkills.BeginUpdate();
            listViewSkills.Items.Clear();

            var filterItems = new FilterListViewItems(_allItems);
            _filteredItems = filterItems.Filter(filter);
            _filteredItems.ForEach(s => listViewSkills.Items.Add(s));

            listViewSkills.EndUpdate();
        }

        private void listViewSkills_DoubleClick(object sender, EventArgs e)
        {
            if (listViewSkills.SelectedItems.Count > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

    }
}
