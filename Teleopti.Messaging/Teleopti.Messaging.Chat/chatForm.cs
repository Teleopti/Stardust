using System.Windows.Forms;

namespace Teleopti.Messaging.Chat
{
    public partial class ChatForm : Form
    {
        public ChatForm()
        {
            InitializeComponent();
        }

        public TextBox TextBoxMainWindow
        {
            get { return textBoxMainWindow; }
        }

        public TextBox TextBoxChat
        {
            get { return textBoxChat; }
        }


        public Button ButtonSend
        {
            get { return buttonSend; }
        }




    }
}
