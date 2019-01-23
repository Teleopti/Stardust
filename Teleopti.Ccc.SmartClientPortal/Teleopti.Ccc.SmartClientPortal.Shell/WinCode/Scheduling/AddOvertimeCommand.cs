using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class AddOvertimeCommand : AddLayerCommand
    {
        private IList<IMultiplicatorDefinitionSet> _definitionSets;
	    private readonly IEditableShiftMapper _editableShiftMapper;
		
		public AddOvertimeCommand(ISchedulerStateHolder schedulerStateHolder, IScheduleViewBase scheduleViewBase, ISchedulePresenterBase presenter, IList<IMultiplicatorDefinitionSet> definitionSets, IList<IScheduleDay> scheduleParts, IEditableShiftMapper editableShiftMapper)
            : base(schedulerStateHolder, scheduleViewBase, presenter, scheduleParts ?? scheduleViewBase.SelectedSchedules())
		{
		    DefaultIsSet = false;
            if (ScheduleParts.Count > 0 && ScheduleParts[0] != null)
            {
                DefaultPeriod = GetDefaultPeriodFromPart(ScheduleParts[0]);
            }
            _definitionSets = definitionSets;
			_editableShiftMapper = editableShiftMapper;
		}

        public bool DefaultIsSet
        {
            get;
            set; 
        }

        public static DateTimePeriod GetDefaultPeriodFromPart(IScheduleDay scheduleDay)
        {
            var defaultPeriod = new DateTimePeriod();
            if (scheduleDay != null)
            {
	            var startDateTimeLocal = scheduleDay.Period.StartDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
	            defaultPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                        startDateTimeLocal.Add(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultStartHour)),
                        startDateTimeLocal.Add(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultEndHour)), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            }
            return defaultPeriod;    
        }
		
        public override void Execute()
        {
            DateTimePeriod period;
            IVisualLayer visualLayer;
            IActivity defaultActivity = null;
            IPersonAssignment personAssignment = null;
			var filteredScheduleParts = SchedulePartsOnePerAgent();

			if (!VerifySelectedSchedule(filteredScheduleParts)) return;
            if (filteredScheduleParts.Count > 0)
            {
                personAssignment = filteredScheduleParts[0].PersonAssignment();
            }

            if (personAssignment != null && (personAssignment.MainActivities().Any() || personAssignment.OvertimeActivities().Any()))
            {
                var assPeriod = personAssignment.Period;
                if (!DefaultIsSet)
                    DefaultPeriod = new DateTimePeriod(assPeriod.EndDateTime, assPeriod.EndDateTime.AddHours(1));
				var shift = _editableShiftMapper.CreateEditorShift(personAssignment);
                if (shift != null)
                {
                    visualLayer = shift.ProjectionService().CreateProjection().LastOrDefault();
                    defaultActivity = (IActivity)visualLayer.Payload;
                }
            }

        	foreach (var schedulePart in filteredScheduleParts)
        	{
				if (!schedulePart.Person.IsAgent(schedulePart.DateOnlyAsPeriod.DateOnly))
				{
					ScheduleViewBase.ShowInformationMessage(UserTexts.Resources.CouldNotAddOverTimeNoPersonPeriods, UserTexts.Resources.Information);
					return;
				}
					
        	}

            _definitionSets = DefinitionSetsAccordingToSchedule(filteredScheduleParts, _definitionSets);

            if (!VerifySelectedSchedule(filteredScheduleParts)) return;

            var addPeriod = DefaultPeriod ?? filteredScheduleParts[0].Period;
            var dialog1 = ScheduleViewBase.CreateAddOvertimeViewModel( SchedulerStateHolder.CommonStateHolder.Activities.NonDeleted(),
                                                            _definitionSets, defaultActivity,
                                                            addPeriod, SchedulerStateHolder.TimeZoneInfo);


            var result = dialog1.Result;

            if (!result) return;

            var activity = dialog1.SelectedItem;
            var definitionSet = dialog1.SelectedMultiplicatorDefinitionSet;
            period = dialog1.SelectedPeriod;

            foreach (var part in filteredScheduleParts)
            {
				part.CreateAndAddOvertime(activity, period, definitionSet);
            }

            Presenter.ModifySchedulePart(filteredScheduleParts);
            foreach (var part in filteredScheduleParts)
            {
                ScheduleViewBase.RefreshRangeForAgentPeriod(part.Person, period);
            }
        }

		public static IList<IMultiplicatorDefinitionSet> DefinitionSetsAccordingToSchedule(IList<IScheduleDay> schedules, IList<IMultiplicatorDefinitionSet> definitionSets)
        {
            IList<IMultiplicatorDefinitionSet> returnDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            foreach (IMultiplicatorDefinitionSet set in definitionSets)
            {
                returnDefinitionSets.Add(set);
            }

            foreach (IMultiplicatorDefinitionSet definitionSet in definitionSets)
            {
                foreach (IScheduleDay schedulePart in schedules)
                {
                    if (!schedulePart.Person.Period(schedulePart.DateOnlyAsPeriod.DateOnly).PersonContract.Contract.
                             MultiplicatorDefinitionSetCollection.Contains(definitionSet))
                    {
                        returnDefinitionSets.Remove(definitionSet);
                        break;
                    }
                }
            }
            return returnDefinitionSets;
        }
    }
}