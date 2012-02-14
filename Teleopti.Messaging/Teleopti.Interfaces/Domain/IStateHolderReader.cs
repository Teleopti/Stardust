namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// State Holder Reader Interface
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-15
    /// </remarks>
    public interface IStateHolderReader
    {
        /// <summary>
        /// Gets the statereader.
        /// </summary>
        /// <value>The state.</value>
        IStateReader StateReader { get; }
    }
}
