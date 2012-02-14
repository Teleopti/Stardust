using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public abstract class WorkShiftLimiter : AggregateEntity, IWorkShiftLimiter
    {
        public abstract object Clone();
        public abstract IWorkShiftLimiter NoneEntityClone();
        public abstract IWorkShiftLimiter EntityClone();
        public abstract bool IsValidAtEnd(IVisualLayerCollection endProjection);
        public abstract bool IsValidAtStart(IWorkShift shift, IList<IWorkShiftExtender> extenders);
    }
}
