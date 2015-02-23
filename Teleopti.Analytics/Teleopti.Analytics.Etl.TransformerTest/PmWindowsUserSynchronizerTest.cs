using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.PM.PMServiceHost;

namespace Teleopti.Analytics.Etl.TransformerTest
{
	[TestFixture]
	public class PmWindowsUserSynchronizerTest
	{
		private PmWindowsUserSynchronizer _target;
		private IPmPermissionTransformer _transformer;

		[SetUp]
		public void Setup()
		{
			_target = new PmWindowsUserSynchronizer();
			_transformer = MockRepository.GenerateMock<IPmPermissionTransformer>();
		}

		[Test, ExpectedException(typeof(PmSynchronizeException))]
		public void ShouldThrowExceptionSynchronizationOfPermissionsFails()
		{
			var users = new List<UserDto>();
			_transformer.Stub(x => x.SynchronizeUserPermissions(users, "olapServer", "oladDb")).Return(new ResultDto { Success = false });
			_target.Synchronize(users, _transformer, "olapServer", "oladDb");
		}

		[Test]
		public void ShouldReturnTheSynchronizedUsers()
		{
			var windowsAuthUsers = new List<UserDto>();
			var resultDto = new ResultDto { Success = true };
			resultDto.ValidAnalyzerUsers.Add(new UserDto());

			_transformer.Stub(x => x.SynchronizeUserPermissions(windowsAuthUsers, "olapServer", "oladDb")).Return(resultDto);

			var result = _target.Synchronize(windowsAuthUsers, _transformer, "olapServer", "oladDb");

			result.Should().Be.SameInstanceAs(resultDto.ValidAnalyzerUsers);
		}
	}
}
