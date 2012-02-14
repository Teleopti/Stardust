using System.Windows.Forms;

namespace Teleopti.Messaging.Management.Views
{
    partial class ServiceManagerView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Panel panel;
        private MenuStrip menuStripServiceManager;
        private ToolStripMenuItem actionsToolStripMenuItem;
        private ToolStripMenuItem startServiceToolStripMenuItem;
        private ToolStripMenuItem stopServiceToolStripMenuItem;
        private ToolStripMenuItem installServiceToolStripMenuItem;
        private ToolStripMenuItem uninstallServiceToolStripMenuItem;
        private TextBox svcNameTextBox;
        private TextBox svcPathTextBox;
        private TextBox svcDisplayNameTextBox;
        private TextBox textBoxStatus;
        private Label labelDisplayName;
        private Label labelServiceName;
        private Label labelPath;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServiceManagerView));
            this.panel = new System.Windows.Forms.Panel();
            this.groupBoxMessageBroker = new System.Windows.Forms.GroupBox();
            this.buttonSendMessage = new System.Windows.Forms.Button();
            this.buttonMessageBrokerStop = new System.Windows.Forms.Button();
            this.buttonMessageBrokerStart = new System.Windows.Forms.Button();
            this.groupBoxService = new System.Windows.Forms.GroupBox();
            this.buttonMMC = new System.Windows.Forms.Button();
            this.buttonServices = new System.Windows.Forms.Button();
            this.buttonStar = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonUninstall = new System.Windows.Forms.Button();
            this.buttonInstall = new System.Windows.Forms.Button();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.labelPath = new System.Windows.Forms.Label();
            this.labelServiceName = new System.Windows.Forms.Label();
            this.labelDisplayName = new System.Windows.Forms.Label();
            this.svcDisplayNameTextBox = new System.Windows.Forms.TextBox();
            this.svcNameTextBox = new System.Windows.Forms.TextBox();
            this.svcPathTextBox = new System.Windows.Forms.TextBox();
            this.menuStripServiceManager = new System.Windows.Forms.MenuStrip();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel.SuspendLayout();
            this.groupBoxMessageBroker.SuspendLayout();
            this.groupBoxService.SuspendLayout();
            this.menuStripServiceManager.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel.Controls.Add(this.groupBoxMessageBroker);
            this.panel.Controls.Add(this.groupBoxService);
            this.panel.Controls.Add(this.textBoxStatus);
            this.panel.Controls.Add(this.labelPath);
            this.panel.Controls.Add(this.labelServiceName);
            this.panel.Controls.Add(this.labelDisplayName);
            this.panel.Controls.Add(this.svcDisplayNameTextBox);
            this.panel.Controls.Add(this.svcNameTextBox);
            this.panel.Controls.Add(this.svcPathTextBox);
            this.panel.Location = new System.Drawing.Point(3, 27);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(836, 365);
            this.panel.TabIndex = 0;
            // 
            // groupBoxMessageBroker
            // 
            this.groupBoxMessageBroker.Controls.Add(this.buttonSendMessage);
            this.groupBoxMessageBroker.Controls.Add(this.buttonMessageBrokerStop);
            this.groupBoxMessageBroker.Controls.Add(this.buttonMessageBrokerStart);
            this.groupBoxMessageBroker.Location = new System.Drawing.Point(510, 88);
            this.groupBoxMessageBroker.Name = "groupBoxMessageBroker";
            this.groupBoxMessageBroker.Size = new System.Drawing.Size(254, 52);
            this.groupBoxMessageBroker.TabIndex = 4;
            this.groupBoxMessageBroker.TabStop = false;
            this.groupBoxMessageBroker.Text = " Message Broker Instance ";
            // 
            // buttonSendMessage
            // 
            this.buttonSendMessage.Location = new System.Drawing.Point(168, 19);
            this.buttonSendMessage.Name = "buttonSendMessage";
            this.buttonSendMessage.Size = new System.Drawing.Size(75, 23);
            this.buttonSendMessage.TabIndex = 6;
            this.buttonSendMessage.Text = "Send";
            this.buttonSendMessage.UseVisualStyleBackColor = true;
            // 
            // buttonMessageBrokerStop
            // 
            this.buttonMessageBrokerStop.Location = new System.Drawing.Point(87, 19);
            this.buttonMessageBrokerStop.Name = "buttonMessageBrokerStop";
            this.buttonMessageBrokerStop.Size = new System.Drawing.Size(75, 23);
            this.buttonMessageBrokerStop.TabIndex = 5;
            this.buttonMessageBrokerStop.Text = "Stop";
            this.buttonMessageBrokerStop.UseVisualStyleBackColor = true;
            // 
            // buttonMessageBrokerStart
            // 
            this.buttonMessageBrokerStart.Location = new System.Drawing.Point(9, 19);
            this.buttonMessageBrokerStart.Name = "buttonMessageBrokerStart";
            this.buttonMessageBrokerStart.Size = new System.Drawing.Size(75, 23);
            this.buttonMessageBrokerStart.TabIndex = 4;
            this.buttonMessageBrokerStart.Text = "Start";
            this.buttonMessageBrokerStart.UseVisualStyleBackColor = true;
            // 
            // groupBoxService
            // 
            this.groupBoxService.Controls.Add(this.buttonMMC);
            this.groupBoxService.Controls.Add(this.buttonServices);
            this.groupBoxService.Controls.Add(this.buttonStar);
            this.groupBoxService.Controls.Add(this.buttonStop);
            this.groupBoxService.Controls.Add(this.buttonUninstall);
            this.groupBoxService.Controls.Add(this.buttonInstall);
            this.groupBoxService.Location = new System.Drawing.Point(9, 88);
            this.groupBoxService.Name = "groupBoxService";
            this.groupBoxService.Size = new System.Drawing.Size(495, 52);
            this.groupBoxService.TabIndex = 3;
            this.groupBoxService.TabStop = false;
            this.groupBoxService.Text = " Broker Service ";
            // 
            // buttonMMC
            // 
            this.buttonMMC.Location = new System.Drawing.Point(87, 19);
            this.buttonMMC.Name = "buttonMMC";
            this.buttonMMC.Size = new System.Drawing.Size(75, 23);
            this.buttonMMC.TabIndex = 1;
            this.buttonMMC.Text = "Services";
            this.buttonMMC.UseVisualStyleBackColor = true;
            // 
            // buttonServices
            // 
            this.buttonServices.Location = new System.Drawing.Point(6, 19);
            this.buttonServices.Name = "buttonServices";
            this.buttonServices.Size = new System.Drawing.Size(75, 23);
            this.buttonServices.TabIndex = 0;
            this.buttonServices.Text = "&Query";
            this.buttonServices.UseVisualStyleBackColor = true;
            // 
            // buttonStar
            // 
            this.buttonStar.Location = new System.Drawing.Point(168, 19);
            this.buttonStar.Name = "buttonStar";
            this.buttonStar.Size = new System.Drawing.Size(75, 23);
            this.buttonStar.TabIndex = 2;
            this.buttonStar.Text = "&Start";
            this.buttonStar.UseVisualStyleBackColor = true;
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(249, 19);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.Text = "Sto&p";
            this.buttonStop.UseVisualStyleBackColor = true;
            // 
            // buttonUninstall
            // 
            this.buttonUninstall.Location = new System.Drawing.Point(411, 19);
            this.buttonUninstall.Name = "buttonUninstall";
            this.buttonUninstall.Size = new System.Drawing.Size(75, 23);
            this.buttonUninstall.TabIndex = 5;
            this.buttonUninstall.Text = "&Uninstall";
            this.buttonUninstall.UseVisualStyleBackColor = true;
            // 
            // buttonInstall
            // 
            this.buttonInstall.Location = new System.Drawing.Point(330, 19);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Size = new System.Drawing.Size(75, 23);
            this.buttonInstall.TabIndex = 4;
            this.buttonInstall.Text = "&Install";
            this.buttonInstall.UseVisualStyleBackColor = true;
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatus.BackColor = System.Drawing.SystemColors.WindowText;
            this.textBoxStatus.ForeColor = System.Drawing.Color.LawnGreen;
            this.textBoxStatus.Location = new System.Drawing.Point(4, 146);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxStatus.Size = new System.Drawing.Size(823, 216);
            this.textBoxStatus.TabIndex = 10;
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(12, 13);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(29, 13);
            this.labelPath.TabIndex = 13;
            this.labelPath.Text = "Path";
            // 
            // labelServiceName
            // 
            this.labelServiceName.AutoSize = true;
            this.labelServiceName.Location = new System.Drawing.Point(12, 39);
            this.labelServiceName.Name = "labelServiceName";
            this.labelServiceName.Size = new System.Drawing.Size(74, 13);
            this.labelServiceName.TabIndex = 12;
            this.labelServiceName.Text = "Service Name";
            // 
            // labelDisplayName
            // 
            this.labelDisplayName.AutoSize = true;
            this.labelDisplayName.Location = new System.Drawing.Point(12, 65);
            this.labelDisplayName.Name = "labelDisplayName";
            this.labelDisplayName.Size = new System.Drawing.Size(72, 13);
            this.labelDisplayName.TabIndex = 11;
            this.labelDisplayName.Text = "Display Name";
            // 
            // svcDisplayNameTextBox
            // 
            this.svcDisplayNameTextBox.Location = new System.Drawing.Point(96, 62);
            this.svcDisplayNameTextBox.Name = "svcDisplayNameTextBox";
            this.svcDisplayNameTextBox.Size = new System.Drawing.Size(668, 20);
            this.svcDisplayNameTextBox.TabIndex = 2;
            // 
            // svcNameTextBox
            // 
            this.svcNameTextBox.Location = new System.Drawing.Point(96, 36);
            this.svcNameTextBox.Name = "svcNameTextBox";
            this.svcNameTextBox.ReadOnly = true;
            this.svcNameTextBox.Size = new System.Drawing.Size(668, 20);
            this.svcNameTextBox.TabIndex = 1;
            this.svcNameTextBox.Text = "TeleoptiBrokerService";
            // 
            // svcPathTextBox
            // 
            this.svcPathTextBox.Location = new System.Drawing.Point(96, 10);
            this.svcPathTextBox.Name = "svcPathTextBox";
            this.svcPathTextBox.Size = new System.Drawing.Size(668, 20);
            this.svcPathTextBox.TabIndex = 0;
            // 
            // menuStripServiceManager
            // 
            this.menuStripServiceManager.AllowMerge = false;
            this.menuStripServiceManager.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionsToolStripMenuItem});
            this.menuStripServiceManager.Location = new System.Drawing.Point(0, 0);
            this.menuStripServiceManager.Name = "menuStripServiceManager";
            this.menuStripServiceManager.Size = new System.Drawing.Size(842, 24);
            this.menuStripServiceManager.TabIndex = 1;
            this.menuStripServiceManager.Text = "menuStripServiceManager";
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startServiceToolStripMenuItem,
            this.stopServiceToolStripMenuItem,
            this.installServiceToolStripMenuItem,
            this.uninstallServiceToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.actionsToolStripMenuItem.Text = "&Actions";
            // 
            // startServiceToolStripMenuItem
            // 
            this.startServiceToolStripMenuItem.Name = "startServiceToolStripMenuItem";
            this.startServiceToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.startServiceToolStripMenuItem.Text = "&Start Service";
            // 
            // stopServiceToolStripMenuItem
            // 
            this.stopServiceToolStripMenuItem.Name = "stopServiceToolStripMenuItem";
            this.stopServiceToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.stopServiceToolStripMenuItem.Text = "Sto&p Service";
            // 
            // installServiceToolStripMenuItem
            // 
            this.installServiceToolStripMenuItem.Name = "installServiceToolStripMenuItem";
            this.installServiceToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.installServiceToolStripMenuItem.Text = "&Install Service";
            // 
            // uninstallServiceToolStripMenuItem
            // 
            this.uninstallServiceToolStripMenuItem.Name = "uninstallServiceToolStripMenuItem";
            this.uninstallServiceToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.uninstallServiceToolStripMenuItem.Text = "&Uninstall Service";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(160, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            // 
            // ServiceManagerView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 396);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.menuStripServiceManager);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripServiceManager;
            this.MinimumSize = new System.Drawing.Size(850, 430);
            this.Name = "ServiceManagerView";
            this.Text = "Service Manager";
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.groupBoxMessageBroker.ResumeLayout(false);
            this.groupBoxService.ResumeLayout(false);
            this.menuStripServiceManager.ResumeLayout(false);
            this.menuStripServiceManager.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GroupBox groupBoxService;
        private Button buttonUninstall;
        private Button buttonInstall;
        private GroupBox groupBoxMessageBroker;
        private Button buttonMMC;
        private Button buttonServices;
        private Button buttonStar;
        private Button buttonStop;
        private Button buttonMessageBrokerStop;
        private Button buttonMessageBrokerStart;
        private Button buttonSendMessage;




    }
}