using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    public class SchedulePasteAction : IGridPasteAction<IScheduleDay>
    {
        private readonly PasteOptions _options;
        private readonly IGridlockManager _lockManager;
        private readonly SchedulePartFilter _schedulePartFilter;

        public IPasteBehavior PasteBehavior
        {
            get { return _options.PasteBehavior; }
        }

	    public PasteOptions PasteOptions
	    {
			get { return _options; }
	    }

        public SchedulePasteAction(PasteOptions options, IGridlockManager lockManager, SchedulePartFilter schedulePartFilter)
        {
            _lockManager = lockManager;
            _schedulePartFilter = schedulePartFilter;
            _options = options;
        }

        public IScheduleDay Paste(GridControl gridControl, Clip<IScheduleDay> clip, int rowIndex, int columnIndex)
        {
            IScheduleDay dest = gridControl[rowIndex, columnIndex].CellValue as IScheduleDay;
  
            if (dest == null)
                return null;

            IScheduleDay source = clip.ClipValue;

            return Paste(source, dest, _options);
        }

		public IScheduleDay Paste(IScheduleDay source, IScheduleDay destination, PasteOptions options)
        {
            if (source.SignificantPart() == SchedulePartView.None)
                return null;

            var lockDictionary = _lockManager.Gridlocks(destination);
            if (lockDictionary != null && lockDictionary.Count == 1)
			{
				// if it only is one lock and that is WriteProtected AND the user is allowed to change those
                // Don't remove it the user can change it
                var gridlock = new Gridlock(destination, LockType.WriteProtected);
				if (!lockDictionary.ContainsKey(gridlock.Key) || !PrincipalAuthorization.Current_DONTUSE().IsPermitted(Domain.Security.AuthorizationData.DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
				{
					return null;
				}
			}
            if (lockDictionary != null && lockDictionary.Count > 1)
                return null;

            //check for filter
            if (_schedulePartFilter != SchedulePartFilter.None)
                return null;

            if (options.Default)
            {
	            var significantPart = source.SignificantPart();
	            if(significantPart != SchedulePartView.PreferenceRestriction && significantPart != SchedulePartView.StudentAvailabilityRestriction)
                    destination.Merge(source, false, true);
            }
            else if (options.DefaultDelete)
            {
	            var significantPart = source.SignificantPart();
	            if (significantPart != SchedulePartView.PreferenceRestriction && significantPart != SchedulePartView.StudentAvailabilityRestriction)
                    destination.Merge(source, true);
            }
            else
            {
                IScheduleDay part = (IScheduleDay)source.Clone();
                IScheduleDay tempPart;
                if (options.MainShift)
                {
                    tempPart = (IScheduleDay)part.Clone();
                    tempPart.Clear<IPersonAbsence>();
                    tempPart.Clear<IPreferenceDay>();
                    tempPart.Clear<IStudentAvailabilityDay>();
                    
                    if(tempPart.SignificantPart() == SchedulePartView.MainShift)
                    {
	                    tempPart.PersonAssignment(true).SetDayOff(null);
	                    destination.Merge(tempPart, false, true);
                    }
                }

                if (options.PersonalShifts)
                {
		    IPersonAssignment personAssignmentNoMainShift = new PersonAssignment(source.Person, source.Scenario, new DateOnly(2000, 1, 1));
                    IPersonAssignment sourcePersonAssignment = source.PersonAssignment();
                    if (sourcePersonAssignment != null)
                    {
						foreach (var personalLayer in sourcePersonAssignment.PersonalActivities())
	                    {
		                    personAssignmentNoMainShift.AddPersonalActivity(personalLayer.Payload, personalLayer.Period);
	                    }
                    }

                    tempPart = (IScheduleDay)part.Clone();
                    tempPart.Clear<IPersonAbsence>();
                    tempPart.Clear<IPersonAssignment>();
                    tempPart.Clear<IPreferenceDay>();
                    tempPart.Clear<IStudentAvailabilityDay>();
                    if (personAssignmentNoMainShift.PersonalActivities().Any())
                    {
                        tempPart.Add(personAssignmentNoMainShift);
                        destination.Merge(tempPart, false);
                    }
                }

				if (options.Overtime)
				{
					((ExtractedSchedule)destination).MergeOvertime(source);
				}

                if (options.DayOff)
                {
                    tempPart = (IScheduleDay)part.Clone();
                    tempPart.Clear<IPersonAbsence>();
                    tempPart.PersonAssignment(true).ClearMainActivities();
					tempPart.PersonAssignment(true).ClearOvertimeActivities();
					tempPart.PersonAssignment(true).ClearPersonalActivities();
                    tempPart.Clear<IPreferenceDay>();
                    tempPart.Clear<IStudentAvailabilityDay>();
                    if (tempPart.HasDayOff())
                        destination.Merge(tempPart, false);
                }

                if (options.Absences != PasteAction.Ignore)
                {
                    tempPart = (IScheduleDay)part.Clone();
                    tempPart.Clear<IPersonAssignment>();
                    tempPart.Clear<IPreferenceDay>();
                    tempPart.Clear<IStudentAvailabilityDay>();

                    if (tempPart.PersonAbsenceCollection().Length > 0)
                        destination.MergeAbsences(tempPart, true);
                }

                if (options.Preference)
                {
                    tempPart = (IScheduleDay)part.Clone();
                    tempPart.Clear<IPersonAbsence>();
                    tempPart.Clear<IPersonAssignment>();
                    tempPart.Clear<IStudentAvailabilityDay>();
	                ((ExtractedSchedule) tempPart).ServiceForSignificantPartForDisplay = null;

                    if (((IList<IRestrictionBase>)tempPart.RestrictionCollection()).Count > 0)
                        destination.Merge(tempPart, false);
                }

                if (options.StudentAvailability)
                {
                    tempPart = (IScheduleDay)part.Clone();
                    tempPart.Clear<IPersonAbsence>();
                    tempPart.Clear<IPersonAssignment>();
                    tempPart.Clear<IPreferenceDay>();
					((ExtractedSchedule)tempPart).ServiceForSignificantPartForDisplay = null;

                    if (((IList<IRestrictionBase>)tempPart.RestrictionCollection()).Count > 0)
                        destination.Merge(tempPart, false);
                }

				if (options.ShiftAsOvertime)
				{
					var pasteAsOvertime = new PasteAsOvertime(source, destination, options.MulitiplicatorDefinitionSet);
					pasteAsOvertime.Paste();
				}
            }

            return destination;
        }
    }
}
