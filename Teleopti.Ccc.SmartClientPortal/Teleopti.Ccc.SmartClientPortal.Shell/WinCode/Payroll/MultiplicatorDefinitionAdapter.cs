using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll
{
    /// <summary>
    /// Represents a type of Multiplicator Definition
    /// </summary>
    public class MultiplicatorDefinitionAdapter : IMultiplicatorDefinitionAdapter
    {
        private MultiplicatorDefinitionAdapter() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicatorDefinitionAdapter"/> class.
        /// </summary>
        /// <param name="multiplicatorDefinitionType">Type of the multiplicator definition.</param>
        /// <param name="name">The name.</param>
        public MultiplicatorDefinitionAdapter(Type multiplicatorDefinitionType, string name)
            : this()
        {
            MultiplicatorDefinitionType = multiplicatorDefinitionType;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the type of the multiplicator definition.
        /// </summary>
        /// <value>The type of the multiplicator definition.</value>
        public Type MultiplicatorDefinitionType { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }
}
