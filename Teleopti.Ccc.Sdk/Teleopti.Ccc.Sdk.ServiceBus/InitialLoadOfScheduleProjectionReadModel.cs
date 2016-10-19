﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class InitialLoadOfScheduleProjectionReadModel
	{
		private readonly Func<IServiceBus> _serviceBusFinder;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly IToggleManager _toggleManager;

		public InitialLoadOfScheduleProjectionReadModel(Func<IServiceBus> serviceBusFinder, IDataSourceForTenant dataSourceForTenant, IToggleManager toggleManager)
		{
			_serviceBusFinder = serviceBusFinder;
			_dataSourceForTenant = dataSourceForTenant;
			_toggleManager = toggleManager;
		}

		public void Check()
		{
			if (_toggleManager.IsEnabled(Toggles.LastHandlers_ToHangfire_41203))
				return;

			var messagesOnBoot = true;
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MessagesOnBoot"]))
				bool.TryParse(ConfigurationManager.AppSettings["MessagesOnBoot"], out messagesOnBoot);
			if (!messagesOnBoot)
				return;
			var start = parseNumber("InitialLoadScheduleProjectionStartDate", "-31");
			var end = parseNumber("InitialLoadScheduleProjectionEndDate", "180");

			var bus = _serviceBusFinder.Invoke();

			_dataSourceForTenant.DoOnAllTenants_AvoidUsingThis(tenant =>
			{
				IList<Guid> businessUnitCollection;
				using (var unitOfWork = tenant.Application.CreateAndOpenUnitOfWork())
				{
					var businessUnitRepository = new BusinessUnitRepository(unitOfWork);
					businessUnitCollection = businessUnitRepository.LoadAll().Select(b => b.Id.GetValueOrDefault()).ToList();
				}
				foreach (var businessUnitId in businessUnitCollection)
				{
					bus.Send(new InitialLoadScheduleProjection { LogOnDatasource = tenant.DataSourceName, LogOnBusinessUnitId = businessUnitId, Timestamp = DateTime.UtcNow, StartDays = start, EndDays = end });
				}
			});
		}

		private static int parseNumber(string configString, string defaultValue)
		{
			var days = ConfigurationManager.AppSettings[configString];
			if (string.IsNullOrEmpty(days))
			{
				days = defaultValue;
			}
			int number;
			int.TryParse(days, out number);
			return number;
		}
	}
}