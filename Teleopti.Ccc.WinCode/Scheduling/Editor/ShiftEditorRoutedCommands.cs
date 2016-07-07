using System.Collections.Generic;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Scheduling.Editor
{
    public static class ShiftEditorRoutedCommands
    {
        private static readonly Dictionary<CommandId, RoutedUICommand> Commands = new Dictionary<CommandId, RoutedUICommand>();
        
        private enum CommandId
        {
            AddMainShift,
            AddPersonalShift,
            AddMainLayer,
            AddAbsenceLayer,
            AddOvertime,
            Update,
            Cancel,
            MoveUp,
            MoveDown,
            Delete,
            CommitChanges,
            EditMeeting,
            RemoveParticipant,
            DeleteMeeting,
            CreateMeeting,
            AddActivityWithPeriod,
            AddOvertimeWithPeriod,
            AddAbsenceWithPeriod,
            AddPersonalShiftWithPeriod

        }

        public static RoutedUICommand AddPersonalShift
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddPersonalShift, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddPersonalActivityThreeDots, "AddPersonalShift", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.AddPersonalShift, cmd);
                    }

                }
                return cmd;
            }

        }

        public static RoutedUICommand AddMainShift
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddMainShift, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddMainLayer, "AddMainShift", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.AddMainShift, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand AddMainLayer
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddMainLayer, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddActivityThreeDots, "AddMainLayer", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.AddMainLayer, cmd);
                    }

                }
                return cmd;
            }

        }

        public static RoutedUICommand AddAbsenceLayer
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddAbsenceLayer, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.A, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddAbsenceThreeDots, "AddAbsenceLayer", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.AddAbsenceLayer, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand AddOvertime
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddOvertime, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddOvertime, "AddOvertime", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.AddOvertime, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand Update
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.Update, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.U, ModifierKeys.Control) };
                        cmd = new RoutedUICommand(UserTexts.Resources.Update, "Update", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.Update, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand Cancel
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.Cancel, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.C, ModifierKeys.Control) };
                        cmd = new RoutedUICommand(UserTexts.Resources.Cancel, "Cancel", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.Cancel, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand MoveUp
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.MoveUp, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Up, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.MoveUp, "MoveUp", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.MoveUp, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand MoveDown
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.MoveDown, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Down, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.MoveDown, "MoveDown", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.MoveDown, cmd);
                    }

                }
                return cmd;
            }

        }
        
        public static RoutedUICommand Delete
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.Delete, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Delete) };
                        cmd = new RoutedUICommand(UserTexts.Resources.Delete, "Delete", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.Delete, cmd);
                    }

                }
                return cmd;
            }

        }

        public static RoutedUICommand CommitChanges
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.CommitChanges, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Enter) };
                        cmd = new RoutedUICommand(UserTexts.Resources.Update, "CommitChanges", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.CommitChanges, cmd);
                    }
                }
                return cmd;
            }

        }

        #region meetings
        public static RoutedUICommand RemoveParticipant
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.RemoveParticipant, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Control) };
                        cmd = new RoutedUICommand(UserTexts.Resources.RemoveParticipant, "RemoveParticipant", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.RemoveParticipant, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand DeleteMeeting
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.DeleteMeeting, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.D, ModifierKeys.Control) };
                        cmd = new RoutedUICommand(UserTexts.Resources.DeleteMeeting, "DeleteMeeting", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.DeleteMeeting, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand CreateMeeting
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.CreateMeeting, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.CreateMeeting, "CreateMeeting", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.CreateMeeting, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand EditMeeting
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.EditMeeting, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) };
                        cmd = new RoutedUICommand(UserTexts.Resources.EditMeeting, "EditMeeting", typeof(ShiftEditorRoutedCommands), inputs);
                        Commands.Add(CommandId.EditMeeting, cmd);
                    }

                }
                return cmd;
            }
        }

   


        public static RoutedUICommand AddActivityWithPeriod
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddActivityWithPeriod, out cmd))
                    {

                        cmd = new RoutedUICommand(UserTexts.Resources.AddActivity, "AddActivityWithPeriod", typeof(ShiftEditorRoutedCommands), null);
                        Commands.Add(CommandId.AddActivityWithPeriod, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand AddOvertimeWithPeriod
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddOvertimeWithPeriod, out cmd))
                    {
                        cmd = new RoutedUICommand(UserTexts.Resources.AddOvertime, "AddOvertimeWithPeriod", typeof(ShiftEditorRoutedCommands), null);
                        Commands.Add(CommandId.AddOvertimeWithPeriod, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand AddAbsenceWithPeriod
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddAbsenceWithPeriod, out cmd))
                    {
                        cmd = new RoutedUICommand(UserTexts.Resources.AddAbsence, "AddAbsenceWithPeriod", typeof(ShiftEditorRoutedCommands), null);
                        Commands.Add(CommandId.AddAbsenceWithPeriod, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand AddPersonalShiftWithPeriod
        {
            get
            {
                RoutedUICommand cmd;
                lock (Commands)
                {
                    if (!Commands.TryGetValue(CommandId.AddPersonalShiftWithPeriod, out cmd))
                    {
                        cmd = new RoutedUICommand(UserTexts.Resources.AddPersonalActivityThreeDots, "AddPersonalWithPeriodShift", typeof(ShiftEditorRoutedCommands), null);
                        Commands.Add(CommandId.AddPersonalShiftWithPeriod, cmd);
                    }

                }
                return cmd;
            }
        }

        #endregion
    }
}
