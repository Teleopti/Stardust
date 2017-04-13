using System.ComponentModel;
using System.Windows.Input;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    /// <summary>
    /// Class for Holding a RoutedCommand and defines it owns logic for
    /// CanExecute and OnExecute so commands can be decoupled from GUI
    /// </summary>
    /// <remarks>
    /// Use the CreateCommandBinding to connect the CommandModel directly to the ViewModel.
    /// TextProperty used as a description of the command (text for button, menu etc.)
    /// DescriptionText used as a more descriptive text, like tooltip etc. If not declared, it returns the TextProperty
    /// Created by: henrika
    /// Created date: 2008-12-05
    /// </remarks>
    public abstract class CommandModel : INotifyPropertyChanged
    {
        private  RoutedUICommand _routedCommand;
        
        protected CommandModel (RoutedUICommand command)
        {
            _routedCommand = command;
        }

        protected CommandModel()
        {
            _routedCommand = new RoutedUICommand();
        }

        public abstract string Text
        {
            get;
        }

        public virtual string DescriptionText
        {
            get { return Text; }
        }

        public RoutedUICommand Command
        {
            get { return _routedCommand; }
            protected set { _routedCommand = value;}
        }

        /// <summary>
        /// Do not call base when overriding
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Marked as handled by default for performance, otherwise the Command will continue to travel up the visual tree
        /// Created by: henrika
        /// Created date: 2008-09-16
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public virtual void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

      
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public abstract void OnExecute(object sender, ExecutedRoutedEventArgs e);

        //Just for fixing mem leaks in WPF
#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067
    }

   
    
}
