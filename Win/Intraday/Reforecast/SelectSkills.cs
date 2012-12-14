﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.Filter;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Intraday.Reforecast
{
    public partial class SelectSkills : BaseUserControl, IPropertyPageNoRoot<ReforecastModelCollection>
    {
        private IList<ListViewItem> _filteredItems;
        private IList<ListViewItem> _allItems;
        private readonly IList<ISkill> _skills;
        private readonly ICollection<string> _errorMessages = new List<string>();

        public SelectSkills(IList<ISkill> skills)
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            setColors();
            _skills = skills;
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            listViewSkills.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

        public void Populate(ReforecastModelCollection stateObj)
        {
            ReloadSkillsListView(_skills.Any() ? _skills[0] : null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool Depopulate(ReforecastModelCollection stateObj)
        {
            if (!validated()) return false;

            foreach (ListViewItem item in listViewSkills.CheckedItems)
                stateObj.ReforecastModels.Add(new ReforecastModel { Skill = (ISkill)item.Tag});

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void BuildColumns()
        {
            var header = new ColumnHeader {Text = Resources.Skill, Width = 250};
            listViewSkills.Columns.Add(header);
        }

        private static void FindChecked(ListViewItem lvi, ISkill skill, ISkill selectedSkill)
        {
            if (!skill.Id.Equals(selectedSkill.Id)) return;
            lvi.Selected = true;
            lvi.Checked = true;
        }

        private void ReloadSkillsListView(ISkill selectedSkill)
        {
            listViewSkills.BeginUpdate();
            listViewSkills.ListViewItemSorter = null;
            listViewSkills.Items.Clear();
            listViewSkills.Columns.Clear();
            _filteredItems = new List<ListViewItem>();
            _allItems = new List<ListViewItem>();

            listViewSkills.RightToLeftLayout = (RightToLeft == RightToLeft.Yes);

            BuildColumns();

            foreach (var skill in _skills)
            {
                var lvi = new ListViewItem { Tag = skill,  Text = skill.Name };
                if (selectedSkill != null)
                    FindChecked(lvi, skill, selectedSkill);
                _allItems.Add(lvi);
            }

            _filteredItems = _allItems.ToList();
            listViewSkills.Items.AddRange(_filteredItems.ToArray());
            listViewSkills.EndUpdate();
        }

        private void textBoxExFilter_TextChanged(object sender, EventArgs e)
        {
            Filter(textBoxExFilter.Text);
        }

        private void Filter(string filter)
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
            if (listViewSkills.CheckedItems.Count != 0)
            {
                return true;
            }
            MessageBoxAdv.Show(Resources.YouHaveToSelectAtLeastOneSkill, Resources.Message,
                               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }
    }
}
