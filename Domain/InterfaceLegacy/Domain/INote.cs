namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface supporting a scheduleNote
    /// for a specific day
    /// </summary>
    public interface INote : IPersistableScheduleData, IExportToAnotherScenario, IVersioned
    {
    	/// <summary>
    	/// Gets the scheduleNote. Eg "Agent Kalle went home"
    	/// </summary>
    	/// <value>The scheduleNote.</value>
    	string GetScheduleNote(ITextFormatter formatter);

        /// <summary>
        /// Gets the notes date.
        /// </summary>
        /// <value>The notes date.</value>
        DateOnly NoteDate { get; }

        /// <summary>
        /// Appends the scheduleNote.
        /// </summary>
        /// <param name="text">The text.</param>
        void AppendScheduleNote(string text);

        /// <summary>
        /// Clears the scheduleNote.
        /// </summary>
        void ClearScheduleNote();

        /// <summary>
        /// Nones the entity clone.
        /// </summary>
        /// <returns></returns>
        INote NoneEntityClone();
    }
}