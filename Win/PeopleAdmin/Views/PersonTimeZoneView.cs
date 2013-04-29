using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Collection;
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

		public PersonTimeZoneView(IList<IPerson> persons)
		{
			InitializeComponent();
			_presenter = new PersonTimeZonePresenter(this, persons);
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

		public void SetPersonTimeZone(IPerson person)
		{
			if (!person.Id.HasValue) return;

			var currentTimeZone = comboBoxAdvTimeZones.SelectedItem as TimeZoneInfo;

			using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
			{
				conn.Open();

				using (var tx = conn.BeginTransaction())
				{
					AgentDayConverters.ForPeople().ForEach(x => x.Execute(tx, person.Id.Value, currentTimeZone));
					tx.Commit();
				}
			}
		}
	}
}
