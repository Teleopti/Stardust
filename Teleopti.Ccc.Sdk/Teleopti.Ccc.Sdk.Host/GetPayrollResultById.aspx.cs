using System;
using System.Globalization;
using System.Web.UI;
using Autofac;
using Autofac.Integration.Wcf;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Logic.Payroll;

namespace Teleopti.Ccc.Sdk.WcfHost
{
	public partial class GetPayrollResultById : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var guid = Request.Form["ResultGuid"];
			var userName = Request.Form["UserName"];
			var password = Request.Form["Password"];
			var useWindowsIdentity = Convert.ToBoolean(Request.Form["UseWindowsIdentity"], CultureInfo.InvariantCulture);

			var payrollLogon = AutofacHostFactory.Container.Resolve<IPayrollLogon>();

			var result = useWindowsIdentity ? 
				payrollLogon.LogonWindows() : 
				payrollLogon.LogonApplication(userName, password);

			if (!result.Success)
			{
				Response.Write(result.FailReason);
				return;
			}
			var logOnOff = new LogOnOff(null, null, null, null, null, new AppDomainPrincipalContext(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()), new ThreadPrincipalContext()), new TeleoptiPrincipalFactory(), null);
			logOnOff.LogOnWithoutPermissions(result.DataSource, result.Person, null);
			
			using (result.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var service = new PayrollResultService(new PayrollResultRepository(new FromFactory(() => result.DataSource.Application)));
				var buffer = service.CreatePayrollResultFileNameById(new Guid(guid));
				Response.AppendHeader("content-disposition", string.Format(CultureInfo.CurrentCulture, "attachment; filename={0}", guid));
				Response.OutputStream.Write(buffer, 0, buffer.Length);
				Response.End();
			}
		}

		public interface IPayrollLogon
		{
			AuthenticationQuerierResult LogonWindows();
			AuthenticationQuerierResult LogonApplication(string userName, string password);
		}

		public class MultiTenancyPayrollLogon : IPayrollLogon
		{
			private readonly IAuthenticationQuerier _authenticationQuerier;
			private readonly IWindowsUserProvider _windowsUserProvider;
			public const string UserAgent = "SDKPayroll";

			public MultiTenancyPayrollLogon(IAuthenticationQuerier authenticationQuerier, IWindowsUserProvider windowsUserProvider)
			{
				_authenticationQuerier = authenticationQuerier;
				_windowsUserProvider = windowsUserProvider;
			}

			public AuthenticationQuerierResult LogonWindows()
			{
				return _authenticationQuerier.TryLogon(new IdentityLogonClientModel {Identity = _windowsUserProvider.Identity()}, UserAgent);
			}

			public AuthenticationQuerierResult LogonApplication(string userName, string password)
			{
				return _authenticationQuerier.TryLogon(new ApplicationLogonClientModel{UserName = userName, Password = password}, UserAgent);
			}
		}
	}
}