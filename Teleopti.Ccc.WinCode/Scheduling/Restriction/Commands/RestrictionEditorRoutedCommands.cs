using System.Collections.Generic;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction.Commands
{
    public static class RestrictionEditorRoutedCommands
    {
        private static readonly Dictionary<CommandId, RoutedUICommand> Commands = new Dictionary<CommandId, RoutedUICommand>();

        private enum CommandId
        {
            AddPreferenceRestriction,
            AddStudentAvailability,
            UpdateAllRestrictions
        }

        public static RoutedUICommand AddPreferenceRestriction
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddPreferenceRestriction, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddPreferenceRestriction, "AddPreferenceRestriction", typeof(RestrictionEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.AddPreferenceRestriction, cmd);
                    }

                }
                return cmd;
            }

        }

        public static RoutedUICommand AddStudentAvailability
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddStudentAvailability, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddStudentAvailability, "AddStudentAvailability", typeof(RestrictionEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.AddStudentAvailability, cmd);
                    }
                }
                return cmd;
            }
        }

        public static RoutedUICommand UpdateAllRestrictions
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.UpdateAllRestrictions, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.U, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.Update, "UpdateAllRestrictions", typeof(RestrictionEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.UpdateAllRestrictions, cmd);
                    }
                }
                return cmd;
            }
        }
    }

}
