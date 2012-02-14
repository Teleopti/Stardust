namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Writes stuff to disc
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-09-23
    /// </remarks>
    public interface IWriteToFile
    {
        /// <summary>
        /// Saves the specified content to file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="content">The content.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-09-23
        /// </remarks>
        void Save(string fileName, string content);
    }
}
