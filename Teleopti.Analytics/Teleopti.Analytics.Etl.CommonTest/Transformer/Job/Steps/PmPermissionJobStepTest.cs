using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class PmPermissionJobStepTest
	{
		[Test]
		public void ShouldDoNothingIfNotLastBusinessUnit()
		{
			var target = new PmPermissionJobStep(null);
			var result = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), false);
			result.RowsAffected.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotNeedToRunPmPermissionsStep()
		{
			var jobParameters = MockRepository.GenerateMock<IJobParameters>();
			var stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
			var target = new PmPermissionJobStep(jobParameters, true);

			jobParameters.Stub(x => x.StateHolder).Return(stateHolder);
			stateHolder.Stub(x => x.PermissionsMustRun()).Return(false);

			var result = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);
			result.RowsAffected.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSynchronize()
		{
			var jobParameters = MockRepository.GenerateMock<IJobParameters>();
			var stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var transformer = MockRepository.GenerateMock<IPmPermissionTransformer>();
			var permissionExtractor = MockRepository.GenerateMock<IPmPermissionExtractor>();
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();
			var raptorRep = MockRepository.GenerateMock<IRaptorRepository>();
			var personList = new List<IPerson>();
			var pmUsers = new List<PmUser> { new PmUser() };
			var target = new PmPermissionJobStepForTest(jobParameters, unitOfWorkFactory, true)
			{
				Transformer = transformer,
				PermissionExtractor = permissionExtractor
			};

			jobParameters.Stub(x => x.StateHolder).Return(stateHolder);
			stateHolder.Stub(x => x.PermissionsMustRun()).Return(true);
			stateHolder.Stub(x => x.UserCollection).Return(personList);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);

			transformer.Stub(
				x => x.GetUsersWithPermissionsToPerformanceManager(personList,  permissionExtractor, unitOfWorkFactory))
				.Return(pmUsers);
			jobParameters.Stub(x => x.Helper).Return(jobHelper);
			jobHelper.Stub(x => x.Repository).Return(raptorRep);
			raptorRep.Stub(x => x.PersistPmUser(target.BulkInsertDataTable1)).Return(99);

			var result = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);

			pmUsers.Count.Should().Be.EqualTo(1);
			transformer.AssertWasCalled(x => x.Transform(pmUsers, target.BulkInsertDataTable1));
			result.RowsAffected.Should().Be.EqualTo(99);
		}
	}

	public class PmPermissionJobStepForTest : PmPermissionJobStep
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public PmPermissionJobStepForTest(IJobParameters parameters, IUnitOfWorkFactory unitOfWorkFactory)
			: base(parameters)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public PmPermissionJobStepForTest(IJobParameters parameters, IUnitOfWorkFactory unitOfWorkFactory, bool checkIfNeeded)
			: base(parameters, checkIfNeeded)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		protected override IUnitOfWorkFactory UnitOfWorkFactory
		{
			get
			{
				return _unitOfWorkFactory;
			}
		}
	}
}
