using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public JobStatusView(JobStatusModel model)  : this()
		{
			Presenter = new JobStatusPresenter(this,model, StateHolder.Instance.StateReader.ApplicationScopeData.Messaging);
			progressBar1.Maximum = model.ProgressMax;
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

			if (progressBar1.Value + percentage <= progressBar1.Maximum)
				progressBar1.Value += percentage;
			else
				progressBar1.Value = progressBar1.Maximum;

			if (progressBar1.Value == progressBar1.Maximum)
			{
				progressBar1.Visible = false;
				labelTitle.Text = UserTexts.Resources.Ready;
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
			toolTip1.SetToolTip(labelDetail, toolTip1.GetToolTip(labelDetail) + message + Environment.NewLine);
		}

		public void SetJobStatusId(Guid id)
		{
			Presenter.SetJobStatusId(id);
		}

		public void ChangeMaximumProgressValue(int percentage)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<int>(ChangeMaximumProgressValue), percentage);
				return;
			}

			progressBar1.Maximum = percentage;
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
				if (item.TotalPercentage!=100)
				_view.ChangeMaximumProgressValue(item.TotalPercentage);
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

		public void SetJobStatusId(Guid id)
		{
			if(id == Guid.Empty)
				_view.SetMessage(UserTexts.Resources.CommunicationErrorEndPoint);
			else
				_model.JobStatusId = id;
		}
	}

	public interface IJobStatusView
	{
		void SetProgress(int percentage);
		void SetMessage(string message);
		void SetJobStatusId(Guid id);
		void ChangeMaximumProgressValue(int percentage);
	}

	public class JobStatusModel
	{
		public Guid JobStatusId { get; set; }

		public int ProgressMax { get; set; }
	}
}
