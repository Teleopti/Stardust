﻿using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
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
		public void ShouldSynchronizeWhenPmAuthenticationIsWindows()
		{
			var jobParameters = MockRepository.GenerateMock<IJobParameters>();
			var stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var transformer = MockRepository.GenerateMock<IPmPermissionTransformer>();
			var pmWindowsUserSynchronizer = MockRepository.GenerateMock<IPmWindowsUserSynchronizer>();
			var permissionExtractor = MockRepository.GenerateMock<IPmPermissionExtractor>();
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();
			var raptorRep = MockRepository.GenerateMock<IRaptorRepository>();
			var personList = new List<IPerson>();
			var windowsAuthUsers = new List<UserDto> { new UserDto(), new UserDto() };
			var appAuthUsers = new List<UserDto> { new UserDto() };
			var target = new PmPermissionJobStepForTest(jobParameters, unitOfWorkFactory, true)
			{
				Transformer = transformer,
				PmWindowsUserSynchronizer = pmWindowsUserSynchronizer,
				PermissionExtractor = permissionExtractor
			};

			jobParameters.Stub(x => x.StateHolder).Return(stateHolder);
			stateHolder.Stub(x => x.PermissionsMustRun()).Return(true);
			stateHolder.Stub(x => x.UserCollection).Return(personList);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			jobParameters.Stub(x => x.OlapServer).Return("os");
			jobParameters.Stub(x => x.OlapDatabase).Return("od");	
			transformer.Stub(
				x => x.GetUsersWithPermissionsToPerformanceManager(personList, false, permissionExtractor, unitOfWorkFactory))
				.Return(appAuthUsers);
			transformer.Stub(
			x => x.GetUsersWithPermissionsToPerformanceManager(personList, true, permissionExtractor, unitOfWorkFactory))
			.Return(windowsAuthUsers);
			pmWindowsUserSynchronizer.Stub(x => x.Synchronize(windowsAuthUsers, transformer, "os", "od"))
				.Return(new List<UserDto> {new UserDto()});
			transformer.Stub(x => x.IsPmWindowsAuthenticated("os", "od")).Return(new ResultDto {IsWindowsAuthentication = true});
			jobParameters.Stub(x => x.Helper).Return(jobHelper);
			jobHelper.Stub(x => x.Repository).Return(raptorRep);
			raptorRep.Stub(x => x.PersistPmUser(target.BulkInsertDataTable1)).Return(99);

			var result = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);

			appAuthUsers.Count.Should().Be.EqualTo(2);
			transformer.AssertWasCalled(x => x.Transform(appAuthUsers, target.BulkInsertDataTable1));
			result.RowsAffected.Should().Be.EqualTo(99);

		}

		[Test]
		public void ShouldSynchronizeWhenPmAuthenticationIsAnonymous()
		{
			var jobParameters = MockRepository.GenerateMock<IJobParameters>();
			var stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var transformer = MockRepository.GenerateMock<IPmPermissionTransformer>();
			var pmWindowsUserSynchronizer = MockRepository.GenerateMock<IPmWindowsUserSynchronizer>();
			var permissionExtractor = MockRepository.GenerateMock<IPmPermissionExtractor>();
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();
			var raptorRep = MockRepository.GenerateMock<IRaptorRepository>();
			var personList = new List<IPerson>();
			var windowsAuthUsers = new List<UserDto> { new UserDto(), new UserDto() };
			var appAuthUsers = new List<UserDto> { new UserDto() };
			var target = new PmPermissionJobStepForTest(jobParameters, unitOfWorkFactory, true)
			{
				Transformer = transformer,
				PmWindowsUserSynchronizer = pmWindowsUserSynchronizer,
				PermissionExtractor = permissionExtractor
			};

			jobParameters.Stub(x => x.StateHolder).Return(stateHolder);
			stateHolder.Stub(x => x.PermissionsMustRun()).Return(true);
			stateHolder.Stub(x => x.UserCollection).Return(personList);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			jobParameters.Stub(x => x.OlapServer).Return("os");
			jobParameters.Stub(x => x.OlapDatabase).Return("od");
			transformer.Stub(
				x => x.GetUsersWithPermissionsToPerformanceManager(personList, false, permissionExtractor, unitOfWorkFactory))
				.Return(appAuthUsers);
			transformer.Stub(
			x => x.GetUsersWithPermissionsToPerformanceManager(personList, true, permissionExtractor, unitOfWorkFactory))
			.Return(windowsAuthUsers);
			pmWindowsUserSynchronizer.Stub(x => x.Synchronize(windowsAuthUsers, transformer, "os", "od")).Return(new List<UserDto>());
			transformer.Stub(x => x.IsPmWindowsAuthenticated("os", "od")).Return(new ResultDto { IsWindowsAuthentication = false });
			jobParameters.Stub(x => x.Helper).Return(jobHelper);
			jobHelper.Stub(x => x.Repository).Return(raptorRep);
			raptorRep.Stub(x => x.PersistPmUser(target.BulkInsertDataTable1)).Return(99);

			var result = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);

			appAuthUsers.Count.Should().Be.EqualTo(3);
			transformer.AssertWasCalled(x => x.Transform(appAuthUsers, target.BulkInsertDataTable1));
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
