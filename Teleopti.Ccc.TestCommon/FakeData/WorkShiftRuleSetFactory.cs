using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: Aruna Priyankara Wickrama
    /// Created date: 2008-05-13
    /// </remarks>
    public static class WorkShiftRuleSetFactory
    {
        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        public static WorkShiftRuleSet Create()
        {
            TimePeriodWithSegment tp1 = new TimePeriodWithSegment(new TimePeriod(5,0,6,2), new TimeSpan(1));
            TimePeriodWithSegment tp2 = new TimePeriodWithSegment(new TimePeriod(5,0,6,2), new TimeSpan(1));
            
            WorkShiftTemplateGenerator gen =
                new WorkShiftTemplateGenerator(new Activity("Test"), tp1, tp2, new ShiftCategory("Test Category"));

            WorkShiftRuleSet retObj = new WorkShiftRuleSet(gen);

            return retObj;
        }

    }
}
