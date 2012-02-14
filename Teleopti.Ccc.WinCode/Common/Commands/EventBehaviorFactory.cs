using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    /// <summary>
    /// For binding commands to events
    /// Create a Behavior for a control and add use the CreateCommandExecutionEventBehavior like:
    /// 
    /// public static readonly DependencyProperty TextChangedCommand =
    /// EventBehaviorFactory.CreateCommandExecutionEventBehavior(TextBox.TextChangedEvent, "TextChangedCommand", typeof(TextBoxBehavior));
    /// 
    /// Sample usage in the XAML:
    /// <UserControl TextBoxBehavior.TextChangedCommand="{Binding MyViewModel.MyCommandModel.MyCommand}>
    ///     <TextBox Text="Changing this text will trigger MyCommand on the ViewModel"/>
    /// </UserControl>
    /// </summary>
    public static class EventBehaviorFactory
    {
        public static DependencyProperty CreateCommandExecutionEventBehavior(RoutedEvent routedEvent, string propertyName, Type ownerType)
        {
            DependencyProperty property = DependencyProperty.RegisterAttached(propertyName, typeof(ICommand), ownerType,
                                                               new PropertyMetadata(null,
                                                                   new ExecuteCommandOnRoutedEventBehavior(routedEvent).PropertyChangedHandler));

            return property;
        }

        /// <summary>
        /// An internal class to handle listening for an event and executing a command,
        /// when a Command is assigned to a particular DependencyProperty
        /// </summary>
        private class ExecuteCommandOnRoutedEventBehavior : ExecuteCommandBehavior
        {
            private readonly RoutedEvent _routedEvent;

            public ExecuteCommandOnRoutedEventBehavior(RoutedEvent routedEvent)
            {
                _routedEvent = routedEvent;
            }

            /// <summary>
            /// Handles attaching or Detaching Event handlers when a Command is assigned or unassigned
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="oldValue"></param>
            /// <param name="newValue"></param>
            protected override void AdjustEventHandlers(DependencyObject sender, object oldValue, object newValue)
            {
                UIElement element = sender as UIElement;
                if (element != null)
                {
                    if (oldValue != null) element.RemoveHandler(_routedEvent, new RoutedEventHandler(EventHandler));
                    if (newValue != null) element.AddHandler(_routedEvent, new RoutedEventHandler(EventHandler));
                }
            }

            protected void EventHandler(object sender, RoutedEventArgs e)
            {
                HandleEvent(sender, e);
            }
        }

        internal abstract class ExecuteCommandBehavior : INotifyPropertyChanged
        {
            protected DependencyProperty _property;
            protected abstract void AdjustEventHandlers(DependencyObject sender, object oldValue, object newValue);

            protected void HandleEvent(object sender, EventArgs e)
            {
                DependencyObject dp = sender as DependencyObject;
                if (dp != null)
                {
                    ICommand command = dp.GetValue(_property) as ICommand;
                    if (command != null && command.CanExecute(e)) command.Execute(e);
                }
            }

            /// <summary>
            /// Listens for a change in the DependencyProperty that we are assigned to, and
            /// adjusts the EventHandlers accordingly
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void PropertyChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {

                if (_property == null)
                {
                    _property = e.Property;
                }

                object oldValue = e.OldValue;
                object newValue = e.NewValue;

                AdjustEventHandlers(sender, oldValue, newValue);
            }

            //Just for fixing mem leaks in WPF
#pragma warning disable 0067
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067
        }
    }
}
