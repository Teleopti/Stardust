using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using log4net;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class InitializePayrollFormatsToDb : IInitializePayrollFormats
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(InitializePayrollFormatsToDb));
		private readonly IPlugInLoader _plugInLoader;
		private readonly IDataSourceForTenant _dataSourceForTenant;

		public InitializePayrollFormatsToDb(IPlugInLoader plugInLoader, IDataSourceForTenant dataSourceForTenant)
		{
			_plugInLoader = plugInLoader;
			_dataSourceForTenant = dataSourceForTenant;
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
				_dataSourceForTenant.DoOnAllTenants_AvoidUsingThis(tenant =>
				{
					using (var unitOfWork = tenant.Application.CreateAndOpenUnitOfWork())
					{
						var payrollFormatRepository = new PayrollFormatRepository(new ThisUnitOfWork(unitOfWork));
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
