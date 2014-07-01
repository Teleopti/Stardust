using System.Windows.Input;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
    public class EditorCutPasteHandler : BaseCutPasteHandler
    {
        public override void Copy()
        {
            var focusedElement = Keyboard.FocusedElement;
            if (focusedElement != null)
            {
                ApplicationCommands.Copy.Execute(null, focusedElement);
            }
        }

        public override void Paste()
        {
            var focusedElement = Keyboard.FocusedElement;
            if (focusedElement != null)
            {
                ApplicationCommands.Paste.Execute(null, focusedElement);
            }
        }

        public override void CopySpecial()
        {
            Copy();
        }

        public override void PasteSpecial()
        {
            Paste();
        }

        public override void Delete()
        {
            var focusedElement = Keyboard.FocusedElement;
            if (focusedElement != null)
            {
                ApplicationCommands.Delete.Execute(null, focusedElement);
            }
        }

        public override void DeleteSpecial()
        {
            Delete();
        }

        public override void Cut()
        {
            var focusedElement = Keyboard.FocusedElement;
            if (focusedElement != null)
            {
                ApplicationCommands.Cut.Execute(null, focusedElement);
            }
        }

        public override void CutSpecial()
        {
            Cut();
        }
    }
}