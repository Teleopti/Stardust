namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface INotesAltered
    {
        void NotesAltered();
        bool NotesIsAltered { get; set; }
        void NoteRemoved();
    }
}
