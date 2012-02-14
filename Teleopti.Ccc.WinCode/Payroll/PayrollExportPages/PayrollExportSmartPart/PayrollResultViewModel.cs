using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll.PayrollExportPages.PayrollExportSmartPart
{
    /// <summary>
    /// ViewModel for PayrollResult
    /// </summary>
    /// <remarks>
    /// For now, just exposes the properties
    /// </remarks>
    public class PayrollResultViewModel : INotifyPropertyChanged, IEquatable<PayrollResultViewModel>
    {
        IPayrollResult _model;

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
                if(Details.Count() > 0)
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
            if (_model.Id.Equals(exportProgress.JobResultId))
            {
                Progress = exportProgress;
            }
        }

        private IJobResultProgress _progress;
       
        public IJobResultProgress Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                NotifyPropertyChanged("Progress");
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
