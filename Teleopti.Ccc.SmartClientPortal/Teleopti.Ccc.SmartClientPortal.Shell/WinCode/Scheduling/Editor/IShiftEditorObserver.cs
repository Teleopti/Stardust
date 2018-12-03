using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    /// <summary>
    /// Instead of events, an observer can add itself to listen the shifteditor
    /// </summary>
    public interface IShiftEditorObserver
    {
        /// <summary>
        /// Occurs when [shift updated].
        /// </summary>
        void EditorShiftUpdated(IScheduleDay part);

        /// <summary>
        /// Editors the update command executed.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <remarks>
        /// Difference between EditorShiftUpdated is that EditorUpdateCommandExecuted is executed manually
        /// </remarks>
        void EditorUpdateCommandExecuted(IScheduleDay part);
        
        /// <summary>
        /// Occurs when [add activity].
        /// </summary>
		void EditorAddActivity(IScheduleDay part, DateTimePeriod? period, IPayload payload);

		void EditorAddActivity(IScheduleDay part, DateTimePeriod? period);

        /// <summary>
        /// Occurs when [add overtime].
        /// </summary>
        void EditorAddOvertime(IScheduleDay part, DateTimePeriod? period);

        /// <summary>
        /// Occurs when [add absence].
        /// </summary>
        void EditorAddAbsence(IScheduleDay part, DateTimePeriod? period);

        /// <summary>
        /// Occurs when [add personal shift].
        /// </summary>
        void EditorAddPersonalShift(IScheduleDay part, DateTimePeriod? period);


        /// <summary>
        /// Occurs when CommitChanges is executed
        /// </summary>
        void EditorCommitChangesExecuted(IScheduleDay part);


        /// <summary>
        /// Occurs when edit meeting executed.
        /// </summary>
        /// <param name="personMeeting">The person meeting.</param>
        void EditorEditMeetingExecuted(IPersonMeeting personMeeting);

        /// <summary>
        /// Occurs when delete meeting is executed.
        /// </summary>
        /// <param name="personMeeting">The person meeting.</param>
        void EditorDeleteMeetingExecuted(IPersonMeeting personMeeting);

        /// <summary>
        /// Occurs when remove participants from meeting executed.
        /// </summary>
        void EditorRemoveParticipantsFromMeetingExecuted(IPersonMeeting personMeeting);

        /// <summary>
        /// Occurs when create-meeting executed.
        /// </summary>
        void EditorCreateMeetingExecuted(IPersonMeeting personMeeting);

	    void EditorShowLayersExecuted();
    }
}
