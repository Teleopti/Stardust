namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface supporting a scheduleNote
    /// for a specific day
    /// </summary>
    public interface IPublicNote : IPersistableScheduleData, IExportToAnotherScenario
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
        IPublicNote NoneEntityClone();

        /// <summary>
        /// Replaces the text in the note.
        /// </summary>
        /// <param name="text">The text.</param>
        void ReplaceText(string text);
    }
}