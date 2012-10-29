using System.Runtime.Serialization;


namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a ScenarioDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ScenarioDto : Dto
    {
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