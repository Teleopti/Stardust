using System;
using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Models;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Wfm.Administration.Core
{
	public class SaveTenant
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ILoadAllTenants _loadAllTenants;

		public SaveTenant(ICurrentTenantSession currentTenantSession, ILoadAllTenants loadAllTenants)
		{
			_currentTenantSession = currentTenantSession;
			_loadAllTenants = loadAllTenants;
		}

		public ImportTenantResultModel Execute(UpdateTenantModel model)
		{
			if (model.CommandTimeout == 0)
			{
				return new ImportTenantResultModel
				{
					Success = false,
					Message = "The command timeout must be a integer between 30 and 600."
				};
			}
			try
			{
				var oldTenant = _loadAllTenants.Tenants().FirstOrDefault(x => x.Name.Equals(model.OriginalName));
				if (oldTenant != null)
				{
					// bug 40926 We can't edit them so no need to change them
					//var appBuilder = new SqlConnectionStringBuilder { DataSource = model.Server, InitialCatalog = model.AppDatabase, UserID = model.UserName, Password = model.Password, IntegratedSecurity = model.UseIntegratedSecurity};
					//var analBuilder = new SqlConnectionStringBuilder(appBuilder.ConnectionString) { InitialCatalog = model.AnalyticsDatabase };
					//oldTenant.DataSourceConfiguration.SetApplicationConnectionString(appBuilder.ConnectionString);
					//oldTenant.DataSourceConfiguration.SetAnalyticsConnectionString(analBuilder.ConnectionString);
					oldTenant.SetApplicationConfig(Environment.CommandTimeout, model.CommandTimeout.ToString());
					oldTenant.SetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl, model.MobileQRCodeUrl);
					oldTenant.SetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes, model.MaximumSessionTime.ToString());
					oldTenant.Active = model.Active;
					_currentTenantSession.CurrentSession().Save(oldTenant);
				}
			}
			catch (Exception exception)
			{
				return new ImportTenantResultModel
				{
					Success = false,
					Message = exception.Message
				};
			}

			return new ImportTenantResultModel
			{
				Success = true,
				Message = ""
			};
		}
	}
}