namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
    /// <summary>
    /// What "iteration" is the interceptor running on?
    /// </summary>
    public enum InterceptorIteration
    {
        /// <summary>
        /// First iteration
        /// </summary>
        Normal,
        /// <summary>
        /// Second iteration
        /// </summary>
        UpdateRoots
    }
}