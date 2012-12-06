using System;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public interface IAdvanceSchedulingService
    {
        object BlockIntradayAggregation();
        object EffectiveRestrictionAggregation();
        object RunScheduling();
    }
    public class AdvanceSchedulingService : IAdvanceSchedulingService
    {
        public object BlockIntradayAggregation()
        {
            throw new NotImplementedException();
        }

        public object EffectiveRestrictionAggregation()
        {
            throw new NotImplementedException();
        }

        public object RunScheduling()
        {
            throw new NotImplementedException();
        }
    }
}