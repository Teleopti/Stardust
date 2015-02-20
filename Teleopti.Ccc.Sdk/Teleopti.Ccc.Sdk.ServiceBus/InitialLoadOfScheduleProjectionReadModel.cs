﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class InitialLoadOfScheduleProjectionReadModel
	{
		private readonly Func<IServiceBus> _serviceBusFinder;

		public InitialLoadOfScheduleProjectionReadModel(Func<IServiceBus> serviceBusFinder)
		{
			_serviceBusFinder = serviceBusFinder;
		}

		public void Check()
		{
			var messagesOnBoot = true;
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MessagesOnBoot"]))
				bool.TryParse(ConfigurationManager.AppSettings["MessagesOnBoot"], out messagesOnBoot);
			if (!messagesOnBoot)
				return;
			var start = parseNumber("InitialLoadScheduleProjectionStartDate", "-31");
			var end = parseNumber("InitialLoadScheduleProjectionEndDate", "180");

			var bus = _serviceBusFinder.Invoke();

			StateHolderReader.Instance.StateReader.ApplicationScopeData.DoOnAllTenants_AvoidUsingThis(tenant =>
			{
				IList<Guid> businessUnitCollection;
				using (var unitOfWork = tenant.Application.CreateAndOpenUnitOfWork())
				{
					var businessUnitRepository = new BusinessUnitRepository(unitOfWork);
					businessUnitCollection = businessUnitRepository.LoadAll().Select(b => b.Id.GetValueOrDefault()).ToList();
				}
				foreach (var businessUnitId in businessUnitCollection)
				{
					bus.Send(new InitialLoadScheduleProjection { Datasource = tenant.DataSourceName, BusinessUnitId = businessUnitId, Timestamp = DateTime.UtcNow, StartDays = start, EndDays = end });
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