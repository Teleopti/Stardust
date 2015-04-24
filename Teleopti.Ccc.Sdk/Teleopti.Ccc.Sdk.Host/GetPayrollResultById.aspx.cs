using System;
using System.Globalization;
using System.Security.Principal;
using Autofac;
using Autofac.Integration.Wcf;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
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

			var payrollLogon = AutofacHostFactory.Container.Resolve<IPayrollLogon>();

			AuthenticationResult result;

			if (useWindowsIdentity)
			{
				result = payrollLogon.LogonWindows(datasource);
			}
			else
			{
				result = payrollLogon.LogonApplication(userName, password, datasource);
			}

			if (!result.Successful)
			{
				Response.Write(result.Message);
				return;
			}

			//var provider = sourceContainer.AvailableBusinessUnitProvider;
			var logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
			//var bUnit = provider.AvailableBusinessUnits().FirstOrDefault(b => b.Id.ToString().Equals(businessUnit));
			logOnOff.LogOn(result.DataSource, result.Person, null);// bUnit);

			using (result.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var service = new PayrollResultService(CurrentUnitOfWorkFactory.Make(), new PayrollResultRepository(result.DataSource.Application));
				var buffer = service.CreatePayrollResultFileNameById(new Guid(guid));
				Response.AppendHeader("content-disposition", string.Format(CultureInfo.CurrentCulture, "attachment; filename={0}", guid));
				Response.OutputStream.Write(buffer, 0, buffer.Length);
				Response.End();
			}
		}

		public interface IPayrollLogon
		{
			AuthenticationResult LogonWindows(string dataSource);
			AuthenticationResult LogonApplication(string userName, string password, string dataSource);
		}

		public class MultiTenancyPayrollLogon : IPayrollLogon
		{
			private readonly IMultiTenancyApplicationLogon _multiTenancyApplicationLogon;
			private readonly IMultiTenancyWindowsLogon _multiTenancyWindowsLogon;
			public const string UserAgent = "SDKPayroll";

			public MultiTenancyPayrollLogon(IMultiTenancyApplicationLogon multiTenancyApplicationLogon, IMultiTenancyWindowsLogon multiTenancyWindowsLogon)
			{
				_multiTenancyApplicationLogon = multiTenancyApplicationLogon;
				_multiTenancyWindowsLogon = multiTenancyWindowsLogon;
			}

			public AuthenticationResult LogonWindows(string dataSource)
			{
				var model = new LogonModel();
				var result = _multiTenancyWindowsLogon.Logon(model, StateHolderReader.Instance.StateReader.ApplicationScopeData,
					UserAgent);
				result.DataSource = model.SelectedDataSourceContainer.DataSource;
				return result;
			}

			public AuthenticationResult LogonApplication(string userName, string password, string dataSource)
			{
				var model = new LogonModel { UserName = userName, Password = password };
				var result = _multiTenancyApplicationLogon.Logon(model, StateHolderReader.Instance.StateReader.ApplicationScopeData,
					UserAgent);
				result.DataSource = model.SelectedDataSourceContainer.DataSource;
				return result;
			}
		}
	}
}