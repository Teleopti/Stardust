using System;
using System.Windows.Forms;

namespace Teleopti.Messaging.Chat
{
    static class Program
    {
        private static ChatController _controller;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ChatForm form = GetForm();
            Application.Run(form);
        }

        private static ChatForm GetForm()
        {
            ChatForm form = new ChatForm();
            ChatModel model = new ChatModel();
            _controller = new ChatController(model, form);
            return form;
        }
    }
}
