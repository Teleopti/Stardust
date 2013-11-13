using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class AddOvertimeCommand : AddLayerCommand
    {
        private IList<IMultiplicatorDefinitionSet> _definitionSets;
	    private readonly IEditableShiftMapper _editableShiftMapper;

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
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
                defaultPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                        scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.FromHours(8)),
                        scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.FromHours(17)));
            }
            return defaultPeriod;    
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override void Execute()
        {
            DateTimePeriod period;
            IVisualLayer visualLayer;
            IActivity defaultActivity = null;
            IPersonAssignment personAssignment = null;
            if (!VerifySelectedSchedule(ScheduleParts)) return;
            if (ScheduleParts.Count > 0)
            {
                personAssignment = ScheduleParts[0].PersonAssignment();
            }

			//if (personAssignment != null && personAssignment.HasProjection)
            if (personAssignment != null && (personAssignment.MainLayers().Any() || personAssignment.OvertimeLayers().Any()))
            {
                DateTimePeriod assPeriod = personAssignment.Period;
                if (!DefaultIsSet)
                    DefaultPeriod = new DateTimePeriod(assPeriod.EndDateTime, assPeriod.EndDateTime.AddHours(1));
				var shift = _editableShiftMapper.CreateEditorShift(personAssignment);
                if (shift != null)
                {
                    visualLayer = shift.ProjectionService().CreateProjection().LastOrDefault();
                    defaultActivity = (IActivity)visualLayer.Payload;
                }
            }

        	foreach (var schedulePart in ScheduleParts)
        	{
				if (schedulePart.Person.Period(schedulePart.DateOnlyAsPeriod.DateOnly) == null)
				{
					ScheduleViewBase.ShowInformationMessage(UserTexts.Resources.CouldNotAddOverTimeNoPersonPeriods, UserTexts.Resources.Information);
					return;
				}
					
        	}

            _definitionSets = DefinitionSetsAccordingToSchedule(ScheduleParts, _definitionSets);

            if (!VerifySelectedSchedule(ScheduleParts)) return;

            DateTimePeriod addPeriod = DefaultPeriod ?? ScheduleParts[0].Period;

            IAddOvertimeViewModel dialog1 =
                ScheduleViewBase.CreateAddOvertimeViewModel(ScheduleParts.FirstOrDefault(), SchedulerStateHolder.CommonStateHolder.ActiveActivities,
                                                            _definitionSets, defaultActivity,
                                                            addPeriod, SchedulerStateHolder.TimeZoneInfo);


            bool result = dialog1.Result;

            if (!result) return;

            IActivity activity = dialog1.SelectedItem;
            IMultiplicatorDefinitionSet definitionSet = dialog1.SelectedMultiplicatorDefinitionSet;
            period = dialog1.SelectedPeriod;

            foreach (IScheduleDay part in ScheduleParts)
            {
								part.CreateAndAddOvertime(activity, period, definitionSet);
            }


            Presenter.ModifySchedulePart(ScheduleParts);
            foreach (IScheduleDay part in ScheduleParts)
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