using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Configuration information.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IConfigurationInfo : ISerializable, IXmlSerializable 
    {
        /// <summary>
        /// Gets or sets the configuration id.
        /// </summary>
        /// <value>The configuration id.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        Int32 ConfigurationId { get; set; }
        /// <summary>
        /// Gets or sets the type of the configuration.
        /// </summary>
        /// <value>The type of the configuration.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string ConfigurationType { get; set; }
        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        /// <value>The name of the configuration.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string ConfigurationName { get; set; }
        /// <summary>
        /// Gets or sets the configuration value.
        /// </summary>
        /// <value>The configuration value.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string ConfigurationValue { get; set; }
        /// <summary>
        /// Gets or sets the type of the configuration data.
        /// </summary>
        /// <value>The type of the configuration data.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string ConfigurationDataType { get; set; }
        /// <summary>
        /// Gets or sets the changed by.
        /// </summary>
        /// <value>The changed by.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string ChangedBy { get; set; }
        /// <summary>
        /// Gets or sets the changed date time.
        /// </summary>
        /// <value>The changed date time.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        DateTime ChangedDateTime { get; set; }
    }
}