using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter
{
    /// <summary>
    /// Default roots used by converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-10-22
    /// </remarks>
    public class DefaultAggregateRoot
    {
        private IBusinessUnit _businessUnit;
        private IApplicationRole _administratorRole;
        private IApplicationRole _businessUnitAdministratorRole;
        private IApplicationRole _agentRole;
        private IApplicationRole _siteManagerRole;
        private IApplicationRole _teamLeaderRole;


        //private IApplicationFunction _applicationFunction;


        /// <summary>
        /// Gets or sets the business unit.
        /// </summary>
        /// <value>The business unit.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-10-22
        /// </remarks>
        public IBusinessUnit BusinessUnit
        {
            get { return _businessUnit; }
            set { _businessUnit = value; }
        }

        /// <summary>
        /// Gets or sets the built in administrator role.
        /// </summary>
        /// <value>The administrator role.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/30/2007
        /// </remarks>
        public IApplicationRole AdministratorRole
        {
            get { return _administratorRole; }
            set { _administratorRole = value; }
        }

        /// <summary>
        /// Gets or sets the built in business unit coordinator role.
        /// </summary>
        /// <value>The business unit administrator role.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/30/2007
        /// </remarks>
        public IApplicationRole BusinessUnitAdministratorRole
        {
            get { return _businessUnitAdministratorRole; }
            set { _businessUnitAdministratorRole = value; }
        }

        /// <summary>
        /// Gets or sets the built in team leader role.
        /// </summary>
        /// <value>The built in user role.</value>
        public IApplicationRole TeamLeaderRole
        {
            get { return _teamLeaderRole; }
            set { _teamLeaderRole = value; }
        }

        /// <summary>
        /// Gets or sets the built in site manager role.
        /// </summary>
        /// <value>The built in user role.</value>
        public IApplicationRole SiteManagerRole
        {
            get { return _siteManagerRole; }
            set { _siteManagerRole = value; }
        }

        /// <summary>
        /// Gets or sets the built in standard user role.
        /// </summary>
        /// <value>The built in user role.</value>
        public IApplicationRole AgentRole
        {
            get { return _agentRole; }
            set { _agentRole = value; }
        }

        /// <summary>
        /// Gets or sets the skill type inbound telephony.
        /// </summary>
        /// <value>The skill type inbound telephony.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-18
        /// </remarks>
        public ISkillType SkillTypeInboundTelephony { get; set; }
        /// <summary>
        /// Gets or sets the skill type time.
        /// </summary>
        /// <value>The skill type time.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-18
        /// </remarks>
        public ISkillType SkillTypeTime { get; set; }

        /// <summary>
        /// Gets or sets the skill type email.
        /// </summary>
        /// <value>The skill type email.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-18
        /// </remarks>
        public ISkillType SkillTypeEmail { get; set; }

        /// <summary>
        /// Gets or sets the skill type backoffice.
        /// </summary>
        /// <value>The skill type backoffice.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-18
        /// </remarks>
        public ISkillType SkillTypeBackoffice { get; set; }

        /// <summary>
        /// Gets or sets the skill type project.
        /// </summary>
        /// <value>The skill type project.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-18
        /// </remarks>
        public ISkillType SkillTypeProject { get; set; }

        /// <summary>
        /// Gets or sets the skill type fax.
        /// </summary>
        /// <value>The skill type fax.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-18
        /// </remarks>
        public ISkillType SkillTypeFax { get; set; }
    }
}