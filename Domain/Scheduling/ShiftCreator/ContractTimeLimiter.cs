using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Validates that a workshift has a specific contract time
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-06
    /// </remarks>
    public class ContractTimeLimiter : WorkShiftLimiter
    {
        private TimePeriod _timeLimit;
        private TimeSpan _lengthSegment;

        public ContractTimeLimiter(TimePeriod timeLimit, TimeSpan lengthSegment)
        {
            _timeLimit = timeLimit;
            _lengthSegment = lengthSegment;
        }

        protected ContractTimeLimiter(){}

        public virtual TimePeriod TimeLimit
        {
            get { return _timeLimit; }
            set { _timeLimit = value; }
        }


        public virtual TimeSpan LengthSegment
        {
            get { return _lengthSegment; }
            set { _lengthSegment = value; }
        }

        public override bool IsValidAtEnd(IVisualLayerCollection endProjection)
        {
            TimeSpan contractTime = endProjection.ContractTime();

            if (_timeLimit.ContainsPart(contractTime))
            {
                if (_lengthSegment.TotalMinutes != 0)
                {
                    if ((contractTime - _timeLimit.StartTime).TotalMinutes % _lengthSegment.TotalMinutes == 0)
                        return true;
                }
                else
                {
                    return true;                    
                }
            }

            return false;
        }

        public override bool IsValidAtStart(IWorkShift shift, IList<IWorkShiftExtender> extenders)
        {
            TimeSpan shiftLength = shift.LayerCollection[0].Period.ElapsedTime();
            if(shiftLength<_timeLimit.StartTime)
                return false;
            TimeSpan notInContractMax = TimeSpan.Zero;
            foreach (IWorkShiftExtender extender in extenders)
            {
                if(!extender.ExtendWithActivity.InContractTime)
                {
                    notInContractMax += extender.ExtendMaximum();
                }
            }

            return _timeLimit.EndTime.Add(notInContractMax) >= shiftLength;
        }


        public override IWorkShiftLimiter NoneEntityClone()
        {
            ContractTimeLimiter retobj = (ContractTimeLimiter)MemberwiseClone();
            retobj.SetId(null);
            retobj.SetParent(null);
            return retobj;
        }

        public override IWorkShiftLimiter EntityClone()
        {
            return (ContractTimeLimiter)MemberwiseClone();
        }


        public override object Clone()
        {
            return EntityClone();
        }
    }
}
