namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Multiple bool result options. Used for returning a summary result for a bool question regarding a list of objects. 
    /// </summary>
    public enum MultipleBool
        {
            /// <summary>
            /// All result is false
            /// </summary>
            AllFalse = 0,
            /// <summary>
            /// At least one result is true
            /// </summary>
            AtLeastOneTrue = 1,
            /// <summary>
            /// All result is true
            /// </summary>
            AllTrue = 2
        }
}
