namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service for verify which funciton is permitted.
    /// </summary>
    public interface IBudgetPermissionService
    {
        /// <summary>
        /// Check if allowance feature is allowed.
        /// </summary>
        bool IsAllowancePermitted { get;}
    }
}