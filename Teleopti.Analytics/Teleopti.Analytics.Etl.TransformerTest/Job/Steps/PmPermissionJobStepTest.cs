using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[TestFixture]
	public class PmPermissionJobStepTest
	{
		private PmPermissionJobStep _target;
		private MockRepository _mocks;
		private IJobParameters _jobParameters;
		private IPmPermissionTransformer _pmPermissionTransformer;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _unitOfWork;
		private ICommonStateHolder _stateHolder;
		private IPmPermissionExtractor _permissionExtractor;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_jobParameters = _mocks.StrictMock<IJobParameters>();
			_pmPermissionTransformer = _mocks.StrictMock<IPmPermissionTransformer>();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_unitOfWork = _mocks.StrictMock<IUnitOfWork>();
			_stateHolder = _mocks.StrictMock<ICommonStateHolder>();
			_permissionExtractor = _mocks.StrictMock<IPmPermissionExtractor>();

			_target = new PmPermissionJobStepForTest(_jobParameters, _unitOfWorkFactory)
						  {
							  Transformer = _pmPermissionTransformer,
							  PermissionExtractor = _permissionExtractor
						  };
		}

		[Test]
		public void VerifyRunNotLastBusinessUnit()
		{
			using (_mocks.Record())
			{
				IList<IPerson> personList = new List<IPerson>();
				Expect.Call(_jobParameters.StateHolder).Return(_stateHolder);
				Expect.Call(_stateHolder.UserCollection).Return(new List<IPerson>());
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				_unitOfWork.Reassociate(personList);
				Expect.Call(_pmPermissionTransformer.GetUsersWithPermissionsToPerformanceManager(personList, true, _permissionExtractor, _unitOfWorkFactory)).Return(new List<UserDto>());
				Expect.Call(_pmPermissionTransformer.GetUsersWithPermissionsToPerformanceManager(personList, false, _permissionExtractor, _unitOfWorkFactory)).Return(new List<UserDto>());
				_unitOfWork.Dispose();
			}

			using (_mocks.Playback())
			{
				IJobStepResult jobStepResult = _target.Run(new List<IJobStep>(), new BusinessUnit("bu"), new List<IJobResult>(), false);
				Assert.AreEqual(0, jobStepResult.RowsAffected);
			}
		}

		[Test]
		public void VerifyRunLastBusinessUnit()
		{
			testRunLastBusinessUnit(true);

			using (_mocks.Playback())
			{
				IJobStepResult jobStepResult = _target.Run(new List<IJobStep>(), new BusinessUnit("bu"), new List<IJobResult>(), true);
				Assert.AreEqual(2, jobStepResult.RowsAffected);
				Assert.AreEqual("Done", jobStepResult.Status);
			}
		}

		[Test]
		public void VerifyRunLastBusinessUnitUnsuccessful()
		{
			testRunLastBusinessUnit(false);

			using (_mocks.Playback())
			{
				IJobStepResult jobStepResult =_target.Run(new List<IJobStep>(), new BusinessUnit("bu"), new List<IJobResult>(), true);
				Assert.AreEqual("Error", jobStepResult.Status);
				Assert.IsNotNull(jobStepResult.JobStepException);
			}
		}

		private void testRunLastBusinessUnit(bool isSuccess)
		{
			using (_mocks.Record())
			{
				const string olapServer = "olapServer";
				const string olapDb = "olapDb";

				IList<IPerson> personList = new List<IPerson>();
				var jobHelper = _mocks.StrictMock<IJobHelper>();
				var repository = _mocks.StrictMock<IRaptorRepository>();

				Expect.Call(_jobParameters.StateHolder).Return(_stateHolder).Repeat.AtLeastOnce();
				Expect.Call(_stateHolder.UserCollection).Return(new List<IPerson>()).Repeat.AtLeastOnce();
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				_unitOfWork.Reassociate(personList);
				Expect.Call(_pmPermissionTransformer.GetUsersWithPermissionsToPerformanceManager(personList, true, _permissionExtractor, _unitOfWorkFactory)).Return(new List<UserDto>());
				Expect.Call(_pmPermissionTransformer.GetUsersWithPermissionsToPerformanceManager(personList, false, _permissionExtractor, _unitOfWorkFactory)).Return(new List<UserDto>());
				_unitOfWork.Dispose();

				Expect.Call(_pmPermissionTransformer.GetPmUsersForAllBusinessUnits(_target.Name, new List<IJobResult>(), new List<UserDto>())).Return(new List<UserDto>());
				Expect.Call(_jobParameters.OlapServer).Return(olapServer);
				Expect.Call(_jobParameters.OlapDatabase).Return(olapDb);
				Expect.Call(_pmPermissionTransformer.SynchronizeUserPermissions(new List<UserDto>(), olapServer, olapDb)).Return(createResult(isSuccess));
				if (!isSuccess)
					return;
				_pmPermissionTransformer.Transform(new List<UserDto>(), _target.BulkInsertDataTable1);
				Expect.Call(_jobParameters.Helper).Return(jobHelper);
				Expect.Call(jobHelper.Repository).Return(repository);
				Expect.Call(repository.PersistPmUser(_target.BulkInsertDataTable1)).Return(1);
			}
		}

		[Test]
		public void ShouldCheckIfNeedRun()
		{
			_target = new PmPermissionJobStep(_jobParameters,true);
			Expect.Call(_jobParameters.StateHolder).Return(_stateHolder);
			Expect.Call(_stateHolder.PermissionsMustRun()).Return(false);
			_mocks.ReplayAll();
			_target.Run(new List<IJobStep>(), new BusinessUnit("bu"), new List<IJobResult>(), true);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCheckIfStageNeedRun()
		{
			var helper = _mocks.DynamicMock<IJobHelper>();
			var rep = _mocks.DynamicMock<IRaptorRepository>();
			_target = new PmPermissionJobStep(_jobParameters, true);
			Expect.Call(_jobParameters.Helper).Return(helper).Repeat.AtLeastOnce();
			Expect.Call(helper.Repository).Return(rep);
			Expect.Call(rep.LastChangedDate(null, "Permissions")).IgnoreArguments().Return(new LastChangedReadModel());

			Expect.Call(_jobParameters.StateHolder).Return(_stateHolder).Repeat.AtLeastOnce();
			Expect.Call(() =>_stateHolder.SetThisTime(new LastChangedReadModel(), "Permissions")).IgnoreArguments();
			Expect.Call(_stateHolder.PermissionsMustRun()).Return(false);
			
			_mocks.ReplayAll();
			var target = new StagePermissionJobStep(_jobParameters, true);
			target.Run(new List<IJobStep>(), new BusinessUnit("bu"), new List<IJobResult>(), true);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCheckIfReportStepNeedRun()
		{
			_target = new PmPermissionJobStep(_jobParameters, true);
			Expect.Call(_jobParameters.StateHolder).Return(_stateHolder);
			Expect.Call(_stateHolder.PermissionsMustRun()).Return(false);
			_mocks.ReplayAll();
			var target = new PermissionReportJobStep(_jobParameters, true);
			target.Run(new List<IJobStep>(), new BusinessUnit("bu"), new List<IJobResult>(), true);
			_mocks.VerifyAll();
		}

		private static ResultDto createResult(bool isSuccess)
		{
			var result = new ResultDto {Success = isSuccess, AffectedUsersCount = 1};

			return result;
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

		protected override IUnitOfWorkFactory UnitOfWorkFactory
		{
			get
			{
				return _unitOfWorkFactory;
			}
		}
	}
}
