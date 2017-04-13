using System.Windows.Controls;
using System.Windows.Data;

namespace Teleopti.Ccc.Win.WpfControls.Controls.DateTimePeriodControl
{
    /// <summary>
    /// Interaction logic for DateTimePeriodEditBox.xaml
    /// </summary>
    public partial class EditableDateTimePeriodView : UserControl
    {
        public EditableDateTimePeriodView()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Handles the LostFocus event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// this is because wpf databinding will not change the text since the cahnges came from the user
        /// by default this will not retrigger, that would cause a race condition.
        /// We have to tell the gui to re-evalute its binding
        /// Also, before going deep on this one, learn the difference between BindingExpression and Binding (and base/multi etc)!
        /// this could probably be moved to wincode later
        /// Created by: henrika
        /// Created date: 2010-07-08
        /// Note to myself 2010-07-09. I dont think its a problem that it only occurs when x_LostFocus, if a databinding updates on propertychange, i dont want to re-read from viewmodel! Infact, it should probably only happen on lostfocus (but mayby not just for textbox)
        /// </remarks>
        private void TextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBox t = e.OriginalSource as TextBox;
            if(t!=null)
            {
                var bindingexpression = BindingOperations.GetBindingExpressionBase(t, TextBox.TextProperty);
                if(bindingexpression!=null)bindingexpression.UpdateTarget();
            
            }
            
        }

       
    }
}