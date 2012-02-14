using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public partial class InputBox : BaseRibbonForm
    {
        private InputBoxValidatingHandler _validator;

        public InputBox()
        {
            InitializeComponent();
            textBox.Focus();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "title"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "prompt"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public static InputBoxResult Show(string prompt, 
                                          string title, 
                                          string defaultResponse, 
                                          InputBoxValidatingHandler validator, 
                                          int xPosition, 
                                          int yPosition)
        {
            using (InputBox form = new InputBox())
            {               
                form.textBox.Text = defaultResponse;
                if (xPosition >= 0 && yPosition >= 0)
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Left = xPosition;
                    form.Top = yPosition;
                }
                form.Validator = validator;

                DialogResult result = form.ShowDialog();
                InputBoxResult retval;
                if (result == DialogResult.OK)
                    retval = new InputBoxResult(InputBoxOperation.Ok, form.textBox.Text);
                else 
                    retval = new InputBoxResult(InputBoxOperation.Cancel, string.Empty);

                return retval;
            }
        }

        
        public static InputBoxResult Show(string prompt, string title, string defaultText, InputBoxValidatingHandler validator)
        {
            return Show(prompt, title, defaultText, validator, -1, -1);
        }


        /// <summary>
        /// Reset the ErrorProvider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void textBoxText_TextChanged(object sender, System.EventArgs e)
        //{
        //    errorProviderText.SetError(textBox, "");
        //}

        /// <summary>
        /// Validate the Text using the Validator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void textBoxText_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    if (Validator != null)
        //    {
        //        InputBoxValidatingEventArgs eventArgs = new InputBoxValidatingEventArgs(textBox.Text, string.Empty, true);
        //        Validator(this, eventArgs);
        //        if (eventArgs.Cancel)
        //        {
        //            e.Cancel = true;
        //            errorProviderText.SetError(textBox, eventArgs.Message);
        //        }
        //    }
        //}

        protected InputBoxValidatingHandler Validator
        {
            get
            {
                return (this._validator);
            }
            set
            {
                this._validator = value;
            }
        }
	}

    public enum InputBoxOperation : int
    {
        Ok,
        Cancel,
    }

	/// <summary>
	/// Class used to store the result of an InputBox.Show message.
	/// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct InputBoxResult : IEquatable<InputBoxResult>
    {
	    private readonly InputBoxOperation _button;
        private readonly string _text;

        public InputBoxResult(InputBoxOperation button, string text)
        {
            _button = button;
            _text = text; 
        }

	    public InputBoxOperation Button
	    {
	        get { return _button; }
	    }

	    public string Text
	    {
	        get { return _text; }
	    }

        #region IEquatable<InputBoxResult> Members

        public bool Equals(InputBoxResult other)
        {
            bool matched = false;
            if ((_button == other.Button) && (_text == other.Text))
                matched = true;
            return matched;
        }

        #endregion

        #region IEquatable<InputBoxResult> Members

        //bool IPreferenceRestriction.Equals(InputBoxResult other)
        //{
        //    return Equals(other);
        //}

        #endregion
    }

	/// <summary>
	/// EventArgs used to Validate an InputBox
	/// </summary>
	public class InputBoxValidatingEventArgs : EventArgs 
    {
		private string _text;
        private string _message;
        private bool _cancel;

        public InputBoxValidatingEventArgs(string text, string message, bool cancel)
        {
            _text = text;
            _message = message;
            _cancel = cancel;
        }

	    public string Text
	    {
	        get { return _text; }
            set { _text = value; }
	    }

	    public string Message
	    {
	        get { return _message; }
            set { _message = value; }
	    }

	    public bool Cancel
	    {
	        get { return _cancel; }
            set { _cancel = value; }
	    }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
    public delegate void InputBoxValidatingHandler(object sender, InputBoxValidatingEventArgs e);

}
