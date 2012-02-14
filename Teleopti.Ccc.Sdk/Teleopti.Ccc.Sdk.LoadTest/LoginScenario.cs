using System;
using System.Linq;
using Teleopti.Ccc.Sdk.Client;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public class LoginScenario : ITestScenario
	{
		private readonly string _serviceUrl;
		private readonly string _userName;
		private readonly string _password;
		private readonly TestContext _context = new TestContext();
		private char _status = ' ';

		public LoginScenario(string serviceUrl, string userName, string password)
		{
			LastElapsedTime = TimeSpan.Zero;
			_serviceUrl = serviceUrl;
			_userName = userName;
			_password = password;
			_status = '.';
		}

		public TimeSpan LastElapsedTime { get; set; }

		public void WriteStatus(Action<string> writer)
		{
			writer.Invoke(string.Format("{0} {1}", _status, LastElapsedTime.Seconds));
		}

		public void RunOnce()
		{
			var startTime = DateTime.Now;
			SetStatus('o');
			_context.Make(_serviceUrl);
			LoginProcess();
			//var personDtos = client.OrganizationService.GetPersons(true, true);
			_context.Clear();
			LastElapsedTime = DateTime.Now.Subtract(startTime);
			SetStatus('.');
		}

		private void SetStatus(char status)
		{
			_status = status;
		}

		private void LoginProcess() {
			SetStatus('O');
            ThreadLocalAuthenticationSoapHeader.Reset();
			var dataSource = GetDataSource();
			var businessUnit = Login(dataSource);
            if (businessUnit != null)
            {
                SetBusinessUnit(businessUnit);
                VerifyLicense();
                var loggedOnPerson = GetLoggedOnPerson();
                if (loggedOnPerson.ApplicationLogOnName != _userName)
                {
                    throw new InvalidOperationException("This should never happen!");
                }
                _context.Session.SetSessionData(loggedOnPerson, businessUnit, dataSource, _password);
            }
		}

		private PersonDto GetLoggedOnPerson()
		{
			SetStatus('p');
			return _context.Client.LogOnServiceClient.GetLoggedOnPerson();
		}

		private void VerifyLicense()
		{
			SetStatus('v');
			_context.Client.LogOnServiceClient.VerifyLicense();
		}

		private void SetBusinessUnit(BusinessUnitDto businessUnit)
		{
			SetStatus('b');
			_context.Client.LogOnServiceClient.SetBusinessUnit(businessUnit);
		}

        private BusinessUnitDto Login(DataSourceDto dataSource)
        {
            SetStatus('l');
            var authenticationResultDto = _context.Client.LogOnServiceClient.LogOnApplicationUser(_userName, _password,
                                                                                                  dataSource);

            ThreadLocalAuthenticationSoapHeader.Current.UserName = _userName;
            ThreadLocalAuthenticationSoapHeader.Current.Password = _password;
            ThreadLocalAuthenticationSoapHeader.Current.UseWindowsIdentity = false;

            if (!authenticationResultDto.Successful) return null;

            var result = authenticationResultDto.BusinessUnitCollection.First();

            ThreadLocalAuthenticationSoapHeader.Current.BusinessUnit = result.Id;

            return result;
        }

	    private DataSourceDto GetDataSource() {
			SetStatus('d');
			var dataSources = _context.Client.LogOnServiceClient.GetDataSources();
			var result = dataSources.First(d => d.Name.Contains(" CCC"));

            ThreadLocalAuthenticationSoapHeader.Current.DataSource = result.Name;

		    return result;
		}
	}
}