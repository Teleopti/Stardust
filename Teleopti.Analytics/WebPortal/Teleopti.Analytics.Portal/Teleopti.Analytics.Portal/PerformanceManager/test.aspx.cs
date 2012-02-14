using System;
using System.Security.Principal;

namespace Teleopti.Analytics.Portal.PerformanceManager
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(WindowsIdentity.GetCurrent().Name);
        }
    }
}