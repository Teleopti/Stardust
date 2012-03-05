﻿using System;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Coders;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
	public partial class JobStatusView : BaseDialogForm, IJobStatusView
	{
		protected JobStatusView()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
				labelDetail.Text = UserTexts.Resources.WaitingThreeDots;
			}
		}

		public JobStatusView(JobStatusModel model)  : this()
		{
			Presenter = new JobStatusPresenter(this,model, StateHolder.Instance.StateReader.ApplicationScopeData.Messaging);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Presenter.Initialize();
		}

		public JobStatusPresenter Presenter { get; internal set; }
		public void SetProgress(int percentage)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<int>(SetProgress), percentage);
				return;
			}

			if (progressBar1.Value+percentage<=progressBar1.Maximum)
			{
				progressBar1.Value += percentage;
			}
		}

		public void SetMessage(string message)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<string>(SetMessage), message);
				return;
			}

			labelDetail.Text = message;
		}

	    public void SetJobStatusId(Guid? id)
	    {
            Presenter.SetJobStatusId(id);
	    }
        
	    private void ReleaseManagedResources()
		{
			Presenter.Dispose();
			Presenter = null;
		}

        private void buttonAdvClose_Click(object sender, EventArgs e)
        {
            Close();
        }
	}

	public class JobStatusPresenter : IDisposable
    {
		private IJobStatusView _view;
		private readonly JobStatusModel _model;
		private IMessageBroker _messageBroker;
		private readonly JobResultProgressDecoder _decoder = new JobResultProgressDecoder();

		public JobStatusPresenter(IJobStatusView view, JobStatusModel model, IMessageBroker messageBroker)
		{
			_view = view;
			_model = model;
			_messageBroker = messageBroker;
		}

		public void Initialize()
		{
			_messageBroker.RegisterEventSubscription(handleIncomingJobStatus, typeof(IJobResultProgress));
		}

		private void handleIncomingJobStatus(object sender, EventMessageArgs e)
		{
			var item = _decoder.Decode(e.Message.DomainObject);
			if (item!=null && item.JobResultId==_model.JobStatusId)
			{
				_view.SetProgress(item.Percentage);
				_view.SetMessage(item.Message);
			}
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_messageBroker.UnregisterEventSubscription(handleIncomingJobStatus);
				_messageBroker = null;
				_view = null;
			}
		}

        public void SetJobStatusId(Guid? id)
	    {
            if(id == null)
                _view.SetMessage(UserTexts.Resources.CommunicationErrorEndPoint);
	        else
                _model.JobStatusId = id.GetValueOrDefault();
	    }
	}

	public interface IJobStatusView
	{
		void SetProgress(int percentage);
		void SetMessage(string message);
        void SetJobStatusId(Guid? id);
	}

	public class JobStatusModel
	{
		public Guid JobStatusId { get; set; }
        public CommandDto CommandDto { get; set; }
	}
}
