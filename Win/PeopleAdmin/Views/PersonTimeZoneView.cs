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

		public PersonTimeZoneView(IList<IPerson> persons)
		{
			InitializeComponent();
			_presenter = new PersonTimeZonePresenter(this, persons);
			_affectedPersons = new List<IPerson>();
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
			buttonAdvOk.Enabled = false;
			_presenter.OnButtonAdvOkClick(comboBoxAdvTimeZones.SelectedItem as TimeZoneInfo);
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			_presenter.OnButtonAdvCancelClick();	
		}

		public void Cancel()
		{
			_cancellationToken.Cancel();
			Hide();
		}

		public void SetPersonsTimeZone(IList<IPerson> persons, TimeZoneInfo timeZoneInfo)
		{
			_cancellationToken = new CancellationTokenSource();
			_affectedPersons.Clear();

			var ui = TaskScheduler.FromCurrentSynchronizationContext();
			var task = Task.Factory.StartNew(() => setPersonsTimeZoneTask(persons, timeZoneInfo), _cancellationToken.Token);
			task.ContinueWith(result => Hide(), CancellationToken.None, TaskContinuationOptions.None, ui);

			try
			{
				task.Wait();
			}

			catch (AggregateException aggregateException)
			{
				using (var view = new SimpleExceptionHandlerView(aggregateException.GetBaseException(), Resources.TimeZone, Resources.ErrorOccuredWhenAccessingTheDataSource))
				{
					view.ShowDialog();
				}	
			}
		}

		private void setPersonsTimeZoneTask(IEnumerable<IPerson> persons, TimeZoneInfo timeZoneInfo)
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
					}
				}
			}	
		}
	}
}
