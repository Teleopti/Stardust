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
        }
    }
}