using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using log4net;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class InitializePayrollFormatsToDb : IInitializePayrollFormats
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(InitializePayrollFormatsToDb));
		private readonly IPlugInLoader _plugInLoader;

		public InitializePayrollFormatsToDb(IPlugInLoader plugInLoader)
		{
			_plugInLoader = plugInLoader;
		}

		public void Initialize()
		{
			try
			{
				logger.Info("Initializing payroll formats");

				var folder = AppDomain.CurrentDomain.BaseDirectory;
				logger.InfoFormat("Loading plugins in folder {0}", folder);
				var allPayrollFormats = _plugInLoader.LoadDtos();

				var formatsToAllDbs = allPayrollFormats.Where(f => f.DataSource.Equals("")).ToList();
				logger.InfoFormat(CultureInfo.CurrentCulture, "Sending formats to DB");
				StateHolderReader.Instance.StateReader.ApplicationScopeData.DataSourceForTenant.DoOnAllTenants_AvoidUsingThis(tenant =>
				{
					using (var unitOfWork = tenant.Application.CreateAndOpenUnitOfWork())
					{
						var payrollFormatRepository = new PayrollFormatRepository(unitOfWork);
						var oldOnes = payrollFormatRepository.LoadAll();
						foreach (var payrollFormat in oldOnes)
						{
							payrollFormatRepository.Remove(payrollFormat);
						}
						formatsToAllDbs.AddRange(allPayrollFormats.Where(f => f.DataSource.Equals(tenant.DataSourceName)));

						foreach (var payrollFormatDto in formatsToAllDbs)
						{
							var format = new PayrollFormat {Name = payrollFormatDto.Name, FormatId = payrollFormatDto.FormatId};
							payrollFormatRepository.Add(format);
						}
						unitOfWork.PersistAll();
					}

					
				});
			}
			catch (CommunicationException exception)
			{
				logger.ErrorFormat("Error when initializing payroll formats: {0}", exception.Message);
			}
		}
	}
}
