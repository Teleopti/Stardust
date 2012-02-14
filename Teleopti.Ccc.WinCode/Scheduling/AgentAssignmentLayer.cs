using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Person assignment layer handling
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-15
    /// </remarks>
    public static class AgentAssignmentLayer
    {
        /// <summary>
        /// Adds the layer to assignment.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="typeOfLayer">The type of layer.</param>
        /// <param name="shiftCategory">The shift category.</param>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        public static IPersonAssignment AddLayerToAssignment(IPerson agent, IScenario scenario, LayerType typeOfLayer, IShiftCategory shiftCategory, IActivity activity, DateTimePeriod period)
        {
            return AddLayerToAssignment(agent,scenario, null, typeOfLayer, shiftCategory, activity, period);
        }

        /// <summary>
        /// Adds the layer to assignment.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="personAssignment">The agent assignment.</param>
        /// <param name="typeOfLayer">The type of layer.</param>
        /// <param name="shiftCategory">The shift category.</param>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        public static IPersonAssignment AddLayerToAssignment(IPerson agent, IScenario scenario, IPersonAssignment personAssignment, LayerType typeOfLayer, IShiftCategory shiftCategory, IActivity activity, DateTimePeriod period)
        {
            InParameter.NotNull("activity", activity);

            if (personAssignment == null)
            {
                InParameter.NotNull("agent", agent);
                InParameter.NotNull("scenario", scenario);
                personAssignment = new PersonAssignment(agent, scenario);
            }

            switch (typeOfLayer)
            {
                case LayerType.MainShift:
                    if (personAssignment.MainShift == null)
                    {
                        InParameter.NotNull("shiftCategory", shiftCategory);
                        personAssignment.SetMainShift(new MainShift(shiftCategory));
                    }
                    personAssignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(activity,period));

                    break;
                case LayerType.PersonalShift:
                    PersonalShift newPersonalShift = new PersonalShift();
                    newPersonalShift.LayerCollection.Add(new PersonalShiftActivityLayer(activity, period));

                    personAssignment.AddPersonalShift(newPersonalShift);

                    break;
            }

            return personAssignment;
        }
    }

    /// <summary>
    /// Enum of layer types
    /// </summary>
    public enum LayerType
    {
        /// <summary>
        /// Main shift
        /// </summary>
        MainShift,
        /// <summary>
        /// Personal shift
        /// </summary>
        PersonalShift,
        /// <summary>
        /// Absences shift
        /// </summary>
        AbsencesShift,
        /// <summary>
        /// Overtime shift
        /// </summary>
        OvertimeShift,
    }
}
