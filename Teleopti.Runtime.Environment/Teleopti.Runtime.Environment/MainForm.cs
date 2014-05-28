using System.Windows.Forms;

namespace Teleopti.Runtime.Environment
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(string url):this()
        {
            webView1.Url = url;
        }

        private void webView1_NewWindow(object sender, EO.WebBrowser.NewWindowEventArgs e)
        {
            var form=new MainForm();
            if (e.Height.HasValue)
                form.Height = e.Height.Value;
            if (e.Width.HasValue)
                form.Width = e.Width.Value;
            form.webControl1.WebView = e.WebView;
            form.webControl1.WebView.NewWindow += form.WebView_NewWindow;
            e.Accepted = true;
            form.Show();
        }

        private void WebView_NewWindow(object sender, EO.WebBrowser.NewWindowEventArgs e)
        {
        }
    }
}
