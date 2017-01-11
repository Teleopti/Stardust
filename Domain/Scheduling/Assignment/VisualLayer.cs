using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{

    /// <summary>
    /// Layers to be shown in a projected schedule,
    /// mixing activities and absences
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-22
    /// </remarks>
	public class VisualLayer : Layer<IPayload>, IVisualLayer, IActivityRestrictableVisualLayer
    {
	    public VisualLayer(IPayload payload, DateTimePeriod period, IActivity highestPriorityActivity, IPerson person,
		    Guid? personAbsenceId = null) : base(payload, period)
	    {
		    InParameter.NotNull(nameof(highestPriorityActivity), highestPriorityActivity);

		    HighestPriorityActivity = highestPriorityActivity;
		    Person = person;
		    PersonAbsenceId = personAbsenceId;
	    }

	    public IMultiplicatorDefinitionSet DefinitionSet { get; set; }
		public IPerson Person { get; internal set; }
        public IAbsence HighestPriorityAbsence { get; set; }
        public IActivity HighestPriorityActivity { get; set; }
        public Guid? PersonAbsenceId { get; set; }

        public Color DisplayColor()
        {
			return Payload.ConfidentialDisplayColor(Person);
        }

        public Description DisplayDescription()
        {
			return Payload.ConfidentialDescription(Person);
        }

        internal TimeSpan ThisLayerContractTime()
        {
            return hasContractTime() ? Period.ElapsedTime() : TimeSpan.Zero;
        }

        private bool hasContractTime()
        {
            if (DefinitionSet == null && HighestPriorityActivity.InContractTime)
            {
                IAbsence refAbs = HighestPriorityAbsence;
                if (refAbs == null || refAbs.InContractTime)
                {
                    return true;
                }
            }
            return false;
        }

		internal TimeSpan ThisLayerWorkTime()
		{
			return hasWorkTime() ? Period.ElapsedTime() : TimeSpan.Zero;
		}

		internal TimeSpan ThisLayerPaidTime()
		{
			return hasPaidTime() ? Period.ElapsedTime() : TimeSpan.Zero;
		}

        public TimeSpan WorkTime()
        {
            return hasWorkTime() ? Period.ElapsedTime() : TimeSpan.Zero;
        }

        private bool hasWorkTime()
        {
            if (HighestPriorityActivity.InWorkTime)
            {
                IAbsence refAbs = HighestPriorityAbsence;
                if (refAbs == null || refAbs.InWorkTime)
                {
                    return true;
                }
            }
            return false;
        }

        public TimeSpan PaidTime()
        {
            return hasPaidTime() ? Period.ElapsedTime() : TimeSpan.Zero;
        }

        private bool hasPaidTime()
        {
            if (HighestPriorityActivity.InPaidTime)
            {
                IAbsence refAbs = HighestPriorityAbsence;
                if (refAbs == null || refAbs.InPaidTime)
                {
                    return true;
                }
            }
            return false;
        }

    	public Guid ActivityId => HighestPriorityActivity != null ? HighestPriorityActivity.Id.GetValueOrDefault() : Guid.Empty;

	    public IVisualLayer CloneWithNewPeriod(DateTimePeriod newPeriod)
		{
			var ret = new VisualLayer(Payload, newPeriod, HighestPriorityActivity, Person, PersonAbsenceId)
			{
				HighestPriorityAbsence = HighestPriorityAbsence,
				DefinitionSet = DefinitionSet
			};
			return ret;
		}
    }
}
