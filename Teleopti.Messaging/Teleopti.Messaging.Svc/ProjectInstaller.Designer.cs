namespace Teleopti.Messaging.Svc
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.teleoptiBrokerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.teleoptiBrokerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // teleoptiBrokerServiceProcessInstaller
            // 
            this.teleoptiBrokerServiceProcessInstaller.Password = "".ToString();
            this.teleoptiBrokerServiceProcessInstaller.Username = "NT AUTHORITY\\LocalService".ToString();
            // 
            // teleoptiBrokerServiceInstaller
            // 
            this.teleoptiBrokerServiceInstaller.Description = "The Teleopti Broker Service supports tcp/ip and multicast messaging protocols";
            this.teleoptiBrokerServiceInstaller.DisplayName = "Teleopti Message Broker Service";
            this.teleoptiBrokerServiceInstaller.ServiceName = "TeleoptiBrokerService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.teleoptiBrokerServiceProcessInstaller,
            this.teleoptiBrokerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller teleoptiBrokerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller teleoptiBrokerServiceInstaller;
    }
}