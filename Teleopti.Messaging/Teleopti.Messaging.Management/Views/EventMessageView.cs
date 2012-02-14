using System.Windows.Forms;

namespace Teleopti.Messaging.Management.Views
{
    /// <summary>
    /// The Event Message View.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 11/06/2010
    /// </remarks>
    public partial class EventMessageView : Form
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EventMessageView"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/06/2010
        /// </remarks>
        public EventMessageView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the buttonSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/06/2010
        /// </remarks>
        private void buttonSend_Click(object sender, System.EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 11/06/2010
        /// </remarks>
        public string Message
        {
            get { return textBoxEventMessage.Text;  }
        }

    }
}
