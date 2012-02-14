using System.Security.Principal;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class PrincipalProvider : IPrincipalProvider
	{
		private readonly IDataSourcesProvider _dataSourcesProvider;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;

	    public PrincipalProvider(IDataSourcesProvider dataSourcesProvider, 
								ISessionSpecificDataProvider sessionSpecificDataProvider, 
								IRepositoryFactory repositoryFactory, 
								IRoleToPrincipalCommand roleToPrincipalCommand)
		{
			_dataSourcesProvider = dataSourcesProvider;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_repositoryFactory = repositoryFactory;
			_roleToPrincipalCommand = roleToPrincipalCommand;
		}

		public TeleoptiPrincipal Generate()
		{
			var sessionData = _sessionSpecificDataProvider.Grab();
			return sessionData == null ? null : createPrincipal(sessionData);
		}

		private TeleoptiPrincipal createPrincipal(SessionSpecificData sessionData)
		{
			var dataSource = _dataSourcesProvider.RetrieveDataSourceByName(sessionData.DataSourceName);

		    TeleoptiPrincipal principal;
		    using(var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRep.Get(sessionData.PersonId);
				if (person == null)
					return null;

                var buRep = _repositoryFactory.CreateBusinessUnitRepository(uow);
                IBusinessUnit businessUnit = buRep.Get(sessionData.BusinessUnitId);

				var teleoptiIdentity = new TeleoptiIdentity(person.Name.ToString(), dataSource, businessUnit, WindowsIdentity.GetCurrent());
			    principal = new TeleoptiPrincipal(teleoptiIdentity, person);
			    _roleToPrincipalCommand.Execute(principal, uow, personRep);
				
			}

            return principal;
		}
	}
}