using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.FileImport;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadPages
{
    public partial class WorkloadQueues : BaseUserControl, IPropertyPage
    {
        private IWorkload _workload;
        private IList<ListViewItem> _filteredItems;
        private IList<ListViewItem> _allItems;

        public WorkloadQueues()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColors();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            listViewQueues.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            _workload = aggregateRoot as IWorkload;
            var listViewColumnSorter = new ListViewColumnSorter {Order = SortOrder.Ascending};
            listViewQueues.ListViewItemSorter = listViewColumnSorter;
            reloadQueuesListView(SortOrder.Ascending);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void reloadQueuesListView(SortOrder sortOrder)
        {
            listViewQueues.BeginUpdate();
            listViewQueues.ListViewItemSorter = null;
            listViewQueues.Items.Clear();
            listViewQueues.Columns.Clear();
            _filteredItems = new List<ListViewItem>();
            _allItems = new List<ListViewItem>();

            listViewQueues.RightToLeftLayout = (RightToLeft == RightToLeft.Yes);

            var columnHeader1 = new ColumnHeader {Text = UserTexts.Resources.Queue, Width = 120};
            var columnHeader2 = new ColumnHeader {Text = UserTexts.Resources.LogObject, Width = 100};
            var columnHeader3 = new ColumnHeader {Text = UserTexts.Resources.Description, Width = 120};

            listViewQueues.Columns.Add(columnHeader1);
            listViewQueues.Columns.Add(columnHeader2);
            listViewQueues.Columns.Add(columnHeader3);

            var queues = getQueues(sortOrder);

            foreach (IQueueSource qSource in queues)
            {
                var lvi = new ListViewItem(qSource.Name);
                lvi.Tag = qSource;
                lvi.Name = qSource.Id.ToString();
                lvi.SubItems.Add(qSource.LogObjectName);
                lvi.SubItems.Add(qSource.Description);
                if (_workload.QueueSourceCollection.Contains(qSource))
                {
                    lvi.Checked = true;
                }
                _allItems.Add(lvi);
            }
            _filteredItems = _allItems.ToList();
            listViewQueues.Items.AddRange(_filteredItems.ToArray());
            listViewQueues.EndUpdate();
        }

        private static IOrderedEnumerable<IQueueSource> getQueues(SortOrder sortOrder)
        {
            IOrderedEnumerable<IQueueSource> queues;
            using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var queueSourceRepository = QueueSourceRepository.DONT_USE_CTOR(unitOfWork);
                if (sortOrder == SortOrder.Descending)
                {
                    queues = from queueSource in queueSourceRepository.LoadAll()
                             orderby queueSource.Name descending
                             select queueSource;
                }
                else
                {
                    queues = from queueSource in queueSourceRepository.LoadAll()
                             orderby queueSource.Name ascending
                             select queueSource;
                }
            }
            return queues;
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            var unique = _allItems.Union(_filteredItems);
            var workload = aggregateRoot as IWorkload;
            workload.RemoveAllQueueSources();

            IList<ListViewItem> viewItems = unique.Where(listViewItem => listViewItem.Checked).ToList();

            foreach (var listViewItem in viewItems)
            {
                workload.AddQueueSource((IQueueSource)listViewItem.Tag);
            }

            return true;
        }

        public string PageName
        {
            get { return UserTexts.Resources.Queues; }
        }

        public void SetEditMode()
        {
        }

        private void listViewQueues_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listViewQueues.Sorting == SortOrder.Ascending)
            {
                listViewQueues.Sorting = SortOrder.Descending;
            }
            else
            {
                listViewQueues.Sorting = SortOrder.Ascending;
            }
            listViewQueues.Sort();
        }

        private void listViewQueues_Resize(object sender, EventArgs e)
        {
            listViewQueues.Columns[0].Width = 120;
            listViewQueues.Columns[1].Width = 100;
            listViewQueues.Columns[2].Width = 120;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void import_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Comma Separated Text File (*.csv)|*.csv|Comma Separated Text File (*.txt)|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    new ImportWizardWindow(dialog.FileName, _workload.Skill.TimeZone).ShowDialog();
                }
                catch (IOException)
                {
                    ViewBase.ShowErrorMessage(UserTexts.Resources.TheFileIsLockedByAnotherProgram, UserTexts.Resources.FileImport);
                    return;
                }

                try
                {
                    using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        var statRep = StatisticRepositoryFactory.Create();
                        var matrixQueues = statRep.LoadQueues();
                        MatrixSync.SynchronizeQueueSources(uow, new List<IQueueSource>(matrixQueues));
                        uow.PersistAll();
                    }
                }
                catch (Exception ex)
                {
                    ViewBase.ShowErrorMessage(ex.Message, UserTexts.Resources.ErrorMessage);
                }

                var listViewColumnSorter = new ListViewColumnSorter();
                listViewColumnSorter.Order = SortOrder.Ascending;
                listViewQueues.ListViewItemSorter = listViewColumnSorter;
                reloadQueuesListView(SortOrder.Ascending);
            }
        }

        private void textBoxExFilter_TextChanged(object sender, EventArgs e)
        {
            Filter(textBoxExFilter.Text);
        }

        void Filter(string filter)
        {
            listViewQueues.BeginUpdate();
            listViewQueues.Items.Clear();

            var filterItems = new FilterListViewItems(_allItems);
            _filteredItems = filterItems.Filter(filter);

            foreach (var s in _filteredItems)
            {
                listViewQueues.Items.Add(s);
            }
            listViewQueues.EndUpdate();
        }
    }
}
