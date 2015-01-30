using System;
using System.Windows.Forms;
using System.IO;

namespace ClickOnceSign
{
    public partial class Form1 : Form
    {
        private OpenFileDialog certFileDialog = new OpenFileDialog();
        private FolderBrowserDialog appDirFolder = new FolderBrowserDialog();
        private bool signed = false;
        private bool fastClose = false;

        public Form1()
        {
            InitializeComponent();
            appDirFolder.ShowNewFolderButton = false;     
            certFileDialog.Filter = "pfx files (*.pfx)|*.pfx";           
        }

        public void DisableTextboxes()
        {
            if (txtApplication.Text.Length > 0)
                txtApplication.Enabled = false;

            if (txtManifest.Text.Length > 0)
                txtManifest.Enabled = false;

            if (txtApplicationDir.Text.Length > 0)
            {
                txtApplicationDir.Enabled = false;
                btnBrowseAppDir.Enabled = false;
            }
        }

        public void SetFastClose(bool fastClose)
        {
            this.fastClose = fastClose;
        }

        public void SetApplication(String application) 
        {
            txtApplication.Text = application;
        }

        public void SetManifest(String manifest)
        {
            txtManifest.Text = manifest;
        }

        public void SetProviderUrl(String providerUrl)
        {
            txtProviderUrl.Text = providerUrl;
        }

        public void SetCertFile(String certFile)
        {
            txtCertFile.Text = certFile;
        }

        public void SetPassword(String password)
        {
            txtPassword.Text = password;
        }

        public void SetAppDir(String appDir)
        {
            txtApplicationDir.Text = appDir;         

            if (txtApplication.Text.Length == 0)
                txtApplication.Text = FindFilename(txtApplicationDir.Text, "*.application");

            if (txtManifest.Text.Length == 0)
                txtManifest.Text = FindFilename(txtApplicationDir.Text, "*.manifest");
        }
   
        private void button1_Click(object sender, EventArgs e)
        {
            if (txtCertFile.Text.Length == 0)
                certFileDialog.InitialDirectory = txtApplicationDir.Text;
            if (certFileDialog.ShowDialog() == DialogResult.OK)
                txtCertFile.Text = certFileDialog.FileName;
        }

        private void btnBrowseAppDir_Click(object sender, EventArgs e)
        {
            appDirFolder.SelectedPath = txtApplication.Text;
            if (appDirFolder.ShowDialog() == DialogResult.OK)
            {
                SetAppDir(appDirFolder.SelectedPath);
            }
        }

        private static String FindFilename(String directory, String type)
        {
            try
            {
                String[] f = Directory.GetFiles(directory, type);
                if (f.Length > 0)
                    return Path.GetFileName(f[0]);
            }
            catch (Exception)
            {
                // Don't care, since this is only a help funktion
            }
            return "";
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
	        var url = txtProviderUrl.Text.ToLower();
	        if (url.StartsWith("http://localhost") ||
                url.StartsWith("http://*"))
            {
                if (MessageBox.Show("Your provider url points to host 'localhost', are sure this is correct?", "URL Domain", MessageBoxButtons.YesNo) 
                    != DialogResult.Yes)
                    return;
            }

            btnSign.Enabled = false;
            btnDone.Enabled = false;

            signed = Sign.CoSign(txtApplicationDir.Text, txtApplication.Text, txtManifest.Text, 
                txtProviderUrl.Text, txtCertFile.Text, txtPassword.Text);

            if (signed && fastClose)
            {
                Application.Exit();
            }

            btnSign.Enabled = true;
            btnDone.Enabled = true;

            if (signed)
            {
                btnDone.Text = "Done";
                MessageBox.Show("Successfully signed ClickOnce deployment files", "Signed");
            }
            else
                btnDone.Text = "Cancel";
        }

  

        private void btnDone_Click(object sender, EventArgs e)
        {
            if (!signed)
                if (MessageBox.Show("You have not yet successfully signed the application. Are you sure you want to exit?", "Close", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

            Application.Exit();
        }
    }
}
