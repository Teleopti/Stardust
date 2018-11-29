using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for skill data periods
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-17
    /// </remarks>
    public interface ISkillDataPeriod : ICloneableEntity<ISkillDataPeriod>, ISkillData
    {
    }

    /// <summary>
    /// Shared stuff for TemplateSkillDataPeriod and SkillDataPeriod
    /// </summary>
    public interface ISkillData : IPeriodized, IAggregateEntity
    {
        /// <summary>
        /// Gets or sets the service agreement.
        /// </summary>
        /// <value>The service agreement.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-14
        /// </remarks>
        ServiceAgreement ServiceAgreement { get; set; }

        /// <summary>
        /// Gets or sets the min occupancy.
        /// </summary>
        /// <value>The min occupancy.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        Percent MinOccupancy { get; set; }

        /// <summary>
        /// Gets or sets the max occupancy.
        /// </summary>
        /// <value>The max occupancy.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        Percent MaxOccupancy { get; set; }

        /// <summary>
        /// Gets or sets the service level percent.
        /// </summary>
        /// <value>The service level percent.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        Percent ServiceLevelPercent { get; set; }

        /// <summary>
        /// Gets or sets the service level seconds.
        /// </summary>
        /// <value>The service level seconds.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        double ServiceLevelSeconds { get; set; }

        /// <summary>
        /// Gets or sets the service level time span/ServiceLevelSeconds.
        /// </summary>
        /// <value>The service level time span.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-08-27
        /// </remarks>
        TimeSpan ServiceLevelTimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the minimum persons.
        /// </summary>
        /// <value>The minimum persons.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        int MinimumPersons { get; set; }

        /// <summary>
        /// Gets or sets the maximum persons.
        /// </summary>
        /// <value>The maximum persons.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        int MaximumPersons { get; set; }

        /// <summary>
        /// Gets or sets the agent skill data.
        /// </summary>
        /// <value>The agent skill data.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        SkillPersonData SkillPersonData { get; set; }

        /// <summary>
        /// Gets or sets the shrinkage, the percentage that will decrease
        /// the staff. If 10% = 90 % of the staff, so this must be handled
        /// on calculation.
        /// </summary>
        /// <value>The shrinkage.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-09
        /// </remarks>
        Percent Shrinkage { get; set; }

        ///<summary>
        /// Gets or sets the efficiency, the percentage that will decrease
        /// the staff. If 10% = 90 % of the staff, so this must be handled
        /// on calculation.
        /// </summary>
        /// <value>The efficiency.</value>
        /// <remarks>
        /// Created by: marias
        /// Created date: 2010-06-30
        /// </remarks>
        Percent Efficiency { get; set; }

        ///<summary>
        /// Gets or sets the manually inputed agents
        ///</summary>
        double? ManualAgents { get; set; }
    }
}