using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.PerformanceManagerProxy;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class PmPermissionJobStep : JobStepBase
	{
		private readonly bool _checkIfNeeded;

		public PmPermissionJobStep(IJobParameters jobParameters, bool checkIfNeeded = false)
			: base(jobParameters)
		{
			_checkIfNeeded = checkIfNeeded;
			Name = "Performance Manager permissions";
			Transformer = new PmPermissionTransformer(new PmProxyFactory());
			PermissionExtractor = new PmPermissionExtractor(new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()));
			PmUserInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}
		
		protected virtual IUnitOfWorkFactory UnitOfWorkFactory
		{
			get { return Ccc.Infrastructure.UnitOfWork.UnitOfWorkFactory.Current; }
		}

		public IPmPermissionTransformer Transformer { get; set; }

		public IPmPermissionExtractor PermissionExtractor { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			if (_checkIfNeeded)
			{
				if (!_jobParameters.StateHolder.PermissionsMustRun()) return 0;
			}

			var personList = _jobParameters.StateHolder.UserCollection;
			List<UserDto> pmUserCheckList;

			using (var uow = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(personList);
				Result.SetPmUsersForCurrentBusinessUnit(Transformer.GetUsersWithPermissionsToPerformanceManager(personList, true, PermissionExtractor, UnitOfWorkFactory));
				pmUserCheckList = (List<UserDto>)Transformer.GetUsersWithPermissionsToPerformanceManager(personList, false, PermissionExtractor, UnitOfWorkFactory);
			}

			if (!isLastBusinessUnit)
				return 0;

			// Do synchronization of permissions for PM (WCF Service through proxy object)
			IList<UserDto> userDtoCollection = Transformer.GetPmUsersForAllBusinessUnits(Name, jobResultCollection, Result.PmUsersForCurrentBusinessUnit);
			ResultDto result = Transformer.SynchronizeUserPermissions(userDtoCollection, _jobParameters.OlapServer, _jobParameters.OlapDatabase);

			if (!result.Success)
				throw new PmSynchronizeException(result.ErrorMessage); // Throw exception since PM sync was NOT successful

			pmUserCheckList.AddRange(result.ValidAnalyzerUsers);
			Transformer.Transform(pmUserCheckList, BulkInsertDataTable1);
			var validAnalyzerUserCount = _jobParameters.Helper.Repository.PersistPmUser(BulkInsertDataTable1);

			return result.AffectedUsersCount + validAnalyzerUserCount;
		}
	}
}