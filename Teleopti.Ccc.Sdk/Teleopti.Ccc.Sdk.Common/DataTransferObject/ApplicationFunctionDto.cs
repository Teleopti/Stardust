using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents an ApplicationFunctionDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ApplicationFunctionDto : Dto
    {
      

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationFunctionDto"/> class.
        /// </summary>
        /// <param name="functionCode">The function code.</param>
        public ApplicationFunctionDto(string functionCode)
        {
            InParameter.NotStringEmptyOrNull("functionCode", functionCode);
            FunctionCode = functionCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationFunctionDto"/> class.
        /// </summary>
        /// <param name="applicationFunction">The application function.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ApplicationFunctionDto(IApplicationFunction applicationFunction)
        {
            FunctionDescription = applicationFunction.FunctionDescription;
            ForeignId = applicationFunction.ForeignId;
            ForeignSource = applicationFunction.ForeignSource;
            FunctionCode = applicationFunction.FunctionCode;
            FunctionPath = applicationFunction.FunctionPath;
            IsPreliminary = applicationFunction.IsPreliminary;
        }

        /// <summary>
        /// Gets or sets the function description.
        /// </summary>
        /// <value>The function description.</value>
        [DataMember]
        public string FunctionDescription{ get; set; }

        /// <summary>
        /// Gets or sets the foreign id.
        /// </summary>
        /// <value>The foreign id.</value>
        [DataMember]
        public string ForeignId{ get; set; }

        /// <summary>
        /// Gets or sets the foreign source.
        /// </summary>
        /// <value>The foreign source.</value>
        [DataMember]
        public string ForeignSource{ get; set; }

        /// <summary>
        /// Gets or sets the function code.
        /// </summary>
        /// <value>The function code.</value>
        [DataMember]
        public string FunctionCode{ get; set; }

        /// <summary>
        /// Gets or sets the function path.
        /// </summary>
        /// <value>The function path.</value>
        [DataMember]
        public string FunctionPath{ get; set; }

        /// <summary>
        /// Indicates that the application function is preliminary.
        /// </summary>
        [DataMember]
        public bool IsPreliminary { get; set; }

    }
}