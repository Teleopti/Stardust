using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages.PayrollExportSmartPart
{
    /// <summary>
    /// ViewModel for PayrollResult
    /// </summary>
    /// <remarks>
    /// For now, just exposes the properties
    /// </remarks>
    public class PayrollResultViewModel : INotifyPropertyChanged, IEquatable<PayrollResultViewModel>
    {
        private readonly IPayrollResult _model;
		private IJobResultProgress _progress = new JobResultProgress{JobResultId = Guid.Empty,Message = UserTexts.Resources.WaitingThreeDots,Percentage = 0,TotalPercentage = 100};

        public PayrollResultViewModel(IPayrollResult result)
        {
            InParameter.NotNull("PayrollResult", result);
            _model = result;
        }

        public string Owner
        {
            get { return _model.Owner.Name.ToString(); }
        }

        public string PayrollFormatName
        {
            get { return _model.PayrollFormatName; }
        }

        public DateOnlyPeriod Period
        {
            get { return _model.Period; }
        }

        public DateTime Timestamp
        {
            get { return _model.Timestamp; }
        }

        public PayrollResult Model
        {
            get { return (PayrollResult)_model; }
        }

        public ExportStatus Status
        {
            get
            {
                var message = string.Empty;
                if(Details.Any())
                {
                    message = Details.FirstOrDefault().Message;
                }

                if (_model.HasError())
                {
                    if (string.IsNullOrEmpty(message))
                    {
                        message = UserTexts.Resources.TimedOutWhileWaitingforProcessing;
                    }
                    _progress = new JobResultProgress { Percentage = 0, Message = message };
                    return new StatusError();
                }

                if (_model.IsWorking())
                {
                    if(_progress==null)
                    {
                        _progress = new JobResultProgress {Percentage = 1, Message = UserTexts.Resources.WaitingThreeDots};
                    }
                    return new StatusWorking();
                }
                _progress = new JobResultProgress { Percentage = 100, Message = message };
                return new StatusDone();
            }
        }

        public IEnumerable<IPayrollResultDetail> Details
        {
            get
            {
                return _model.Details.OrderByDescending(p=>p.Timestamp.Ticks);
            }
        }

        public void TrySetProgress(IJobResultProgress exportProgress)
        {
            if (!_model.Id.Equals(exportProgress.JobResultId)) return;

            var currentProgress = Progress;
            if (currentProgress!=null && currentProgress.Percentage <= exportProgress.Percentage)
                Progress = exportProgress;
        }

        public IJobResultProgress Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                NotifyPropertyChanged(nameof(Progress));
                _progress = value;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(property));
			}
        }
        #endregion //INotifyPropertyChanged Members

        public bool Equals(PayrollResultViewModel other)
        {
            return Model.Equals(other.Model);
        }

        public override int GetHashCode()
        {
            return Model.GetHashCode();
        }
    }



    public class ExportStatus
    {

    }

    public class StatusDone : ExportStatus
    {

    }

    public class StatusError : ExportStatus
    {

    }

    public class StatusWorking : ExportStatus
    {

    }
}
