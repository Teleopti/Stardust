using System;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Represents a type of Multiplicator Definition
    ///</summary>
    public interface IMultiplicatorDefinitionAdapter    
    {
        /// <summary>
        /// Gets or sets the type of the multiplicator definition.
        /// </summary>
        /// <value>The type of the multiplicator definition.</value>
        Type MultiplicatorDefinitionType { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }
    }
}