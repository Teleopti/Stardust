namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface supporting a scheduleNote
    /// for a specific day
    /// </summary>
    public interface INote : IExportToAnotherScenario
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

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        int GetHashCode();

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        bool Equals(object value);

        /// <summary>
        /// Replaces the text in the note.
        /// </summary>
        /// <param name="text">The text.</param>
        void ReplaceText(string text);
    }
}