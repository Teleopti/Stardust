using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages.PayrollExportSmartPart
{
    public class PayrollExportSmartPartViewModel : IDisposable
    {
        private SmartClientPortal.Shell.WinCode.Common.IObservable<IPayrollResult> _observer;
        private CollectionViewSource _payrollResults;

        /// <summary>
        /// CommandModel for calling the Notify model
        /// </summary>
        /// <value>The SaveAs CommandModel</value>
        /// <remarks>
        /// Can Execute if CurrentPayrollResult is selected 
        /// </remarks>
        public CommandModel SaveAs
        {
            get; 
            private set;
        }

        public ObservableCollection<PayrollResultViewModel> PayrollResultsCollection
        {
            get;
            private set;
        }

      
        public PayrollResultViewModel CurrentPayrollResult
        {
            get { return PayrollResults.View.CurrentItem as PayrollResultViewModel; }
        }


        public CollectionViewSource PayrollResults
        {
            get { return _payrollResults; }
        }


        public PayrollExportSmartPartViewModel()
        {
            PayrollResultsCollection = new ObservablePayrollResultHashSet<PayrollResultViewModel>();
            _payrollResults = new CollectionViewSource() { Source = PayrollResultsCollection };
            _payrollResults.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Descending));
            SaveAs = new SaveSelectedAsCommand(this);
        }

		public PayrollExportSmartPartViewModel(SmartClientPortal.Shell.WinCode.Common.IObservable<IPayrollResult> observer)
            : this()
        {
            _observer = observer;
        }

        public void LoadPayrollResults(ICollection<IPayrollResult> results)
        {
            results.ForEach(r =>PayrollResultsCollection.Add(new PayrollResultViewModel(r)));
        }

        private void SaveSelected()
        {
            if (_observer != null) _observer.Notify(CurrentPayrollResult.Model);
        }

        #region command
        private class SaveSelectedAsCommand:CommandModel
        {
            private PayrollExportSmartPartViewModel _model;

            public SaveSelectedAsCommand(PayrollExportSmartPartViewModel model)
            {
                _model = model;
            }
            public override string Text
            {
                get { return UserTexts.Resources.SaveAs; }
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
               //Can execute if we have a selected item:
                e.CanExecute = (_model.CurrentPayrollResult != null) && 
                                _model.CurrentPayrollResult.Status is StatusDone;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _model.SaveSelected();
            }
        }

        #endregion

        public void UpdateProgress(IJobResultProgress exportProgress)
        {
            foreach (var viewModel in PayrollResultsCollection)
            {
                viewModel.TrySetProgress(exportProgress);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            _observer = null;
        }
    }

    //Hmm ugly as hell, will move this later to somewhere, orkar inte längre
    public class ObservablePayrollResultHashSet<T> : ObservableCollection<T> where T : PayrollResultViewModel
    {
        protected override void InsertItem(int index, T item)
        {
            int i = IndexOf(item);
            if (i==-1)
                base.InsertItem(index, item);
            else
            {
                T oldItem = this[i];
                if(item.Model.UpdatedOn.GetValueOrDefault()>oldItem.Model.UpdatedOn.GetValueOrDefault())
                    base.SetItem(i, item);
            }
        }
        protected override void SetItem(int index, T item)
        {
            int i = IndexOf(item);
            if (!(i >= 0 && i != index))
                base.SetItem(index, item);
        }
    }
}
