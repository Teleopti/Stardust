using System;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps an AgentDay to an PersonAssignment
    /// </summary>
    public class AgentAssignmentMapper : Mapper<IPersonAssignment, global::Domain.AgentDay>
    {
	    private ShiftContainsOvertime _shiftContainsOvertime;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAssignmentMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public AgentAssignmentMapper(MappedObjectPair mappedObjectPair, TimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
        {
	        _shiftContainsOvertime = new ShiftContainsOvertime(mappedObjectPair);
        }

	    /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public override IPersonAssignment Map(global::Domain.AgentDay oldEntity)
        {
            PersonAssignment agAss = null;
            OvertimeShiftMapper overtimeShiftMapper = new OvertimeShiftMapper(MappedObjectPair,TimeZone,oldEntity.AgentDate);
            if (oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.WorkShift)
            {
                //main shift
                agAss =
                    new PersonAssignment(MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent),
                                        MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario),
																				new DateOnly(oldEntity.AgentDate));
                MainShiftMapper msMapper = new MainShiftMapper(MappedObjectPair, TimeZone, oldEntity.AgentDate);

				new EditableShiftMapper().SetMainShiftLayers(agAss, msMapper.Map(oldEntity.AgentDayAssignment.Assigned.AssignedWorkshift));
                if (_shiftContainsOvertime.IsSatisfiedBy(oldEntity.AgentDayAssignment.Assigned.AssignedWorkshift))
                {
									foreach (IOvertimeShiftLayer overtimeLayer in overtimeShiftMapper.Map(oldEntity.AgentDayAssignment.Assigned.AssignedWorkshift))
	                {
		                agAss.AddOvertimeLayer(overtimeLayer.Payload, overtimeLayer.Period, overtimeLayer.DefinitionSet);
	                }
                }
            }

            if (oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.AbsenceWithSavedWorkShift &&
                oldEntity.AgentDayAssignment.AbsenceWorkshift != null &&
                !oldEntity.AgentDayAssignment.Assigned.AssignedAbsence.UseCountRules) //This excludes Days Off
            {
                //main shift
                agAss =
                    new PersonAssignment(MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent),
                                        MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario),
																				new DateOnly(oldEntity.AgentDate));
                MainShiftMapper msMapper = new MainShiftMapper(MappedObjectPair, TimeZone, oldEntity.AgentDate);

				new EditableShiftMapper().SetMainShiftLayers(agAss, msMapper.Map(oldEntity.AgentDayAssignment.AbsenceWorkshift));
                if (_shiftContainsOvertime.IsSatisfiedBy(oldEntity.AgentDayAssignment.AbsenceWorkshift))
                {
									foreach (IOvertimeShiftLayer overtimeLayer in overtimeShiftMapper.Map(oldEntity.AgentDayAssignment.AbsenceWorkshift))
	                {
										agAss.AddOvertimeLayer(overtimeLayer.Payload, overtimeLayer.Period, overtimeLayer.DefinitionSet);
	                }
                }
            }

			//check for day off
	        if ((oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.AbsenceWithoutSavedWorkShift ||
	             oldEntity.AgentDayAssignment.AssignmentType == global::Domain.AssignedType.AbsenceWithSavedWorkShift) &&
	            (oldEntity.AgentDayAssignment.Assigned.AssignedAbsence.UseCountRules))
	        {
				if (agAss == null)
				{
					agAss =
						new PersonAssignment(MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent),
											MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario),
																						new DateOnly(oldEntity.AgentDate));
				}

		        IDayOffTemplate defaultTemplate = MappedObjectPair.DayOff.GetPaired(oldEntity.AgentDayAssignment.Assigned.AssignedAbsence);
				agAss.SetDayOff(defaultTemplate);
	        }

	        //Always check for fillups
            if (oldEntity.AgentDayAssignment.FillupCollection.Count > 0)
            {
                if (agAss == null)
                {
                    agAss =
                        new PersonAssignment(MappedObjectPair.Agent.GetPaired(oldEntity.AssignedAgent),
                                            MappedObjectPair.Scenario.GetPaired(oldEntity.AgentScenario),
																						new DateOnly(oldEntity.AgentDate));
                }
                PersonalShiftMapper psMapper = new PersonalShiftMapper(MappedObjectPair, TimeZone,oldEntity.AgentDate);

                foreach (global::Domain.FillupShift fillup in oldEntity.AgentDayAssignment.FillupCollection)
                {
                    if(fillup.LayerCollection.Count == 0)
                        throw new InvalidOperationException("No layers in fill up shift for " + oldEntity.AssignedAgent.FullName() + " on date " + oldEntity.AgentDate);

	                foreach (var layer in psMapper.Map(fillup))
	                {
		                agAss.AddPersonalActivity(layer.Payload, layer.Period);
	                }
                    if (_shiftContainsOvertime.IsSatisfiedBy(fillup))
                    {
											foreach (IOvertimeShiftLayer overtimeLayer in overtimeShiftMapper.Map(fillup))
											{
												agAss.AddOvertimeLayer(overtimeLayer.Payload, overtimeLayer.Period, overtimeLayer.DefinitionSet);
											}
                    }
                }
            }

            return agAss;
        }
    }
}
