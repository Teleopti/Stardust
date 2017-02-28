using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
        private readonly AvailableSkillWithPreselectedSkill _skill;
        private readonly ICollection<string> _errorMessages = new List<string>();
        private readonly ListViewColumnSorter _listViewColumnSorter = new ListViewColumnSorter { Order = SortOrder.Ascending };

        public SelectSkills(AvailableSkillWithPreselectedSkill skill)
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            setColors();
            _skill = skill;
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            listViewSkills.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

        public void Populate(ReforecastModelCollection stateObj)
        {
            ReloadSkillsListView(stateObj, _skill.PreselectedSkill);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool Depopulate(ReforecastModelCollection stateObj)
        {
            if (!validated()) return false;
            stateObj.ReforecastModels.Clear();

	        var uniqueSkills = listViewSkills.CheckedItems.Cast<ListViewItem>().Select(i => (ISkill) i.Tag).Distinct();
	        foreach (var uniqueSkill in uniqueSkills)
	        {
		        stateObj.ReforecastModels.Add(new ReforecastModel{Skill = uniqueSkill});
	        }

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
            var header = new ColumnHeader { Text = Resources.Skill, Width = 357 };
            listViewSkills.Columns.Add(header);
        }

        private static void SetChecked(ListViewItem lvi)
        {
            lvi.Selected = true;
            lvi.Checked = true;
        }

        private void ReloadSkillsListView(ReforecastModelCollection stateObj, ISkill selectedSkill)
        {
            listViewSkills.BeginUpdate();
            listViewSkills.ListViewItemSorter = _listViewColumnSorter;
            listViewSkills.Items.Clear();
            listViewSkills.Columns.Clear();
            _filteredItems = new List<ListViewItem>();
            _allItems = new List<ListViewItem>();

            listViewSkills.RightToLeftLayout = (RightToLeft == RightToLeft.Yes);

            BuildColumns();

	        var isFirstTime = !stateObj.ReforecastModels.Any(m => _skill.AvailableSkills.Contains(m.Skill));
            foreach (var skill in _skill.AvailableSkills)
            {
                var lvi = new ListViewItem { Tag = skill,  Text = skill.Name };
				if (stateObj.ReforecastModels.Any(m => skill.Equals(m.Skill)))
                {
	                SetChecked(lvi);
                }
                else if (isFirstTime && skill.Equals(selectedSkill))
                {
	                SetChecked(lvi);
                }
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
                return true;

			ViewBase.ShowErrorMessage(Resources.YouHaveToSelectAtLeastOneSkill, Resources.Message);

            return false;
        }

        private void ListViewSkillsOnColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _listViewColumnSorter.SortColumn)
                _listViewColumnSorter.Order = _listViewColumnSorter.Order == SortOrder.Ascending
                                                  ? SortOrder.Descending
                                                  : SortOrder.Ascending;
            else
            {
                _listViewColumnSorter.SortColumn = e.Column;
                _listViewColumnSorter.Order = SortOrder.Ascending;
            }

            listViewSkills.Sort();
        }
    }
}
