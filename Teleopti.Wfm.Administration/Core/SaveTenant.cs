using System;
using System.Data.SqlClient;
using System.Linq;
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
					var appBuilder = new SqlConnectionStringBuilder { DataSource = model.Server, InitialCatalog = model.AppDatabase, UserID = model.UserName, Password = model.Password };
					var analBuilder = new SqlConnectionStringBuilder(appBuilder.ConnectionString) { InitialCatalog = model.AnalyticsDatabase };
					oldTenant.DataSourceConfiguration.SetApplicationConnectionString(appBuilder.ConnectionString);
					oldTenant.DataSourceConfiguration.SetAnalyticsConnectionString(analBuilder.ConnectionString);
					oldTenant.DataSourceConfiguration.SetNHibernateConfig(Environment.CommandTimeout, model.CommandTimeout.ToString());
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