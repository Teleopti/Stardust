using System.Collections.Generic;
using System.Windows.Input;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Teleopti.Ccc.WpfControls.Commands")]
namespace Teleopti.Ccc.WpfControls.Commands
{



    /// <summary>
    /// RoutedCommands for shifteditor
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-02-18
    /// </remarks>
    public static class RoutedCommands
    {
        private static readonly Dictionary<CommandId, RoutedUICommand> _commands = new Dictionary<CommandId, RoutedUICommand>();
        private enum CommandId
        {
            MoveUp,
            MoveDown,
            Remove,
            AddMainShift,
            AddPersonalShift,
            AddMainLayer,
            AddAbsenceLayer,
            Filter,
            MoveLeft,
            MoveRight,
            NextAgentDayInformation,
            PreviousAgentDayInformation,
            TriggerUpdate,
            ToggleAutoUpdate,
            SetOpacity,
            SetLayerType,
            ClipAbsence,
            ChangeDateTime,
            ChangeContent,
            ChangeInterval,
            AddPersonTimeActivity,
            Next,
            Previous,
            ForceUpdate,
            AddTimeActivity,
            EditMeeting,
            FitToAbsence,
            LaunchUndoRedo
        }


        /// <summary>
        /// Sets the filter.
        /// </summary>
        /// <value>The filter.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-07-08    
        /// /// </remarks>
        public static RoutedUICommand Filter
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.Filter, out cmd))
                    {
                       
                        cmd = new RoutedUICommand(UserTexts.Resources.Filter, "Filter", typeof(RoutedCommands), null);
                        _commands.Add(CommandId.Filter, cmd);
                    }

                }
                return cmd;
            }

        }

        /// <summary>
        /// Gets next.
        /// </summary>
        /// <value>The next.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-07-07    
        /// /// </remarks>
        public static RoutedUICommand Next
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.Next, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Right, ModifierKeys.None) };
                        cmd = new RoutedUICommand("xxNext", "Next", typeof(RoutedCommands), inputs);
                        _commands.Add(CommandId.Next, cmd);
                    }

                }
                return cmd;
            }

        }

        /// <summary>
        /// Gets previous.
        /// </summary>
        /// <value>The previous.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-07-07    
        /// /// </remarks>
        public static RoutedUICommand Previous
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.Previous, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Left, ModifierKeys.None) };
                        cmd = new RoutedUICommand("xxPrevious", "Previous", typeof(RoutedCommands), inputs);
                        _commands.Add(CommandId.Previous, cmd);
                    }

                }
                return cmd;
            }

        }

        /// <summary>
        /// Gets the move up layer.
        /// </summary>
        /// <value>The move up layer.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-19
        /// </remarks>
        public static RoutedUICommand MoveUp
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.MoveUp, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Up, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.MoveUp, "MoveUp", typeof(RoutedCommands), inputs);
                        _commands.Add(CommandId.MoveUp, cmd);
                    }
                
                }
                return cmd;
            }
        
        }

        /// <summary>
        /// Gets the move down layer.
        /// </summary>
        /// <value>The move down layer.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-19
        /// </remarks>
        public static RoutedUICommand MoveDown
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.MoveDown, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Down, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.MoveDown, "MoveDown", typeof(RoutedCommands), inputs);
                        _commands.Add(CommandId.MoveDown, cmd);
                    }

                }
                return cmd;
            }

        }

        /// <summary>
        /// Gets the remove.
        /// </summary>
        /// <value>The remove.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-19
        /// </remarks>
        public static RoutedUICommand Remove
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.Remove, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Delete) };
                        cmd = new RoutedUICommand(UserTexts.Resources.Delete, "Remove", typeof(RoutedCommands), inputs);
                        _commands.Add(CommandId.Remove, cmd);
                        
                    }

                }
                return cmd;
            }

        }

        /// <summary>
         /// Gets the add personal shift.
         /// </summary>
         /// <value>The add personal shift.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-02-26
         /// </remarks>
        public static RoutedUICommand AddPersonalShift
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.AddPersonalShift, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.AddPersonalActivityThreeDots, "AddPersonalShift", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.AddPersonalShift, cmd);
                     }

                 }
                 return cmd;
             }

         }

        /// <summary>
         /// Gets the add main shift.
         /// </summary>
         /// <value>The add main shift.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-02-26
         /// </remarks>
        public static RoutedUICommand AddMainShift
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.AddMainShift, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.AddActivityThreeDots, "AddMainShift", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.AddMainShift, cmd);
                     }

                 }
                 return cmd;
             }

         }

        /// <summary>
         /// Gets the add main shift layer.
         /// </summary>
         /// <value>The add main shift layer.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-02-19
         /// </remarks> 
        public static RoutedUICommand AddMainLayer
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.AddMainLayer, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddActivityThreeDots, "AddMainLayer", typeof(RoutedCommands), inputs);
                        _commands.Add(CommandId.AddMainLayer, cmd);
                    }

                }
                return cmd;
            }

        }

         /// <summary>
         /// Gets the add absence layer.
         /// </summary>
         /// <value>The add absence layer.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-02-19
         /// </remarks>
         public static RoutedUICommand AddAbsenceLayer
        {
            get
            {
                RoutedUICommand cmd;
                lock (_commands)
                {
                    if (!_commands.TryGetValue(CommandId.AddAbsenceLayer, out cmd))
                    {
                        InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.A, ModifierKeys.Alt) };
                        cmd = new RoutedUICommand(UserTexts.Resources.AddAbsenceThreeDots, "AddAbsenceLayer", typeof(RoutedCommands), inputs);
                        _commands.Add(CommandId.AddAbsenceLayer, cmd);
                    }

                }
                return cmd;
            }

        }

      
         /// <summary>
         /// Gets the move  left.
         /// Used for moving shifts with keyboard
         /// </summary>
         /// <value>The move shift left.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-02-27
         /// </remarks>
         public static RoutedUICommand MoveLeft
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.MoveLeft, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Left, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.Back, "MoveLeft", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.MoveLeft, cmd);
                     }

                 }
                 return cmd;
             }

         }

         /// <summary>
         /// Gets the move  right.
         /// Used for moving shifts with keyboard
         /// </summary>
         /// <value>The move shift right.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-02-27
         /// </remarks>
         public static RoutedUICommand MoveRight
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.MoveRight, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Right, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.Forward, "MoveRight", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.MoveRight, cmd);
                     }

                 }
                 return cmd;
             }

         }
     
         /// <summary>
         /// Gets the NextAgentDayInformation Command
         /// </summary>
         /// <value>The next agent day information.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 3/3/2008
         /// </remarks>
         public static RoutedUICommand NextAgentDayInformation
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.NextAgentDayInformation, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Right, ModifierKeys.Shift) };
                         cmd = new RoutedUICommand("xxNextAgentDayInformation", "NextAgentDayInformation", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.NextAgentDayInformation, cmd);
                     }

                 }
                 return cmd;
             }

         }

         /// <summary>
         /// Gets the PreviousAgentDayInformation Command
         /// </summary>
         /// <value>The previous agent day information.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 3/3/2008
         /// </remarks>
         public static RoutedUICommand PreviousAgentDayInformation
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.PreviousAgentDayInformation, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Right, ModifierKeys.Shift) };
                         cmd = new RoutedUICommand("xxPreviousAgentDayInformation", "PreviousAgentDayInformation", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.PreviousAgentDayInformation, cmd);
                     }

                 }
                 return cmd;
             }

         }

         /// <summary>
         /// Gets the TriggerUpdate Command.
         /// </summary>
         /// <value>The trigger update.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 3/3/2008
         /// </remarks>
         public static RoutedUICommand TriggerUpdate
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.TriggerUpdate, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand("xxUpdate", "TriggerUpdate", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.TriggerUpdate, cmd);
                     }

                 }
                 return cmd;
             }

         }

         /// <summary>
         /// Gets the toggle auto update.
         /// </summary>
         /// <value>The toggle auto update.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-04-14
         /// </remarks>
         public static RoutedUICommand ToggleAutoUpdate
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.ToggleAutoUpdate, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand("xxAuto", "ToggleAutoUpdate", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.ToggleAutoUpdate, cmd);
                     }

                 }
                 return cmd;
             }

         }

         /// <summary>
         /// Gets the set opacity.
         /// </summary>
         /// <value>The set opacity.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-04-14
         /// </remarks>
         public static RoutedUICommand SetOpacity
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.SetOpacity, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand("xxSetOpacity", "SetOpacity", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.SetOpacity, cmd);
                     }

                 }
                 return cmd;
             }

         }

         /// <summary>
         /// Gets the type of the set layer.
         /// </summary>
         /// <value>The type of the set layer.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-05-06
         /// </remarks>
         public static RoutedUICommand SetLayerType
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.SetLayerType, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand("xxSetLayerType", "SetLayerType", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.SetLayerType, cmd);
                     }
                 }
                 return cmd;
             }
         
         }

         /// <summary>
         /// Gets the clip absence.
         /// </summary>
         /// <value>The clip absence.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-04-14
         /// </remarks>
         public static RoutedUICommand ClipAbsence
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.ClipAbsence, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.C, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.ClipAbsence, "ClipAbsence", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.ClipAbsence, cmd);
                     }
                 }
                 return cmd;
             }
         
         }

         /// <summary>
         /// Gets the change start time.
         /// </summary>
         /// <value>The change start time.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-05-06
         /// </remarks>
         public static RoutedUICommand ChangeDateTime
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.ChangeDateTime, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.C, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand("xxChangeDateTime", "ChangeDateTime", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.ChangeDateTime, cmd);
                     }
                 }
                 return cmd;
             }

         }

       

         /// <summary>
         /// Gets the ChangeContent command
         /// </summary>
         /// <value>ChangeContent RoutedUICommand</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-06-09
         /// </remarks>
         public static RoutedUICommand ChangeContent
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.ChangeContent, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Up, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand("xxChangeContent", "ChangeContent", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.ChangeContent, cmd);
                     }

                 }
                 return cmd;
             }
             
         }

         /// <summary>
         /// Gets the ChangeInterval command
         /// </summary>
         /// <value>ChangeContent RoutedUICommand</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-07-01
         /// </remarks>
         public static RoutedUICommand ChangeInterval
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.ChangeInterval, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.W, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.ChangeInterval, "ChangeInterval", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.ChangeInterval, cmd);
                     }
                 }
                 return cmd;
             }
             
         }


         /// <summary>
         /// Gets the AddPersonTimeActivity command.
         /// </summary>
         /// <value>The add person time activity.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-07-08
         /// </remarks>
         public static RoutedUICommand AddPersonTimeActivity
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.AddPersonTimeActivity, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.AddPersonTimeActivity, "AddPersonTimeActivity", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.AddPersonTimeActivity, cmd);
                     }

                 }
                 return cmd;
             }
             
         }

         /// <summary>
         /// Gets the force update.
         /// </summary>
         /// <value>The force update.</value>
         /// <remarks>
         /// Created by: henrika
         /// Created date: 2008-07-23
         /// </remarks>
         public static RoutedUICommand ForceUpdate
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.ForceUpdate, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.Enter) };
                         cmd = new RoutedUICommand(UserTexts.Resources.Update, "ForceUpdate", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.ForceUpdate, cmd);
                     }

                 }
                 return cmd;
             }

         }

        
         public static RoutedUICommand AddTimeActivity
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.AddTimeActivity, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.AddOvertime, "AddTimeActivity", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.AddTimeActivity, cmd);
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
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.EditMeeting, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) };
                         cmd = new RoutedUICommand(UserTexts.Resources.EditMeeting, "EditMeeting", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.EditMeeting, cmd);
                     }

                 }
                 return cmd;
             }

         }

         public static RoutedUICommand FitToAbsence
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.FitToAbsence, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Control) };
                         cmd = new RoutedUICommand(UserTexts.Resources.ClipAbsence, "FitToAbsence", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.FitToAbsence, cmd);
                     }

                 }
                 return cmd;
             }

         }

         public static RoutedUICommand LaunchUndoRedo
         {
             get
             {
                 RoutedUICommand cmd;
                 lock (_commands)
                 {
                     if (!_commands.TryGetValue(CommandId.LaunchUndoRedo, out cmd))
                     {
                         InputGestureCollection inputs = new InputGestureCollection { new KeyGesture(Key.U, ModifierKeys.Alt) };
                         cmd = new RoutedUICommand(UserTexts.Resources.Undo, "LaunchUndoRedo", typeof(RoutedCommands), inputs);
                         _commands.Add(CommandId.LaunchUndoRedo, cmd);
                     }

                 }
                 return cmd;
             }

         }
        
        
    }
}
