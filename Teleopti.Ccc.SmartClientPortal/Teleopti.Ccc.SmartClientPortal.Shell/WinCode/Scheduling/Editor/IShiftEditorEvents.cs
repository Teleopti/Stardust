using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    /// <summary>
    /// Interface for the events
    /// </summary>
    /// <remarks
    /// Sepearated from the IShiftEditor, the events are only for the view, not the viewmodel
    /// </remarks>
    public interface IShiftEditorEvents
    {

        /// <summary>
        /// Occurs when [commit changes].
        /// </summary>
        event EventHandler<ShiftEditorEventArgs> CommitChanges;
       
        /// <summary>
        /// Occurs when [shift updated].
        /// </summary>
        event EventHandler<ShiftEditorEventArgs> ShiftUpdated;

        /// <summary>
        /// Occurs when [selection changed].
        /// </summary>
        /// <remarks>
        /// The selected layer changes
        /// </remarks>
        event EventHandler<ShiftEditorEventArgs> SelectionChanged;

        /// <summary>
        /// Occurs when [add activity].
        /// </summary>
        event EventHandler<ShiftEditorEventArgs> AddActivity;

        /// <summary>
        /// Occurs when [add overtime].
        /// </summary>
        event EventHandler<ShiftEditorEventArgs> AddOvertime;

        /// <summary>
        /// Occurs when [add absence].
        /// </summary>
        event EventHandler<ShiftEditorEventArgs> AddAbsence;

        /// <summary>
        /// Occurs when [add personal shift].
        /// </summary>
        event EventHandler<ShiftEditorEventArgs> AddPersonalShift;

        /// <summary>
        /// Occurs when [meeting clicked].
        /// </summary>
        event EventHandler<EventArgs> MeetingClicked;

        /// <summary>
        /// Occurs when [edit meeting].
        /// </summary>
        event EventHandler<CustomEventArgs<IPersonMeeting>> EditMeeting;

        /// <summary>
        /// Occurs when [delete meeting].
        /// </summary>
        event EventHandler<CustomEventArgs<IPersonMeeting>> DeleteMeeting;

        /// <summary>
        /// Occurs when [remove participant].
        /// </summary>
        event EventHandler<CustomEventArgs<IPersonMeeting>> RemoveParticipant;

        /// <summary>
        /// Occurs when [create meeting].
        /// </summary>
        event EventHandler<CustomEventArgs<IPersonMeeting>> CreateMeeting;

        /// <summary>
        /// Occurs when [undo].
        /// </summary>
        event EventHandler<EventArgs> Undo;

	    event EventHandler<EventArgs> ShowLayers;

    }
}