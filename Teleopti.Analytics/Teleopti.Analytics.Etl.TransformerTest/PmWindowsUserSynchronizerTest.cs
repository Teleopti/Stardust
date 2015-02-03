using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public void ShouldThrowExceptionIfAuthCheckFails()
		{
			_transformer.Stub(x => x.IsPmWindowsAuthenticated("olapServer", "oladDb")).Return(new ResultDto { Success = false });
			_target.Synchronize(null, _transformer, null, null, "olapServer", "oladDb");
		}

		[Test]
		public void ShouldReturnEmptyUserListIfNotWindowsAuth()
		{
			_transformer.Stub(x => x.IsPmWindowsAuthenticated("olapServer", "oladDb"))
				.Return(new ResultDto {Success = true, IsWindowsAuthentication = false});

			var result = _target.Synchronize(null, _transformer, null, null, "olapServer", "oladDb");
			result.Count.Should().Be.EqualTo(0);
		}

		[Test, ExpectedException(typeof(PmSynchronizeException))]
		public void ShouldThrowExceptionSynchronizationOfPermissionsFails()
		{
			var users = new List<UserDto>();

			_transformer.Stub(x => x.IsPmWindowsAuthenticated("olapServer", "oladDb"))
				.Return(new ResultDto {Success = true, IsWindowsAuthentication = true});
			_transformer.Stub(x => x.GetUsersWithPermissionsToPerformanceManager(null, true, null, null)).Return(users);
			_transformer.Stub(x => x.SynchronizeUserPermissions(users, "olapServer", "oladDb")).Return(new ResultDto { Success = false });

			_target.Synchronize(null, _transformer, null, null, "olapServer", "oladDb");
		}

		[Test]
		public void ShouldReturnTheSynchronizatedUsers()
		{
			var users = new List<UserDto>();
			var resultDto = new ResultDto { Success = true };
			resultDto.ValidAnalyzerUsers.Add(new UserDto());

			_transformer.Stub(x => x.IsPmWindowsAuthenticated("olapServer", "oladDb"))
				.Return(new ResultDto {Success = true, IsWindowsAuthentication = true});
			_transformer.Stub(x => x.GetUsersWithPermissionsToPerformanceManager(null, true, null, null)).Return(users);
			_transformer.Stub(x => x.SynchronizeUserPermissions(users, "olapServer", "oladDb")).Return(resultDto);

			var result = _target.Synchronize(null, _transformer, null, null, "olapServer", "oladDb");

			result.Should().Be.SameInstanceAs(resultDto.ValidAnalyzerUsers);
		}
	}
}
