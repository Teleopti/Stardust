using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday.Reforecast
{
    public partial class SelectWorkload : BaseUserControl, IPropertyPageNoRoot<ReforecastModelCollection>
    {
        private IList<ListViewItem> _filteredItems;
        private IList<ListViewItem> _allItems;
        private Dictionary<ISkill, List<IWorkload>> _workloads;
        private readonly ICollection<string> _errorMessages = new List<string>();
        private readonly ListViewColumnSorter _listViewColumnSorter = new ListViewColumnSorter { Order = SortOrder.Ascending };

        public SelectWorkload()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            setColors();
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            listViewWorkloads.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

        public void Populate(ReforecastModelCollection stateObj)
        {
            LoadWorkloads(stateObj);
            ReloadSkillsListView();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool Depopulate(ReforecastModelCollection stateObj)
        {
            if (!validated()) return false;

            foreach (var model in stateObj.ReforecastModels)
                model.Workload.Clear();

            foreach (var item in listViewWorkloads.CheckedItems.Cast<ListViewItem>().Select(a => (IWorkload)a.Tag))
            {
                var item1 = item;

                foreach (var model in stateObj.ReforecastModels.Where(m => m.Skill == item1.Skill))
                    model.Workload.Add(item);
            }

            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return Resources.SelectWorkload; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void BuildColumns()
        {
            var header = new ColumnHeader { Text = Resources.Workload, Width = 250 };
            var subItems = new ColumnHeader {Text = Resources.Skill, Width = 250};
            listViewWorkloads.Columns.Add(header);
            listViewWorkloads.Columns.Add(subItems);
        }

        private void LoadWorkloads(ReforecastModelCollection stateObj)
        {
            var listDictionary = new Dictionary<ISkill, List<IWorkload>>();
            foreach (var skill in stateObj.ReforecastModels.Select(s => s.Skill))
            {
                var workloadList = skill.WorkloadCollection.ToList();
                listDictionary.Add(skill, workloadList);
            }
            listViewWorkloads.ListViewItemSorter = _listViewColumnSorter;
            _workloads = listDictionary;
        }

        private void ReloadSkillsListView()
        {
            listViewWorkloads.BeginUpdate();
            listViewWorkloads.ListViewItemSorter = _listViewColumnSorter;
            listViewWorkloads.Items.Clear();
            listViewWorkloads.Columns.Clear();
            _filteredItems = new List<ListViewItem>();
            _allItems = new List<ListViewItem>();

            listViewWorkloads.RightToLeftLayout = (RightToLeft == RightToLeft.Yes);

            BuildColumns();

            foreach (var pair in _workloads)
            {
                var skill = new ListViewItem.ListViewSubItem {Tag = pair.Key, Text = pair.Key.Name};
                foreach (var workload in pair.Value.Select(
                            workload => new ListViewItem {Tag = workload, Text = workload.Name}))
                {
                    workload.SubItems.Add(skill);
                    _allItems.Add(workload);
                    workload.Checked = true;
                }
            }

            _filteredItems = _allItems.ToList();
            listViewWorkloads.Items.AddRange(_filteredItems.ToArray());
            listViewWorkloads.EndUpdate();
        }

        private void textBoxExFilter_TextChanged(object sender, EventArgs e)
        {
            Filter(textBoxExFilter.Text);
        }

        private void Filter(string filter)
        {
            listViewWorkloads.BeginUpdate();
            listViewWorkloads.Items.Clear();

            var filterItems = new FilterListViewItems(_allItems);
            _filteredItems = filterItems.Filter(filter);

            foreach (var s in _filteredItems)
            {
                listViewWorkloads.Items.Add(s);
            }
            listViewWorkloads.EndUpdate();
        }

        private bool validated()
        {
            if (listViewWorkloads.CheckedItems.Count != 0)
                return true;

            MessageBoxAdv.Show(Resources.YouHaveToSelectAtLeastOneWorkload, Resources.Message,
                               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

        private void ListViewWorkloadsOnColumnClick(object sender, ColumnClickEventArgs e)
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

            listViewWorkloads.Sort();
        }
    }
}
