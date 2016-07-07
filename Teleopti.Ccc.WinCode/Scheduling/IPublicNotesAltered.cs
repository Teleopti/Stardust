namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-12-02
    /// </remarks>
    public interface IPublicNotesAltered
    {
        /// <summary>
        /// Publics the notes altered.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-12-02
        /// </remarks>
        void PublicNotesAltered();

        /// <summary>
        /// Gets or sets a value indicating whether [public notes is altered].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [public notes is altered]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-12-02
        /// </remarks>
        bool PublicNotesIsAltered { get; set; }

        /// <summary>
        /// Publics the note removed.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-12-02
        /// </remarks>
        void PublicNoteRemoved();
    }
}
