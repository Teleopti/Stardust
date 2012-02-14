namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A holder for two objects of the same type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-12-08
    /// </remarks>
    public interface IPair<T>
    {
        /// <summary>
        /// Gets the "first" object.
        /// </summary>
        /// <value>The first.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-08
        /// </remarks>
        T First { get; }

        /// <summary>
        /// Gets the "second" object.
        /// </summary>
        /// <value>The second.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-08
        /// </remarks>
        T Second { get; }
    }
}