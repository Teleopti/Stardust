using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Detail level for job results
    ///</summary>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Teleopti.Interfaces.Domain")]
    public enum DetailLevel
    {
        ///<summary>
        /// Information
        ///</summary>
        Info,

        ///<summary>
        /// Warning
        ///</summary>
        Warning,

        ///<summary>
        /// Error
        ///</summary>
        Error
    }
}