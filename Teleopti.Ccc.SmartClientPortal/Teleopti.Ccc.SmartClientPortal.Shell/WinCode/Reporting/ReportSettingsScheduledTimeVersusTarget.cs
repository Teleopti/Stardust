using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.WinCode.Reporting
{
	[Serializable]
	public class ReportSettingsScheduledTimeVersusTarget : SettingValue
	{

		private DateTime _startDate = DateTime.Today;
		private DateTime _endDate = DateTime.Today;
		private Guid? _scenarioId;
		private IList<Guid> _persons = new List<Guid>();
		private string _groupPage = string.Empty;


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<Guid> Persons
		{
			get { return _persons; }
			set { _persons = value; }
		}
	
		public Guid? Scenario
		{
			get { return _scenarioId; }
			set { _scenarioId = value; }
		}
		public DateTime StartDate
		{
			get { return _startDate; }
			set { _startDate = value; }
		}
		public DateTime EndDate
		{
			get { return _endDate; }
			set { _endDate = value; }
		}

		public String GroupPage
		{
			get { return _groupPage; }
			set { _groupPage = value; }
		}
	}
}
