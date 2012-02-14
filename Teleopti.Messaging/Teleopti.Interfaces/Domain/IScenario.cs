using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents an scenario
    /// </summary>
    public interface IScenario : IAggregateRoot, 
                                    IComparable<IScenario>,
                                    IChangeInfo,
                                    IBelongsToBusinessUnit
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        Description Description { get; set; }

        /// <summary>
        /// Gets if set to default workspace.
        /// </summary>
        /// <value>Default or not.</value>
        bool DefaultScenario
        {
            get; //TODO: only one scenario may be default, create special scenariocollection for this
            set;
        }

        /// <summary>
        /// Gets if audit_trail is on.
        /// </summary>
        /// <value>Audit trail is on.</value>
        bool AuditTrail { get; set; }

        /// <summary>
        /// Gets if scenario is enabled for reporting.
        /// </summary>
        /// <value>Scenario is enabled for reporting</value>
        bool EnableReporting { get; set; }

		/// <summary>
		/// Restricted
		/// </summary>
		bool Restricted { get; set; }
    }
}
