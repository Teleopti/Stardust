using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpPermissionReport_33584)]
	// TODO This should definately not be used on this event
	public class TestingPermissionUpdater :
		IHandleEvent<PersonCollectionChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IPersonRepository _personRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly IApplicationFunctionRepository _applicationFunctionRepository;
		private readonly INow _now;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IAnalyticsPermissionRepository _analyticsPermissionRepository;

		public TestingPermissionUpdater(IPersonRepository personRepository, ISiteRepository siteRepository, IApplicationFunctionRepository applicationFunctionRepository, INow now, ICurrentDataSource currentDataSource, IAnalyticsPermissionRepository analyticsPermissionRepository)
		{
			_personRepository = personRepository;
			_siteRepository = siteRepository;
			_applicationFunctionRepository = applicationFunctionRepository;
			_now = now;
			_currentDataSource = currentDataSource;
			_analyticsPermissionRepository = analyticsPermissionRepository;
		}

		public void Handle(PersonCollectionChangedEvent @event)
		{
			var sites = _siteRepository.LoadAll();
			foreach (var personId in @event.PersonIdCollection)
			{
				var person = _personRepository.Get(personId);
				ITeamResolver teamResolver = new TeamResolver(person, sites);
				var functionResolver = new ApplicationFunctionResolver(new ApplicationFunctionsForRole(new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()), _applicationFunctionRepository));
				var personRoleResolver = new PersonRoleResolver(person, teamResolver, functionResolver);
				var result = personRoleResolver.Resolve(new DateOnly(_now.UtcDateTime()), _currentDataSource.Current().Application);

				var currentPermissions = _analyticsPermissionRepository.GetPermissionsForPerson(personId);
				_analyticsPermissionRepository.DeletePermissionsForPerson(personId);
				_analyticsPermissionRepository.InsertPermissions(result, @event.LogOnBusinessUnitId);

			}
		}
	}
}