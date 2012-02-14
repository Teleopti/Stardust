using System.ComponentModel;
using System.Configuration.Install;


namespace Teleopti.Analytics.Etl.ServiceHost
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