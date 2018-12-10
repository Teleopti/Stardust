using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview
{
    public interface IMeetingOverviewViewModel
    {
        IScenario CurrentScenario { get; set; }
        IEnumerable<Guid> AllSelectedPersonsId { get; set; }
        IEnumerable<Guid> FilteredPersonsId { get; set; }
        DateOnlyPeriod SelectedPeriod { get; set; }
    	bool IncludeForOrganizer { get; set; }
    }

    public class MeetingOverviewViewModel : IMeetingOverviewViewModel
    {
		public MeetingOverviewViewModel()
		{
			IncludeForOrganizer = true;
		}
        public IScenario CurrentScenario { get; set; }
        public IEnumerable<Guid> AllSelectedPersonsId { get; set; }
        public IEnumerable<Guid> FilteredPersonsId { get; set; }
        public DateOnlyPeriod SelectedPeriod { get; set; }
		public bool IncludeForOrganizer { get; set; }
    }
}