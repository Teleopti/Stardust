using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public class PlanningTimeBankExtractor
    {
        private readonly IPerson _person;

        public PlanningTimeBankExtractor(IPerson person)
        {
            _person = person;
        }

        public PlanningTimeBankDto GetPlanningTimeBank(DateOnly dateOnly)
        {
            var dto = new PlanningTimeBankDto
            {
                BalanceInMinutes = 0,
                BalanceOutMaxMinutes = 0,
                BalanceOutMinMinutes = 0,
                BalanceOutMinutes = 0,
                IsEditable = false
            };
            var virtualPeriod = _person.VirtualSchedulePeriod(dateOnly);
            if (virtualPeriod.IsValid)
            {
                var contract = virtualPeriod.Contract;
                var timeBankCalculator = new PlanningTimeBankCalculator();
                var timeBankCalculatorParams = new PlanningTimeBackCalculatorParameters(
                    (int)contract.PlanningTimeBankMin.TotalMinutes,
                    (int)contract.PlanningTimeBankMax.TotalMinutes,
                    virtualPeriod.Seasonality.Value,
                    (int) virtualPeriod.PeriodTarget().TotalMinutes,
                    contract.AdjustTimeBankWithSeasonality,
                    virtualPeriod.PartTimePercentage.Percentage.Value,
                    contract.AdjustTimeBankWithPartTimePercentage);
                MinMax<int> balanceMinutes = timeBankCalculator.CalculateAdjustedTimeBank(timeBankCalculatorParams);

                dto.BalanceOutMinMinutes = balanceMinutes.Minimum;
                dto.BalanceOutMaxMinutes = balanceMinutes.Maximum;

                dto.BalanceInMinutes = (int)virtualPeriod.BalanceIn.TotalMinutes;
                dto.BalanceOutMinutes = (int)virtualPeriod.BalanceOut.TotalMinutes;

                dto.IsEditable = virtualPeriod.IsOriginalPeriod();
            }

            return dto;
        }
    }
}