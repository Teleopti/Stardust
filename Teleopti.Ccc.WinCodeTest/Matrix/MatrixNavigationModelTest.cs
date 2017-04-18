using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Matrix;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Matrix
{
	[TestFixture]
	public class MatrixNavigationModelTest
	{
		private MatrixNavigationModel _target;
		private MockRepository _mocks;
		private ILicenseActivator _licenseActivator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new MatrixNavigationModel();
			_licenseActivator = _mocks.DynamicMock<ILicenseActivator>();
			DefinedLicenseDataFactory.SetLicenseActivator("for test", _licenseActivator);
		}

		[Test]
		public void ShouldProvidePermittedMatrixFunctions()
		{
			var authorization = _mocks.StrictMock<IAuthorization>();
			var expectedMatrixFunctions = new List<IApplicationFunction>();
			IEnumerable<IApplicationFunction> actualMatrixFunctions;
			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(expectedMatrixFunctions);
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					actualMatrixFunctions = _target.PermittedMatrixFunctions;
				}
			}
			Assert.That(actualMatrixFunctions, Is.Empty);
		}

		[Test]
		public void ShouldProvidePermittedOnlineReportsFunctions()
		{
			var authorization = _mocks.StrictMock<IAuthorization>();
			var expectedFunctions = new List<IApplicationFunction>();
			IEnumerable<IApplicationFunction> acctualFunctions;
			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(expectedFunctions);
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					acctualFunctions = _target.PermittedOnlineReportFunctions;
				}
			}
			Assert.AreEqual(expectedFunctions, acctualFunctions);
		}

		[Test]
		public void ShouldProvideGroupedPermittedMatrixFunctions()
		{
			var authorization = _mocks.StrictMock<IAuthorization>();
			var matrixFunctions = new List<IApplicationFunction>
												  {
														new ApplicationFunction {ForeignId = "C5B88862-F7BE-431B-A63F-3DD5FF8ACE54", ForeignSource = DefinedForeignSourceNames.SourceMatrix},
														new ApplicationFunction {ForeignId = "AnIdNotMappedToAGroup", ForeignSource = DefinedForeignSourceNames.SourceMatrix}
												  };
			IEnumerable<IMatrixFunctionGroup> actualMatrixFunctionGroups;
			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(matrixFunctions).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					actualMatrixFunctionGroups = _target.GroupedPermittedMatrixFunctions;
					Assert.That(actualMatrixFunctionGroups.Count(), Is.EqualTo(1));
					Assert.That(actualMatrixFunctionGroups.ElementAt(0).ApplicationFunctions.Single(),
									Is.SameAs(matrixFunctions.First()));
				}
			}
		}

		[Test]
		public void ShouldProvideOrphanPermittedMatrixFunctions()
		{

			var authorization = _mocks.StrictMock<IAuthorization>();
			var matrixFunctions = new List<IApplicationFunction>
												  {
														new ApplicationFunction {ForeignId = "C5B88862-F7BE-431B-A63F-3DD5FF8ACE54", ForeignSource = DefinedForeignSourceNames.SourceMatrix},
														new ApplicationFunction {ForeignId = "AnIdNotMappedToAGroup", ForeignSource = DefinedForeignSourceNames.SourceMatrix}
												  };
			IEnumerable<IApplicationFunction> orphanMatrixFunctions;
			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(matrixFunctions).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					orphanMatrixFunctions = _target.OrphanPermittedMatrixFunctions;

					Assert.That(orphanMatrixFunctions.Count(), Is.EqualTo(1));
					Assert.That(orphanMatrixFunctions.Single(), Is.SameAs(matrixFunctions.ElementAt(1)));
				}
			}
		}
	}
}