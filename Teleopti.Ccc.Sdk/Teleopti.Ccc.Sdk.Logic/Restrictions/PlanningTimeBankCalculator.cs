using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public class PlanningTimeBankCalculator
    {
        public MinMax<int> CalculateAdjustedTimeBank(PlanningTimeBackCalculatorParameters calculatorParameters)
        {
            int balanceOutMinMinutes = calculatorParameters.BalanceOutMinMinutes;
            int balanceOutMaxMinutes = calculatorParameters.BalanceOutMaxMinutes;
            double seasonality = calculatorParameters.Seasonality;
            double partTimePercentage = calculatorParameters.PartTimePercentage;
            bool adjustTimeBankWithPartTimePercentage = calculatorParameters.AdjustTimeBankWithPartTimePercentage;
            bool adjustTimeBankWithSeasonality = calculatorParameters.AdjustTimeBankWithSeasonality;

            int normalContractTime = CalculateNormalContractTime(calculatorParameters);

            if (adjustTimeBankWithSeasonality && seasonality != 0)
            {
                if (seasonality > 0)
                {
                    balanceOutMaxMinutes = (int)(balanceOutMaxMinutes - seasonality * normalContractTime);
                }
                if (seasonality < 0)
                {
                    balanceOutMinMinutes = (int)(balanceOutMinMinutes - seasonality * normalContractTime);
                }
            }

            if(adjustTimeBankWithPartTimePercentage && partTimePercentage != 0)
            {
                balanceOutMinMinutes = (int)(balanceOutMinMinutes * partTimePercentage);
                balanceOutMaxMinutes = (int)(balanceOutMaxMinutes * partTimePercentage);
            }
            return new MinMax<int>(balanceOutMinMinutes, balanceOutMaxMinutes);
        }

        private static int CalculateNormalContractTime(PlanningTimeBackCalculatorParameters calculatorParameters)
        {
            if(calculatorParameters.PartTimePercentage > 0)
                return (int)(Math.Round(calculatorParameters.PeriodTarget / calculatorParameters.PartTimePercentage));
            return calculatorParameters.PeriodTarget;
        }
    }

    public class PlanningTimeBackCalculatorParameters
    {
        private readonly int _balanceOutMinMinutes;
        private readonly int _balanceOutMaxMinutes;
        private readonly double _seasonality;
        private readonly int _periodTarget;
        private readonly bool _adjustTimeBankWithSeasonality;
        private readonly double _partTimePercentage;
        private readonly bool _adjustTimeBankWithPartTimePercentage;

        public PlanningTimeBackCalculatorParameters(
            int balanceOutMinMinutes,
            int balanceOutMaxMinutes,
            double seasonality, 
            int periodTarget,
            bool adjustTimeBankWithSeasonality,
            double partTimePercentage, 
            bool adjustTimeBankWithPartTimePercentage)
        {
            _balanceOutMinMinutes = balanceOutMinMinutes;
            _balanceOutMaxMinutes = balanceOutMaxMinutes;
            _seasonality = seasonality;
            _periodTarget = periodTarget;
            _adjustTimeBankWithSeasonality = adjustTimeBankWithSeasonality;
            _partTimePercentage = partTimePercentage;
            _adjustTimeBankWithPartTimePercentage = adjustTimeBankWithPartTimePercentage;
        }

        public int BalanceOutMinMinutes { get { return _balanceOutMinMinutes; } }
        public int BalanceOutMaxMinutes { get { return _balanceOutMaxMinutes; } }
        public double Seasonality { get { return _seasonality; } }
        public int PeriodTarget { get { return _periodTarget; } }
        public bool AdjustTimeBankWithSeasonality { get { return _adjustTimeBankWithSeasonality; } }
        public double PartTimePercentage { get { return _partTimePercentage; } }
        public bool AdjustTimeBankWithPartTimePercentage { get { return _adjustTimeBankWithPartTimePercentage; } }
    }
}
