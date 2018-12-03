using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.DomainTest.Reports
{
	[TestFixture]
	public class ReportNavigationModelTest
	{
		private ReportNavigationModel _target;
		private MockRepository _mocks;
		private ILicenseActivator _licenseActivator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new ReportNavigationModel(new List<IReportVisible>
			{
				new ReportScheduledTimeVsTargetVisible()
			}, new ScheduleAnalysisAuditTrailProvider());
			_licenseActivator = _mocks.DynamicMock<ILicenseActivator>();
			DefinedLicenseDataFactory.SetLicenseActivator("for test", _licenseActivator);
		}

		[Test]
		public void ShouldProvidePermittedReportFunctions()
		{
			var authorization = _mocks.StrictMock<IAuthorization>();
			IEnumerable<IApplicationFunction> actualReportFunctions;
			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(new List<IApplicationFunction>());
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					actualReportFunctions = _target.PermittedReportFunctions;
				}
			}
			Assert.That(actualReportFunctions, Is.Empty);
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
					acctualFunctions = _target.PermittedRealTimeReportFunctions;
				}
			}
			Assert.AreEqual(expectedFunctions, acctualFunctions);
		}

		[Test]
		public void ShouldProvideGroupedPermittedReportFunctions()
		{
			var authorization = _mocks.StrictMock<IAuthorization>();
			var reportFunctions = new List<IApplicationFunction>
												  {
														new ApplicationFunction {ForeignId = "C5B88862-F7BE-431B-A63F-3DD5FF8ACE54", ForeignSource = DefinedForeignSourceNames.SourceMatrix},
														new ApplicationFunction {ForeignId = "AnIdNotMappedToAGroup", ForeignSource = DefinedForeignSourceNames.SourceMatrix}
												  };
			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(reportFunctions).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IEnumerable<IMatrixFunctionGroup> actualReportFunctionGroups = _target.PermittedCategorizedReportFunctions.ToList();

					Assert.That(actualReportFunctionGroups.Count(), Is.EqualTo(1));
					Assert.That(actualReportFunctionGroups.ElementAt(0).ApplicationFunctions.Single(),
									Is.SameAs(reportFunctions.First()));
					Assert.That(actualReportFunctionGroups.ElementAt(0).ApplicationFunctions.Single().IsWebReport,
						Is.EqualTo(false));
				}
			}
		}

		[Test]
		public void ShouldProvideAuditTrailInPermittedReportFunctions()
		{
			var authorization = _mocks.StrictMock<IAuthorization>();
			
			var reportFunctions = new List<IApplicationFunction>
			{
				new ApplicationFunction {ForeignId = "0148", ForeignSource = DefinedForeignSourceNames.SourceRaptor}
			};

			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(reportFunctions).Repeat.AtLeastOnce();
			}
			_target = new ReportNavigationModel(new List<IReportVisible>
			{
				new ReportScheduledTimeVsTargetVisible()
			}, new ScheduleAnalysisAuditTrailProvider());
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
				
					IEnumerable<IMatrixFunctionGroup> actualReportFunctionGroups = _target.PermittedCategorizedReportFunctions.ToList();

					Assert.That(actualReportFunctionGroups.Count(), Is.EqualTo(1));
					Assert.That(actualReportFunctionGroups.ElementAt(0).ApplicationFunctions.Single(),
						Is.SameAs(reportFunctions.First()));
					Assert.That(actualReportFunctionGroups.ElementAt(0).ApplicationFunctions.Single().IsWebReport,
						Is.EqualTo(true));
				}
			}
		}

		[Test]
		public void ShouldNotStoreReportFunctions()
		{
			var authorization = _mocks.StrictMock<IAuthorization>();
			var reportFunctions = new List<IApplicationFunction>
			{
				new ApplicationFunction {ForeignId = "C5B88862-F7BE-431B-A63F-3DD5FF8ACE54", ForeignSource = DefinedForeignSourceNames.SourceMatrix}
			};

			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(reportFunctions).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IEnumerable<IMatrixFunctionGroup> actualReportFunctionGroups = _target.PermittedCategorizedReportFunctions.ToList();

					Assert.That(actualReportFunctionGroups.Count(), Is.EqualTo(1));
				}
			}

			_mocks.VerifyAll();
			_mocks.BackToRecordAll();
			reportFunctions.Add(new ApplicationFunction() { ForeignId = "009BCDD2-3561-4B59-A719-142CD9216727", ForeignSource = DefinedForeignSourceNames.SourceMatrix });

			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(reportFunctions).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IEnumerable<IMatrixFunctionGroup> actualReportFunctionGroups = _target.PermittedCategorizedReportFunctions.ToList();

					Assert.That(actualReportFunctionGroups.Count(), Is.EqualTo(2));
				}
			}


		}

		[Test]
		public void ShouldProvidePermittedCustomReportFunctions()
		{

			var authorization = _mocks.StrictMock<IAuthorization>();
			var reportFunctions = new List<IApplicationFunction>
												  {
														new ApplicationFunction {ForeignId = "C5B88862-F7BE-431B-A63F-3DD5FF8ACE54", ForeignSource = DefinedForeignSourceNames.SourceMatrix},
														new ApplicationFunction {ForeignId = "AnIdNotMappedToAGroup", ForeignSource = DefinedForeignSourceNames.SourceMatrix}
												  };
			using (_mocks.Record())
			{
				Expect.Call(authorization.GrantedFunctions()).IgnoreArguments().Return(reportFunctions).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					var customReportFunctions = _target.PermittedCustomReportFunctions.ToList();

					Assert.That(customReportFunctions.Count(), Is.EqualTo(1));
					Assert.That(customReportFunctions.Single(), Is.SameAs(reportFunctions.ElementAt(1)));
				}
			}
		}
	}
}