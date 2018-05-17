using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

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
							var format = new PayrollFormat { Name = payrollFormatDto.Name, FormatId = payrollFormatDto.FormatId };
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

		public void InitializeOneTenant(IList<PayrollFormatDto> tenantPayrollFormats, string tenantName)
		{
			if (tenantPayrollFormats == null)
				tenantPayrollFormats = _plugInLoader.LoadDtos();

			var tenant = _dataSourceForTenant.Tenant(tenantName);
			var unitOfWork = (tenant.Application.HasCurrentUnitOfWork()
				? tenant.Application.CurrentUnitOfWork()
				: tenant.Application.CreateAndOpenUnitOfWork());

			var payrollFormatRepository = new PayrollFormatRepository(new ThisUnitOfWork(unitOfWork));
			var oldOnes = payrollFormatRepository.LoadAll();
			foreach (var payrollFormat in oldOnes)
			{
				var existingPayrollFormat = tenantPayrollFormats.SingleOrDefault(x =>
					x.FormatId == payrollFormat.FormatId && x.Name == payrollFormat.Name);

				if (existingPayrollFormat == null)
				{
					payrollFormatRepository.Remove(payrollFormat);
				}
				else
				{
					tenantPayrollFormats.Remove(existingPayrollFormat);
				}
			}

			foreach (var payrollFormatDto in tenantPayrollFormats)
			{
				var format = new PayrollFormat { Name = payrollFormatDto.Name, FormatId = payrollFormatDto.FormatId };
				payrollFormatRepository.Add(format);
			}

			unitOfWork.PersistAll();
		}
	}
}
