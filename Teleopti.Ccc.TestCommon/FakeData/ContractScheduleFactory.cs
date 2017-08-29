using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for ContractSchedule domain object
    /// </summary>
    public static class ContractScheduleFactory
    {
        /// <summary>
        /// Creates a contract schedule aggregate.
        /// </summary>
        /// <returns></returns>
        public static IContractSchedule CreateContractSchedule(string name)
        {
            return new ContractSchedule(name);
        }

		public static IContractSchedule CreateContractScheduleWithoutWorkDays(string name)
		{
			var contractSchedule = CreateContractSchedule(name);
			contractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			return contractSchedule;
		}

        /// <summary>
        /// Creates a contract schedule aggregate with a working week contract schedule.
        /// </summary>
        /// <returns></returns>
        public static IContractSchedule CreateWorkingWeekContractSchedule()
        {
            IContractSchedule ret = CreateContractSchedule("Working Week");
            IContractScheduleWeek week1 = new ContractScheduleWeek();
            
            week1.Add(DayOfWeek.Monday, true);
            week1.Add(DayOfWeek.Tuesday, true);
            week1.Add(DayOfWeek.Wednesday, true);
            week1.Add(DayOfWeek.Thursday, true);
            week1.Add(DayOfWeek.Friday, true);

            ret.AddContractScheduleWeek(week1);
            return ret;
        }
    }
}