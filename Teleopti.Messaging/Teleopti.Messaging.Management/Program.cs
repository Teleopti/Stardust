using System;
using System.Windows.Forms;
using Teleopti.Messaging.Management.Controllers;
using Teleopti.Messaging.Management.Model;
using Teleopti.Messaging.Management.Views;

namespace Teleopti.Messaging.Management
{
    static class Program
    {
        private static ManagementView _view;
        private static ManagementModel _model;
        private static ManagementViewController _controller;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _view = new ManagementView();
            _model = new ManagementModel();
            _controller = new ManagementViewController(_view, _model);
            _model.Controller = _controller;
            Application.Run(_view);
        }
    }
}
