using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting
{
	public class ReportSettingsScheduleTimeVersusTargetTimeModel
	{
		private List<IPerson> _persons;
		public IScenario Scenario { get; set; }
		public DateOnlyPeriod Period { get; set; }

		public IList<IPerson> Persons
		{
			get { return _persons; }
		}

		public void SetPersons(IList<IPerson> persons)
		{
			_persons = new List<IPerson>();
			_persons.AddRange(persons);	
		}
	}
}
