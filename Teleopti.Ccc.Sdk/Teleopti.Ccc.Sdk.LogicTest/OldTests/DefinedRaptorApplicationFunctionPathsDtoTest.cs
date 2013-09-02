using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class DefinedRaptorApplicationFunctionPathsDtoTest
    {
        private DefinedRaptorApplicationFunctionPathsDto _target;

        [SetUp]
        public void Setup()
        {
            _target = new DefinedRaptorApplicationFunctionPathsDto();
        }

        [Test]
        public void VerifyCanSetProperties()
        {
            _target.OpenRaptorApplication = DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication;
            Assert.AreEqual(_target.OpenRaptorApplication, DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);

            _target.RaptorGlobal = DefinedRaptorApplicationFunctionPaths.RaptorGlobal;
            Assert.AreEqual(_target.RaptorGlobal, DefinedRaptorApplicationFunctionPaths.RaptorGlobal);

            _target.ModifyPersonAbsence = DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence;
            Assert.AreEqual(_target.ModifyPersonAbsence, DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);

			_target.ModifyPersonDayOff = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;
			Assert.AreEqual(_target.ModifyPersonDayOff, DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);

            _target.ModifyPersonAssignment = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;
            Assert.AreEqual(_target.ModifyPersonAssignment, DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);

            _target.ViewUnpublishedSchedules = DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules;
            Assert.AreEqual(_target.ViewUnpublishedSchedules, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

            _target.AccessToReports = DefinedRaptorApplicationFunctionPaths.AccessToReports;
            Assert.AreEqual(_target.AccessToReports, DefinedRaptorApplicationFunctionPaths.AccessToReports);

            _target.OpenAgentPortal = DefinedRaptorApplicationFunctionPaths.OpenAgentPortal;
            Assert.AreEqual(_target.OpenAgentPortal, DefinedRaptorApplicationFunctionPaths.OpenAgentPortal);

            _target.OpenAsm = DefinedRaptorApplicationFunctionPaths.OpenAsm;
            Assert.AreEqual(_target.OpenAsm, DefinedRaptorApplicationFunctionPaths.OpenAsm);

            _target.ModifyShiftCategoryPreferences = DefinedRaptorApplicationFunctionPaths.ModifyShiftCategoryPreferences;
            Assert.AreEqual(_target.ModifyShiftCategoryPreferences, DefinedRaptorApplicationFunctionPaths.ModifyShiftCategoryPreferences);

            _target.ModifyExtendedPreferences = DefinedRaptorApplicationFunctionPaths.ModifyExtendedPreferences;
            Assert.AreEqual(_target.ModifyExtendedPreferences, DefinedRaptorApplicationFunctionPaths.ModifyExtendedPreferences);

            _target.OpenMyReport = DefinedRaptorApplicationFunctionPaths.OpenMyReport;
            Assert.AreEqual(_target.OpenMyReport, DefinedRaptorApplicationFunctionPaths.OpenMyReport);

            _target.CreateTextRequest = DefinedRaptorApplicationFunctionPaths.CreateTextRequest;
            Assert.AreEqual(_target.CreateTextRequest, DefinedRaptorApplicationFunctionPaths.CreateTextRequest);

            _target.CreateShiftTradeRequest = DefinedRaptorApplicationFunctionPaths.CreateShiftTradeRequest;
            Assert.AreEqual(_target.CreateShiftTradeRequest, DefinedRaptorApplicationFunctionPaths.CreateShiftTradeRequest);

            _target.CreateAbsenceRequest = DefinedRaptorApplicationFunctionPaths.CreateAbsenceRequest;
            Assert.AreEqual(_target.CreateAbsenceRequest, DefinedRaptorApplicationFunctionPaths.CreateAbsenceRequest);

            _target.OpenScorecard = DefinedRaptorApplicationFunctionPaths.OpenScorecard;
            Assert.AreEqual(_target.OpenScorecard, DefinedRaptorApplicationFunctionPaths.OpenScorecard);

            _target.CreateStudentAvailability = DefinedRaptorApplicationFunctionPaths.CreateStudentAvailability;
            Assert.AreEqual(_target.CreateStudentAvailability, DefinedRaptorApplicationFunctionPaths.CreateStudentAvailability);
            
            _target.ViewCustomTeamSchedule = DefinedRaptorApplicationFunctionPaths.ViewCustomTeamSchedule;
            Assert.AreEqual(_target.ViewCustomTeamSchedule, DefinedRaptorApplicationFunctionPaths.ViewCustomTeamSchedule);
        }
    }
}