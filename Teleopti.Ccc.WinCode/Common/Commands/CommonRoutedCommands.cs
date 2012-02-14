using System.Collections.Generic;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    public static class CommonRoutedCommands
    {
        private static readonly Dictionary<CommandId, RoutedUICommand> _commands = new Dictionary<CommandId, RoutedUICommand>();
        private enum CommandId
        {
            EditMeeting,
            DeleteMeeting,
            RemoveParticipant,
            CreateMeeting,
            MoveStartOneIntervalEarlier,
            MoveStartOneIntervalLater,
            MoveEndOneIntervalEarlier,
            MoveEndOneIntervalLater,
            MovePeriodOneIntervalEarlier,
            MovePeriodOneIntervalLater,
            LoadPasswordPolicy,
            ToggleAutoUpdate,
            ShowDetails
            

        }
        #region commands
        public static RoutedUICommand EditMeeting
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.EditMeeting, out cmd))
                    {

                        cmd = new RoutedUICommand(UserTexts.Resources.EditMeeting, "EditMeeting", typeof(CommonRoutedCommands), null);
                        _commands.Add(CommandId.EditMeeting, cmd);
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
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.DeleteMeeting, out cmd))
                    {

                        cmd = new RoutedUICommand(UserTexts.Resources.Delete, "DeleteMeeting", typeof(CommonRoutedCommands), null); //Change to Delete Meeting??
                        _commands.Add(CommandId.DeleteMeeting, cmd);
                    }

                }
                return cmd;
            }

        }

        public static RoutedUICommand RemoveParticipant
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.RemoveParticipant, out cmd))
                    {

                        cmd = new RoutedUICommand(UserTexts.Resources.RemoveParticipant, "RemoveParticipant", typeof(CommonRoutedCommands), null); 
                        _commands.Add(CommandId.RemoveParticipant, cmd);
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
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.CreateMeeting, out cmd))
                    {

                        cmd = new RoutedUICommand(UserTexts.Resources.CreateMeeting, "CreateMeeting", typeof(CommonRoutedCommands), null);
                        _commands.Add(CommandId.CreateMeeting, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand MoveStartOneIntervalEarlier
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.MoveStartOneIntervalEarlier, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Subtract) };

                        cmd = new RoutedUICommand(UserTexts.Resources.MoveStartOneIntervalEarlier, "MoveStartOneIntervalEarlier", typeof(CommonRoutedCommands), inputs);
                        _commands.Add(CommandId.MoveStartOneIntervalEarlier, cmd);
                    }

                }
                return cmd;
            }
        }
        
        public static RoutedUICommand MoveStartOneIntervalLater
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.MoveStartOneIntervalLater, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Add) };

                        cmd = new RoutedUICommand(UserTexts.Resources.MoveStartOneIntervalLater, "MoveStartOneIntervalLater", typeof(CommonRoutedCommands), inputs);
                        _commands.Add(CommandId.MoveStartOneIntervalLater, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand MoveEndOneIntervalEarlier
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.MoveEndOneIntervalEarlier, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Subtract, ModifierKeys.Shift) };

                        cmd = new RoutedUICommand(UserTexts.Resources.MoveEndOneIntervalEarlier, "MoveEndOneIntervalEarlier", typeof(CommonRoutedCommands), inputs);
                        _commands.Add(CommandId.MoveEndOneIntervalEarlier, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand MoveEndOneIntervalLater
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.MoveEndOneIntervalLater, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Add, ModifierKeys.Shift) };
                        

                        cmd = new RoutedUICommand(UserTexts.Resources.MoveEndOneIntervalLater, "MoveEndOneIntervalLater", typeof(CommonRoutedCommands), inputs);
                        _commands.Add(CommandId.MoveEndOneIntervalLater, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand MovePeriodOneIntervalEarlier
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.MovePeriodOneIntervalEarlier, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Subtract, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.MovePeriodOneIntervalEarlier, "MovePeriodOneIntervalEarlier", typeof(CommonRoutedCommands), inputs);
                        _commands.Add(CommandId.MovePeriodOneIntervalEarlier, cmd);
                    }

                }
                return cmd;
            }
        }

        public static RoutedUICommand MovePeriodOneIntervalLater
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.MovePeriodOneIntervalLater, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Add, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.MovePeriodOneIntervalLater, "MovePeriodOneIntervalLater", typeof(CommonRoutedCommands), inputs);
                        _commands.Add(CommandId.MovePeriodOneIntervalLater, cmd);
                    }

                }
                return cmd;
            }
        }

      
        public static RoutedUICommand LoadPasswordPolicy
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.LoadPasswordPolicy, out cmd))
                    {

                        cmd = new RoutedUICommand(UserTexts.Resources.Load, "LoadPasswordPolicy", typeof(CommonRoutedCommands), null);
                        _commands.Add(CommandId.LoadPasswordPolicy, cmd);
                    }

                }
                return cmd;
            }
        }

       
        public static RoutedUICommand ToggleAutoUpdate
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.ToggleAutoUpdate, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.A, ModifierKeys.Alt) };

                        cmd = new RoutedUICommand(UserTexts.Resources.AutomaticUpdate, "ToggleAutoUpdate", typeof(CommonRoutedCommands), inputs);
                        _commands.Add(CommandId.ToggleAutoUpdate, cmd);
                    }

                }
                return cmd;
            }
        }

        //ToggleAdvanced
        public static RoutedUICommand ShowDetails
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.ShowDetails, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Alt) };

                        cmd = new RoutedUICommand(UserTexts.Resources.ShowDetails, "ShowDetails", typeof(CommonRoutedCommands), inputs);
                        _commands.Add(CommandId.ShowDetails, cmd);
                    }

                }
                return cmd;
            }
        }

        #endregion //commands
    }
}
