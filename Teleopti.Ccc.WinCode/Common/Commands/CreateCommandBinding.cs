///Henrik 2010-06-03 CreateCommandBinding info:
///Helpers for connecting Commands and CommandModels directly to the ViewModel instead of the codebehind
///There is a lot of different approaches on this problem
///This one is using Dan Creviers CommandModel 
///Easiest usage is just to set the target (that would probably be the viewmodel) like:
///CreateCommandBinding.Target={Binding}
///This will set up all the CommandModels on the viewmodel 
///more info below.....
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    
    public static class CreateCommandBinding
    {
        //For binding all existsing CommandModelProperties directly to the (datacontext) viewmodel
        //Example: If you want all of your viewmodels commands to be handled from the gui: 
        //CreateCommandBinding.Target={Binding}
        //This will hook up all public CommandModels
        #region target
        public static object GetTarget(DependencyObject obj)
        {
            return obj.GetValue(TargetProperty);
        }

        public static void SetTarget(DependencyObject obj, object value)
        {
            obj.SetValue(TargetProperty, value);
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.RegisterAttached("Target", 
            typeof(object), 
            typeof(CreateCommandBinding), 
            new UIPropertyMetadata(new PropertyChangedCallback(OnTargetInvalidated)));

        private static void OnTargetInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if(element!=null && e.NewValue!=null)
            {
                // e.NewValue
                List<PropertyInfo> commandModelProperties =
                    e.NewValue.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(
                        p => p.PropertyType.IsAssignableFrom(typeof (CommandModel))).ToList();

                foreach (PropertyInfo propertyInfo in commandModelProperties)
                {
                        var commandModel = propertyInfo.GetValue(e.NewValue, null);
                    CommandModel test = commandModel as CommandModel;
                    if (test!=null)
                    {
                        element.CommandBindings.Add(new CommandBinding(test.Command, test.OnExecute, test.OnQueryEnabled));
                    }
                }
            }
        }

        #endregion //target

        //For binding a Command to a viewmodel instead of the code behind
        //Example: If you want to handle a command  you can write:
        //<Grid CreateCommandBinding.Command={Binding Path=MyCommandModel.Command}/>
        //This will make the DataContext (ViewModel) handle the command on that CommandModel
        #region command
        /// <summary>
        /// Creates a commandbinding to a CommandModel in XAML
        /// by using an attatched dependencyproperty
        /// </summary>
        /// <remarks>
        /// Example of using:
        ///  <Button Command="{Binding AddCommandModel.Command}"
        ///          CreateCommandBinding.Command="{Binding AddCommandModel}"/>
        /// Created by: henrika
        /// Created date: 2008-09-16
        /// </remarks>
        public static CommandModel GetCommand(DependencyObject obj)
        {
            return (CommandModel)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, CommandModel value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(CommandModel), typeof(CreateCommandBinding), new UIPropertyMetadata(new PropertyChangedCallback(OnCommandInvalidated)));

        //Adds the command to the UIElements CommandBindings, remove if it already exists
        private static void OnCommandInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandModel command = e.NewValue as CommandModel;
            IList<CommandBinding> toRemove = new List<CommandBinding>();
            UIElement element = d as UIElement;
            if (element != null && command != null)
            {
                foreach (CommandBinding binding in element.CommandBindings)
                {
                    if (binding.Command == command.Command) toRemove.Add(binding);
                }
                foreach (CommandBinding commandBinding in toRemove) element.CommandBindings.Remove(commandBinding);

                element.CommandBindings.Add(new CommandBinding(command.Command, command.OnExecute,
                                                               command.OnQueryEnabled));
            }
        }
        #endregion command

        //For binding a Command to a viewmodel, it also sets the content to the commandmodels text, and the tooltip
        //Example: If you want to handle a CommandModel you can write:
        //<Button CreateCommandBinding.CommandModel={Binding Path=MyCommandModel} />
        //This will set up the handling of the command, not the commandsource
        #region commandmodel
        public static CommandModel GetCommandModel(DependencyObject obj)
        {
            return (CommandModel)obj.GetValue(CommandModelProperty);
        }

        public static void SetCommandModel(DependencyObject obj, CommandModel value)
        {
            obj.SetValue(CommandModelProperty, value);
        }

        public static readonly DependencyProperty CommandModelProperty =
            DependencyProperty.RegisterAttached("CommandModel", typeof(CommandModel), typeof(CreateCommandBinding), new UIPropertyMetadata(new PropertyChangedCallback(OnCommandModelInvalidated)));

       
        private static void OnCommandModelInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandModel commandModel = e.NewValue as CommandModel;
            ContentControl element = d as ContentControl;
            if (element != null && commandModel != null)
            {
                element.ToolTip = commandModel.DescriptionText;
                element.Content = commandModel.Text;
            }
            OnCommandInvalidated(d, e); //Set the CommandBinding
            
            //It would be nice if we could set the Command here as well, like:
            //d.SetValue(CommandProperty,commandModel.Command)
        }

        #endregion //commandmodel

        //For binding multiple commands at once, they need to be exposed as a IEnumerable
        //Example:
        //<Window CreateCommandBinding.Commands={Binding Path=AllMyCommandModels}
        //This will hook up all the viewmodels commandmodels that are exposed in that list
        #region commands

        public static IEnumerable<CommandModel> GetCommands(DependencyObject obj)
        {
            return (IEnumerable<CommandModel>)obj.GetValue(CommandsProperty);
        }

        public static void SetCommands(DependencyObject obj, IEnumerable<CommandModel> value)
        {
            obj.SetValue(CommandsProperty, value);
        }

        public static readonly DependencyProperty CommandsProperty =
            DependencyProperty.RegisterAttached("Commands",
            typeof(IEnumerable<CommandModel>), 
            typeof(CreateCommandBinding), 
            new UIPropertyMetadata(new PropertyChangedCallback(OnCommandsInvalidated)));


        private static void OnCommandsInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
         
            IList<CommandBinding> toRemove = new List<CommandBinding>();
            var newCommandModels = e.NewValue as IEnumerable<CommandModel>;
            FrameworkElement targetElement = d as FrameworkElement;

            foreach (CommandBinding binding in targetElement.CommandBindings)
            {
                foreach (var model in newCommandModels)
                {
                    if (model.Command == binding.Command)
                    {
                        toRemove.Add(binding);
                    }
                }
            }
            //Remove all the old commandbindings that are included 
            foreach (CommandBinding commandBinding in toRemove) targetElement.CommandBindings.Remove(commandBinding);
                              
            foreach (var model in newCommandModels)
            {
               targetElement.CommandBindings.Add(new CommandBinding(model.Command, model.OnExecute,model.OnQueryEnabled));
            } 

           
        }

        #endregion commands

        //For executing a command on load.
        //Can execute when the element is loaded
        //Example:
        //<StackPanel CreateCommandBinding.ExecuteCommandModel="{Binding Path=MyCommandModel}"/>
        //Will execute the Command when loaded first time
        //<Stackpanel CreateCommandBinding.ExecuteCommandModel="{Binding Path=MyCommandModel}" LoadOnlyOnce="false"/>
        //will execute the command everytime the stackpanel is loaded.
        #region execute command
        
        public static bool GetLoadOnlyOnce(DependencyObject obj)
        {
            return (bool)obj.GetValue(LoadOnlyOnceProperty);
        }

        public static void SetLoadOnlyOnce(DependencyObject obj, bool value)
        {
            obj.SetValue(LoadOnlyOnceProperty, value);
        }

        public static readonly DependencyProperty LoadOnlyOnceProperty =
            DependencyProperty.RegisterAttached("LoadOnlyOnce", typeof(bool), typeof(CreateCommandBinding), new UIPropertyMetadata(true, OnLoadOnlyOncePropertyInvalidated));

        private static void OnLoadOnlyOncePropertyInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element != null)
            {
                element.Loaded += ElementLoaded;
            }
        }


        public static CommandModel GetExecuteCommandModel(DependencyObject obj)
        {
            return (CommandModel)obj.GetValue(ExecuteCommandModelProperty);
        }

        public static void SetExecuteCommandModel(DependencyObject obj, CommandModel value)
        {
            obj.SetValue(ExecuteCommandModelProperty, value);
        }

        public static readonly DependencyProperty ExecuteCommandModelProperty =
           DependencyProperty.RegisterAttached("ExecuteCommandModel", 
           typeof(CommandModel), 
           typeof(CreateCommandBinding), 
           new UIPropertyMetadata(new PropertyChangedCallback(OnCommandModelInvalidatedAndExecute)));

        private static void OnCommandModelInvalidatedAndExecute(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnCommandInvalidated(d, e);
          
            CommandModel commandModel = e.NewValue as CommandModel;
            FrameworkElement element = d as FrameworkElement;
            
            if (commandModel!=null && element!=null)
            {
                SetCommandModel(element,commandModel);
                element.Loaded += ElementLoaded;
            }
        }

        static void ElementLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement) sender;
            var commandModel = GetCommandModel(element);
            commandModel.Command.Execute(null,element);
            if (GetLoadOnlyOnce(element)) element.Loaded -= ElementLoaded;
        }

        #endregion 
    }
}
