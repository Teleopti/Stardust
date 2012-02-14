using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Helper class for the multisite functionality
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-08
    /// </remarks>
    public static class MultisiteHelper
    {
        /// <summary>
        /// Loads the multisite days.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="multisiteSkill">The multisite skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="multisiteDayRepository">The multisite day repository.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        public static ICollection<IMultisiteDay> LoadMultisiteDays(
            DateOnlyPeriod dateTimePeriod, 
            IMultisiteSkill multisiteSkill, 
            IScenario scenario, 
            IMultisiteDayRepository multisiteDayRepository)
        {
            ICollection<IMultisiteDay> multisiteDays = multisiteDayRepository.FindRange(dateTimePeriod,multisiteSkill, scenario);
            return multisiteDayRepository.GetAllMultisiteDays(dateTimePeriod, multisiteDays, multisiteSkill, scenario);
        }

		public static ICollection<IMultisiteDay> LoadMultisiteDays(
			DateOnlyPeriod dateTimePeriod,
			IMultisiteSkill multisiteSkill,
			IScenario scenario,
			IMultisiteDayRepository multisiteDayRepository, bool addToRepository)
		{
			ICollection<IMultisiteDay> multisiteDays = multisiteDayRepository.FindRange(dateTimePeriod, multisiteSkill, scenario);
			return multisiteDayRepository.GetAllMultisiteDays(dateTimePeriod, multisiteDays, multisiteSkill, scenario, addToRepository);
		}

        public static double[] CalculateLowVarianceDistribution(double total, int count, int decimalNumber)
        {
            total *= 0.01d;
            decimalNumber += 2;
            double[] percent = new double[count];
            double x = Math.Round(total / count, decimalNumber);
            double diff = Math.Round(total - Math.Round(count * x, decimalNumber), decimalNumber);
            int share = (int)Math.Abs(Math.Round(diff * Math.Pow(10, decimalNumber)));

            for (int i = 0; i < count; i++)
            {
                if (share != 0)
                {
                    if (diff > 0)
                        percent[i] = Math.Round(x + Math.Pow(10, -decimalNumber), decimalNumber);
                    else percent[i] = Math.Round(x - Math.Pow(10, -decimalNumber), decimalNumber);
                    --share;
                }
                else
                    percent[i] = x;
            }
            return percent;
        }
    }
}