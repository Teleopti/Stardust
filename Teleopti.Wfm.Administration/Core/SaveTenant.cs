using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Models;

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
			try
			{
				var oldTenant = _loadAllTenants.Tenants().FirstOrDefault(x => x.Name.Equals(model.OriginalName));
				if (oldTenant != null)
				{
					oldTenant.SetApplicationConnectionString(model.AppDatabase);
					oldTenant.SetAnalyticsConnectionString(model.AnalyticsDatabase);
					//oldTenant.Name = model.NewName;
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