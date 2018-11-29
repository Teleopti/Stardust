using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

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