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
			var userName = Request.Form["UserName"];
			var password = Request.Form["Password"];
			var useWindowsIdentity = Convert.ToBoolean(Request.Form["UseWindowsIdentity"], CultureInfo.InvariantCulture);

			var payrollLogon = AutofacHostFactory.Container.Resolve<IPayrollLogon>();

			AuthenticationResult result;

			if (useWindowsIdentity)
			{
				result = payrollLogon.LogonWindows();
			}
			else
			{
				result = payrollLogon.LogonApplication(userName, password);
			}

			if (!result.Successful)
			{
				Response.Write(result.Message);
				return;
			}

			var logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
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
			AuthenticationResult LogonWindows();
			AuthenticationResult LogonApplication(string userName, string password);
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

			public AuthenticationResult LogonWindows()
			{
				var model = new LogonModel();
				var result = _multiTenancyWindowsLogon.Logon(model, StateHolderReader.Instance.StateReader.ApplicationScopeData,
					UserAgent);
				result.DataSource = model.SelectedDataSourceContainer.DataSource;
				return result;
			}

			public AuthenticationResult LogonApplication(string userName, string password)
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