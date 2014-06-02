using System.Windows.Forms;
using EO.WebBrowser;

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
            form.webControl1.WebView.BeforeContextMenu += webView1_BeforeContextMenu;
            e.Accepted = true;
            form.Show();
        }

        private void WebView_NewWindow(object sender, EO.WebBrowser.NewWindowEventArgs e)
        {
        }

        private void webView1_BeforeContextMenu(object sender, EO.WebBrowser.BeforeContextMenuEventArgs e)
        {
            foreach (var item in e.Menu.Items)
            {
                if (item.CommandId == CommandIds.ViewSource)
                {
                    e.Menu.Items.Remove(item);
                    break;
                }
            }
        }
    }
}
