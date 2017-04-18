using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    public interface IPersonPeriodModel
    {
        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-21
        /// </remarks>
        IPerson Parent{ get;}

        /// <summary>
        /// Gets or sets the person contract.
        /// </summary>
        /// <value>The person contract.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-14
        /// </remarks>
        IPersonContract PersonContract { get; set; }

        /// <summary>
        /// Gets or sets the part time percentage.
        /// </summary>
        /// <value>The part time percentage.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-14
        /// </remarks>
        IPartTimePercentage PartTimePercentage { get; set; }

        /// <summary>
        /// Gets or sets the contract.
        /// </summary>
        /// <value>The contract.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-14
        /// </remarks>
        IContract Contract { get; set; }

        /// <summary>
        /// Gets or sets the contract schedule.
        /// </summary>
        /// <value>The contract schedule.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-14
        /// </remarks>
        IContractSchedule ContractSchedule { get; set; }

        /// <summary>
        /// Gets or sets the person skills.
        /// </summary>
        /// <value>The person skills.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-14
        /// </remarks>
        string PersonSkills { get; set; }

        /// <summary>
        /// Gets or sets the rule set bag.
        /// </summary>
        /// <value>The rule set bag.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-14
        /// </remarks>
        IRuleSetBag RuleSetBag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can gray.
        /// </summary>
        /// <value><c>true</c> if this instance can gray; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-14
        /// </remarks>
        bool CanGray { get; }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-18
        /// </remarks>
        IPersonPeriod Period { get; }

        /// <summary>
        /// Gets or sets the external log on names.
        /// </summary>
        /// <value>The external log on names.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-08-15
        /// </remarks>
        string ExternalLogOnNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can bold.
        /// </summary>
        /// <value><c>true</c> if this instance can bold; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-18
        /// </remarks>
        bool CanBold { get; set; }
    }
}
