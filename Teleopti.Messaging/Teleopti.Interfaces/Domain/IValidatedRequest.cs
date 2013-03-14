namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IValidatedRequest
    {
        /// <summary>
        /// 
        /// </summary>
        string ValidationErrors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        bool IsValid { get; set; }
    }
}
