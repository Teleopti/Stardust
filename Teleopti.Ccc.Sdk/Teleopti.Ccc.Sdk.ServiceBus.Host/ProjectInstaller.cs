using System.ComponentModel;
using System.Configuration.Install;


namespace Teleopti.Ccc.Sdk.ServiceBus.Host
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
