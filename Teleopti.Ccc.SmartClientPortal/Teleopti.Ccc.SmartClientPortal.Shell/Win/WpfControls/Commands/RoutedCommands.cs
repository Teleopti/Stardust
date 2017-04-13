using System.Collections.Generic;
using System.Windows.Input;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Commands
{
	public static class RoutedCommands
	{
		private static readonly Dictionary<CommandId, RoutedUICommand> Commands =
			new Dictionary<CommandId, RoutedUICommand>();

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

		public static RoutedUICommand Filter
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.Filter, out cmd))
					{

						cmd = new RoutedUICommand(UserTexts.Resources.Filter, "Filter", typeof (RoutedCommands), null);
						Commands.Add(CommandId.Filter, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand Next
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.Next, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Right, ModifierKeys.None)};
						cmd = new RoutedUICommand("xxNext", "Next", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.Next, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand Previous
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.Previous, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Left, ModifierKeys.None)};
						cmd = new RoutedUICommand("xxPrevious", "Previous", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.Previous, cmd);
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
						var inputs = new InputGestureCollection {new KeyGesture(Key.Up, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.MoveUp, "MoveUp", typeof (RoutedCommands), inputs);
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
						var inputs = new InputGestureCollection {new KeyGesture(Key.Down, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.MoveDown, "MoveDown", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.MoveDown, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand Remove
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.Remove, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Delete)};
						cmd = new RoutedUICommand(UserTexts.Resources.Delete, "Remove", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.Remove, cmd);
					}
				}
				return cmd;
			}
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
						var inputs = new InputGestureCollection {new KeyGesture(Key.P, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.AddPersonalActivityThreeDots, "AddPersonalShift",
						                          typeof (RoutedCommands), inputs);
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
						var inputs = new InputGestureCollection {new KeyGesture(Key.M, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.AddActivityThreeDots, "AddMainShift", typeof (RoutedCommands),
						                          inputs);
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
						var inputs = new InputGestureCollection {new KeyGesture(Key.M, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.AddActivityThreeDots, "AddMainLayer", typeof (RoutedCommands),
						                          inputs);
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
						var inputs = new InputGestureCollection {new KeyGesture(Key.A, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.AddAbsenceThreeDots, "AddAbsenceLayer", typeof (RoutedCommands),
						                          inputs);
						Commands.Add(CommandId.AddAbsenceLayer, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand MoveLeft
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.MoveLeft, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Left, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.Back, "MoveLeft", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.MoveLeft, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand MoveRight
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.MoveRight, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Right, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.Forward, "MoveRight", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.MoveRight, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand NextAgentDayInformation
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.NextAgentDayInformation, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Right, ModifierKeys.Shift)};
						cmd = new RoutedUICommand("xxNextAgentDayInformation", "NextAgentDayInformation", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.NextAgentDayInformation, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand PreviousAgentDayInformation
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.PreviousAgentDayInformation, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Right, ModifierKeys.Shift)};
						cmd = new RoutedUICommand("xxPreviousAgentDayInformation", "PreviousAgentDayInformation", typeof (RoutedCommands),
						                          inputs);
						Commands.Add(CommandId.PreviousAgentDayInformation, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand TriggerUpdate
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.TriggerUpdate, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.E, ModifierKeys.Alt)};
						cmd = new RoutedUICommand("xxUpdate", "TriggerUpdate", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.TriggerUpdate, cmd);
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
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.ToggleAutoUpdate, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.E, ModifierKeys.Alt)};
						cmd = new RoutedUICommand("xxAuto", "ToggleAutoUpdate", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.ToggleAutoUpdate, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand SetOpacity
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.SetOpacity, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.E, ModifierKeys.Alt)};
						cmd = new RoutedUICommand("xxSetOpacity", "SetOpacity", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.SetOpacity, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand SetLayerType
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.SetLayerType, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.S, ModifierKeys.Alt)};
						cmd = new RoutedUICommand("xxSetLayerType", "SetLayerType", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.SetLayerType, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand ClipAbsence
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.ClipAbsence, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.C, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.ClipAbsence, "ClipAbsence", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.ClipAbsence, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand ChangeDateTime
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.ChangeDateTime, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.C, ModifierKeys.Alt)};
						cmd = new RoutedUICommand("xxChangeDateTime", "ChangeDateTime", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.ChangeDateTime, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand ChangeContent
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.ChangeContent, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Up, ModifierKeys.Alt)};
						cmd = new RoutedUICommand("xxChangeContent", "ChangeContent", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.ChangeContent, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand ChangeInterval
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.ChangeInterval, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.W, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.ChangeInterval, "ChangeInterval", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.ChangeInterval, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand AddPersonTimeActivity
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.AddPersonTimeActivity, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.M, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.AddPersonTimeActivity, "AddPersonTimeActivity",
						                          typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.AddPersonTimeActivity, cmd);
					}
				}
				return cmd;
			}
		}

		public static RoutedUICommand ForceUpdate
		{
			get
			{
				RoutedUICommand cmd;
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.ForceUpdate, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.Enter)};
						cmd = new RoutedUICommand(UserTexts.Resources.Update, "ForceUpdate", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.ForceUpdate, cmd);
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
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.AddTimeActivity, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.O, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.AddOvertime, "AddTimeActivity", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.AddTimeActivity, cmd);
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
						var inputs = new InputGestureCollection {new KeyGesture(Key.E, ModifierKeys.Control)};
						cmd = new RoutedUICommand(UserTexts.Resources.EditMeeting, "EditMeeting", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.EditMeeting, cmd);
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
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.FitToAbsence, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.F, ModifierKeys.Control)};
						cmd = new RoutedUICommand(UserTexts.Resources.ClipAbsence, "FitToAbsence", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.FitToAbsence, cmd);
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
				lock (Commands)
				{
					if (!Commands.TryGetValue(CommandId.LaunchUndoRedo, out cmd))
					{
						var inputs = new InputGestureCollection {new KeyGesture(Key.U, ModifierKeys.Alt)};
						cmd = new RoutedUICommand(UserTexts.Resources.Undo, "LaunchUndoRedo", typeof (RoutedCommands), inputs);
						Commands.Add(CommandId.LaunchUndoRedo, cmd);
					}
				}
				return cmd;
			}
		}
	}
}
