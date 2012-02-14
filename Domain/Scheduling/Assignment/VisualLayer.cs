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
    public class VisualLayer : Layer<IPayload>, IVisualLayer
    {
        internal VisualLayer(IPayload payload,
                           DateTimePeriod period,
                           IActivity highestPriorityActivity)
            : base(payload, period)
        {
            InParameter.NotNull("highestPriorityActivity", highestPriorityActivity);
            HighestPriorityActivity = highestPriorityActivity;
        }

        public IMultiplicatorDefinitionSet DefinitionSet { get; set; }
        public IPerson Person { get; set; }

        public IAbsence HighestPriorityAbsence { get; set; }

        public IActivity HighestPriorityActivity { get; set; }

        public Color DisplayColor()
        {
            return Payload.ConfidentialDisplayColor(Person);
        }

        public Description DisplayDescription()
        {
            return Payload.ConfidentialDescription(Person);
        }

        public TimeSpan ContractTime()
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

        //tested from VisualLayerCollectionTest
        public TimeSpan ReadyTime()
        {
            return hasReadyTime() ? Period.ElapsedTime() : TimeSpan.Zero;
        }

        private bool hasReadyTime()
        {
            IActivity act = Payload as IActivity;
            return act != null && act.InReadyTime;
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
    }
}
