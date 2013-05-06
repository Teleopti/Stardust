﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{

	public class HardCodedResolver : IResolve
	{
		public object Resolve(Type type)
		{
			// use autofac soon?
			if (type == typeof (IEnumerable<IHandleEvent<ScheduleChangedEvent>>))
				return new[]
					{
						new ScheduleChangedHandler(
							new EventPublisher(this),
							new ScenarioRepository(CurrentUnitOfWork.Make()),
							new PersonRepository(CurrentUnitOfWork.Make()),
							new ScheduleRepository(CurrentUnitOfWork.Make()),
							new ProjectionChangedEventBuilder())
					};
			if (type == typeof (IEnumerable<IHandleEvent<ProjectionChangedEvent>>))
				return new[]
					{
						new PersonScheduleDayReadModelHandler(
							new PersonScheduleDayReadModelsCreator(
								new PersonRepository(CurrentUnitOfWork.Make()),
								new NewtonsoftJsonSerializer()),
							new PersonScheduleDayReadModelStorage(
								CurrentUnitOfWork.Make(),
								new DoNotSend(),
								new CurrentDataSource(new CurrentIdentity()))
							)
					};
			throw new Exception("Cannot resolve type " + type + "! Add it manually or consider using autofac!");
		}
	}

	public class NewtonsoftJsonSerializer : IJsonSerializer
	{
		public string SerializeObject(object value)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(value);
		}
	}

}