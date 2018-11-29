using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Represents a concrete contract for a person (agent). Contains shedule and
    /// part time information.
    /// </summary>
    public class PersonContract : IPersonContract
    {
        private IContract _contract;
        private IPartTimePercentage _partTimePercentage;
        private IContractSchedule _contractSchedule;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonContract"/> class.
        /// </summary>
        protected PersonContract()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonContract"/> class.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="partTimePercentage">The part time percentage.</param>
        /// <param name="contractSchedule">The contract schedule.</param>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-30
        /// </remarks>
        public PersonContract(IContract contract, IPartTimePercentage partTimePercentage, IContractSchedule contractSchedule)
        {
            InParameter.NotNull(nameof(contract), contract);
            InParameter.NotNull(nameof(partTimePercentage), partTimePercentage);
            InParameter.NotNull(nameof(contractSchedule), contractSchedule);

            _contractSchedule = contractSchedule;
            _partTimePercentage = partTimePercentage;
            _contract = contract;
        }

        /// <summary>
        /// Contract for employment
        /// </summary>
        public virtual IContract Contract
        {
            get { return _contract; }
            set
            {
                InParameter.NotNull(nameof(Contract), value);
                _contract = value;
            }
        }

        /// <summary>
        /// The percentage for part time calculations etc.
        /// </summary>
        public virtual IPartTimePercentage PartTimePercentage
        {
            get
            {
                return _partTimePercentage;
            }
            set
            {
                InParameter.NotNull(nameof(PartTimePercentage), value);
                _partTimePercentage = value;
            }
        }

        /// <summary>
        /// Basic schedule for absence calculation and basic information for scheduling period
        /// </summary>
        public virtual IContractSchedule ContractSchedule
        {
            get { return _contractSchedule; }
            set
            {
                InParameter.NotNull(nameof(ContractSchedule), value);

                _contractSchedule = value;
            }
        }

        /// <summary>
        /// Gets the average work time per day with part time percentage applied.
        /// </summary>
        /// <value>The average work time per day.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-23
        /// </remarks>
        public TimeSpan AverageWorkTimePerDay => TimeSpan.FromMinutes(_partTimePercentage.Percentage.Value *
																	  _contract.WorkTime.AvgWorkTimePerDay.TotalMinutes);

	    #region Implementation of ICloneable

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
