namespace Teleopti.Interfaces.Events
{
    /// <summary>
    /// IQueueMessage is an Avanode interface
    /// for obsolete Enterprise Library code.
    /// This will eventually be removed since the
    /// enterprise code introduced some bugs in 
    /// our usage.
    /// </summary>
    public interface IQueueMessage
    {
        /// <summary>
        /// Dispatch run method.
        /// </summary>
        void Run();
    }
}