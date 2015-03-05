using System;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using Autofac;
using Autofac.Integration.Wcf;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Logic.Payroll;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost
{
    public partial class GetPayrollResultById : System.Web.UI.Page
    {
  
        protected void Page_Load(object sender, EventArgs e)
        {
            var guid = Request.Form["ResultGuid"];
            var datasource = Request.Form["DataSource"];
           // var businessUnit = Request.Form["BusinessUnit"];
            var userName = Request.Form["UserName"];
            var password = Request.Form["Password"];
            var useWindowsIdentity = Convert.ToBoolean(Request.Form["UseWindowsIdentity"], CultureInfo.InvariantCulture); 

            var ds = StateHolder.Instance.StateReader.ApplicationScopeData.DataSource(datasource);
            var repFactory = new RepositoryFactory();
            var findApplicationUser = AutofacHostFactory.Container.Resolve<IFindApplicationUser>();

            AuthenticationResult result;
            DataSourceContainer sourceContainer;

            if (useWindowsIdentity)
            {
                sourceContainer = new DataSourceContainer(ds, repFactory, findApplicationUser, AuthenticationTypeOption.Windows);
                result = sourceContainer.LogOn(WindowsIdentity.GetCurrent().Name);
            }
            else
            {
                sourceContainer = new DataSourceContainer(ds, repFactory, findApplicationUser, AuthenticationTypeOption.Application);
                result = sourceContainer.LogOn(userName, password);
            }

            if (!result.Successful)
            {
                Response.Write(result.Message);
                return;
            }
                
            //var provider = sourceContainer.AvailableBusinessUnitProvider;
            var logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
            //var bUnit = provider.AvailableBusinessUnits().FirstOrDefault(b => b.Id.ToString().Equals(businessUnit));
            logOnOff.LogOn(ds, sourceContainer.User, null);// bUnit);

            using(ds.Application.CreateAndOpenUnitOfWork())
            {
				var service = new PayrollResultService(CurrentUnitOfWorkFactory.Make(), new PayrollResultRepository(ds.Application));
                var buffer = service.CreatePayrollResultFileNameById(new Guid(guid));
                Response.AppendHeader("content-disposition", string.Format(CultureInfo.CurrentCulture, "attachment; filename={0}", guid));
                Response.OutputStream.Write(buffer,0, buffer.Length);
                Response.End();
            }
        }
    }
}