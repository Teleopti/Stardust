using System;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a ScenarioDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ScenarioDto : Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioDto"/> class.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"),Obsolete("Use default constructor and set property values.")]
        public ScenarioDto(IScenario scenario)
        {
            Name = scenario.Description.Name;
            ShortName = scenario.Description.ShortName;
            Id = scenario.Id;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="ScenarioDto"/> class.
		/// </summary>
		public ScenarioDto()
    	{
    	}

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name{ get; set; }

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        /// <value>The short name.</value>
        [DataMember]
        public string ShortName{ get; set; }

		/// <summary>
		/// Gets or sets the default scenario.
		/// </summary>
		/// <value>The default scenario.</value>
		[DataMember(Order = 1,IsRequired = false)]
    	public bool DefaultScenario { get; set; }
    }
}