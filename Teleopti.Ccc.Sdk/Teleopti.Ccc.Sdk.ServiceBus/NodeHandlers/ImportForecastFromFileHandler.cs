using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ImportForecastFromFileHandler : IHandle<ImportForecastsFileToSkill>
	{
		private readonly IComponentContext _componentContext;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IPersonRepository _personRepository;

		public ImportForecastFromFileHandler(IComponentContext componentContext, IDataSourceScope dataSourceScope, IPersonRepository personRepository)
		{
			_componentContext = componentContext;
			_dataSourceScope = dataSourceScope;
			_personRepository = personRepository;
		}

		public void Handle(ImportForecastsFileToSkill parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			//var ngt = _componentContext.Resolve<IBusinessUnitRepository>();

			// here we use the one in the message
			
			using (_dataSourceScope.OnThisThreadUse(parameters.LogOnDatasource))
			{
				var state = _componentContext.Resolve<DataSourceState>();

				using (var uow = state.Get().Application.CreateAndOpenUnitOfWork())
				{
					var person = _personRepository.Get(parameters.OwnerPersonId);
					Thread.CurrentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity(person.Name.FirstName, state.Get(), null, null, parameters.LogOnDatasource),person) ;
				}
				var personId = parameters.OwnerPersonId;
				// before this we need to set up a person as it is logged on
				var theRealOne = _componentContext.Resolve<IHandleEvent<ImportForecastsFileToSkill>>();
				theRealOne.Handle(parameters);
			}
		}
	}
}