using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
	public partial class PersonTimeZoneView : BaseRibbonForm, IPersonTimeZoneView
	{
		private readonly PersonTimeZonePresenter _presenter;
		private readonly IList<IPerson> _affectedPersons;
		private CancellationTokenSource _cancellationToken;
		private bool _busy;

		public PersonTimeZoneView(IList<IPerson> persons)
		{
			InitializeComponent();
			SetTexts();
			_presenter = new PersonTimeZonePresenter(this, persons);
			_affectedPersons = new List<IPerson>();
			_cancellationToken = new CancellationTokenSource();
			toolStripProgressBar1.Visible = false;
			toolStripStatusLabel1.Visible = false;
			PopulateTimeZones();
		}

		public void PopulateTimeZones()
		{
			comboBoxAdvTimeZones.DisplayMember = "DisplayName";
			comboBoxAdvTimeZones.ValueMember = "Id";
			comboBoxAdvTimeZones.DataSource = TimeZoneInfo.GetSystemTimeZones();	
		}

		public TimeZoneInfo SelectedTimeZone()
		{
			return comboBoxAdvTimeZones.SelectedItem as TimeZoneInfo;
		}

		public IList<IPerson> AffectedPersons()
		{
			return _affectedPersons;
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			_presenter.OnButtonAdvOkClick(comboBoxAdvTimeZones.SelectedItem as TimeZoneInfo);
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			_presenter.OnButtonAdvCancelClick();	
		}

		public void Cancel()
		{
			if(_busy)
				_cancellationToken.Cancel();
			else
			{
				Hide();
			}
		}

		public void SetPersonsTimeZone(IList<IPerson> persons, TimeZoneInfo timeZoneInfo)
		{
			_cancellationToken = new CancellationTokenSource();
			_affectedPersons.Clear();
			_busy = true;
			buttonAdvOk.Enabled = false;
			toolStripProgressBar1.Value = 0;
			toolStripStatusLabel1.Text = string.Empty;
			toolStripProgressBar1.Visible = true;
			toolStripStatusLabel1.Visible = true;

			var ui = TaskScheduler.FromCurrentSynchronizationContext();
			var task = Task.Factory.StartNew(() => setPersonsTimeZoneTask(persons, timeZoneInfo), _cancellationToken.Token);
			task.ContinueWith(taskFinished =>
			{
				_busy = false;

				if (taskFinished.IsFaulted)
				{
					showException(task.Exception);	
				}

				if (taskFinished.IsCompleted)
				{
					var statusText = Resources.Done;
					var progress = ((100 * _affectedPersons.Count) / persons.Count);
					updateProgress(statusText, progress);
					enableOk(true);
				}

				if (taskFinished.IsCanceled)
				{
					var statusText = Resources.Cancel;
					var progress = ((100 * _affectedPersons.Count) / persons.Count);
					updateProgress(statusText, progress);
					hideDialog();
				}

			}, CancellationToken.None, TaskContinuationOptions.None, ui);
		}

		private void setPersonsTimeZoneTask(IList<IPerson> persons, TimeZoneInfo timeZoneInfo)
		{
			foreach (var person in persons)
			{	
				_cancellationToken.Token.ThrowIfCancellationRequested();

				if (!person.Id.HasValue) return;

				using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
				{
					conn.Open();

					using (var tx = conn.BeginTransaction())
					{
						AgentDayConverters.ForPeople().ForEach(x => x.Execute(tx, person.Id.Value, timeZoneInfo));
						tx.Commit();
						_affectedPersons.Add(person);
						var statusText = person.Name.FirstName + " " + person.Name.LastName;
						var progress = ((100 * _affectedPersons.Count) / persons.Count);
						updateProgress(statusText, progress);
					}
				}
			}
		}

		private void enableOk(bool enable)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<bool>(enableOk), enable);
			}
			else
			{
				buttonAdvOk.Enabled = enable;
			}	
		}

		private void hideDialog()
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(hideDialog));
			}
			else
			{
				Hide();
			}
		}

		private void showException(Exception exception)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<Exception>(showException), exception);	
			}
			using (var view = new SimpleExceptionHandlerView(exception, Resources.TimeZone, Resources.ErrorOccuredWhenAccessingTheDataSource))
			{
				view.ShowDialog();
			}	
		}

		private void updateProgress(string statusText, int progress)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string, int>(updateProgress), statusText, progress);
			}
			else
			{
				toolStripProgressBar1.Value = progress;	
				toolStripStatusLabel1.Text = statusText;	
			}
		}

		private void personTimeZoneViewFormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			if (!_busy) return;
			e.Cancel = true;
			Cancel();
		}
	}
}
