using System.Globalization;
using System.ServiceModel;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class InitializePayrollFormatsUsingAppDomain : IInitializePayrollFormats
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(InitializePayrollFormatsUsingAppDomain));
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly IPayrollFormatRepositoryFactory _payrollFormatRepositoryFactory;

		public InitializePayrollFormatsUsingAppDomain(IDataSourceForTenant dataSourceForTenant, IPayrollFormatRepositoryFactory payrollFormatRepositoryFactory)
		{
			_dataSourceForTenant = dataSourceForTenant;
			_payrollFormatRepositoryFactory = payrollFormatRepositoryFactory;
		}


		public void Initialize()
		{
			try
			{
				logger.InfoFormat(CultureInfo.CurrentCulture, "Initializing");
				_dataSourceForTenant.DoOnAllTenants_AvoidUsingThis(tenant => { RefreshOneTenant(tenant.DataSourceName); });
			}
			catch (CommunicationException exception)
			{
				logger.ErrorFormat("Error when initializing payroll formats: {0}", exception.Message);
			}
		}

		public void RefreshOneTenant(string tenantName)
		{
			var tenant = _dataSourceForTenant.Tenant(tenantName);
			using (var unitOfWork = tenant.Application.CreateAndOpenUnitOfWork())
			{
				var payrollFormatRepository = 
					_payrollFormatRepositoryFactory.CreatePayrollFormatRepository(new ThisUnitOfWork(unitOfWork));
				var oldOnes = payrollFormatRepository.LoadAll();
				foreach (var payrollFormat in oldOnes)
				{
					payrollFormatRepository.Remove(payrollFormat);
				}

				logger.InfoFormat(CultureInfo.CurrentCulture, "Saving formats for " + tenant.DataSourceName);
				var wrapper = new AppdomainCreatorWrapper();
				//change this with the shared path
				var searchPath = new SearchPath();
				var payrollFormatsDtos = wrapper.FindPayrollFormatsForTenant(tenant.DataSourceName, searchPath.Path);
				foreach (var payrollFormatDto in payrollFormatsDtos)
				{
					var format = new PayrollFormat { Name = payrollFormatDto.Name, FormatId = payrollFormatDto.FormatId };
					payrollFormatRepository.Add(format);
				}

				unitOfWork.PersistAll();
			}
		}
	}
}