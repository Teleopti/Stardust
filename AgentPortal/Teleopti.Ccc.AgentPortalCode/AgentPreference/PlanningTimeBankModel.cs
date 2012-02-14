using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public interface IPlanningTimeBankModel
    {
        TimeSpan BalanceOut { get; set; }
        TimeSpan BalanceIn { get; }
        TimeSpan TimeBankMin { get; }
        TimeSpan TimeBankMax { get; }
        bool CanSetBalanceOut { get; }
    }

    public class PlanningTimeBankModel : IPlanningTimeBankModel
    {
        private readonly TimeSpan _balanceIn;
        private readonly TimeSpan _timeBankMin;
        private readonly TimeSpan _timeBankMax;
        private readonly bool _canSetBalanceOut;


        public PlanningTimeBankModel(
            TimeSpan balanceIn,
            TimeSpan balanceOut,
            TimeSpan timeBankMin,
            TimeSpan timeBankMax,
            bool canSetBalanceOut)
        {
            _balanceIn = balanceIn;
            BalanceOut = balanceOut;
            _timeBankMin = timeBankMin;
            _timeBankMax = timeBankMax;
            _canSetBalanceOut = canSetBalanceOut;
        }

        public TimeSpan BalanceOut { get; set; }

        public TimeSpan BalanceIn
        {
            get { return _balanceIn; }
        }

        public TimeSpan TimeBankMin
        {
            get { return _timeBankMin; }
        }

        public TimeSpan TimeBankMax
        {
            get { return _timeBankMax; }
        }

        public bool CanSetBalanceOut
        {
            get { return _canSetBalanceOut; }
        }
    }
}