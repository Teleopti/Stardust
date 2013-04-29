using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
	public partial class PersonTimeZoneView : BaseRibbonForm, IPersonTimeZoneView
	{
		private readonly PersonTimeZonePresenter _presenter;
		private readonly IPersonAssignmentConverter _personAssignmentConverter;
		private readonly IResetDateOnlyAfterChangedTimeZone _resetDateOnlyAfterChangedTimeZone;

		public PersonTimeZoneView(IList<IPerson> persons)
		{
			InitializeComponent();
			_presenter = new PersonTimeZonePresenter(this, persons);
			_personAssignmentConverter = new PersonTimeZoneSetter();
			_resetDateOnlyAfterChangedTimeZone = new ResetDateOnlyAfterChangedTimeZone();
			PopulateTimeZones();
		}

		public void PopulateTimeZones()
		{
			comboBoxAdvTimeZones.DisplayMember = "DisplayName";
			comboBoxAdvTimeZones.ValueMember = "Id";
			comboBoxAdvTimeZones.DataSource = TimeZoneInfo.GetSystemTimeZones();	
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			_presenter.OnButtonAdvOkClick();
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			_presenter.OnButtonAdvCancelClick();	
		}

		public void Cancel()
		{
			Hide();
		}

		public void HideDialog()
		{
			Hide();
		}

		public void SetPersonTimeZone(IList<IPerson> persons)
		{
			var currentTimeZone = comboBoxAdvTimeZones.SelectedItem as TimeZoneInfo;

			using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
			{
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					foreach (var person in persons)
					{
						if (!person.Id.HasValue) continue;
						_personAssignmentConverter.Execute(transaction, person.Id.Value, currentTimeZone);
					}

					transaction.Commit();
				}
			}
		}

		public void ResetDateOnly(IList<IPerson> persons)
		{
			foreach (var person in persons)
			{
				if (!person.Id.HasValue) continue;
				_resetDateOnlyAfterChangedTimeZone.ResetFor(person);
			}	
		}
	}
}
