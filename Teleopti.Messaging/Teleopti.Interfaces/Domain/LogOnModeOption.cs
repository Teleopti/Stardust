namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Logon mode enumeration. Provided information on what authentication types can the user use for
    /// each datasource.  
    ///</summary>
    public enum LogOnModeOption
    {
        /// <summary>
        /// Authentication type is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Authentication type is windows only
        /// </summary>
        Win,

        /// <summary>
        /// Authentication type is both windows and application 
        /// </summary>
        Mix,

    }
}