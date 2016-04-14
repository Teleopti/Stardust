using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class DefinedRaptorApplicationFunctionPathsDtoTest
    {
        [Test]
        public void VerifyCanSetProperties()
        {
			var target = new DefinedRaptorApplicationFunctionPathsDto();
			target.OpenRaptorApplication = DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication;
            Assert.AreEqual(target.OpenRaptorApplication, DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);

            target.RaptorGlobal = DefinedRaptorApplicationFunctionPaths.RaptorGlobal;
            Assert.AreEqual(target.RaptorGlobal, DefinedRaptorApplicationFunctionPaths.RaptorGlobal);

            target.ModifyPersonAbsence = DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence;
            Assert.AreEqual(target.ModifyPersonAbsence, DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);

			target.ModifyPersonDayOff = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;
			Assert.AreEqual(target.ModifyPersonDayOff, DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);

            target.ModifyPersonAssignment = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;
            Assert.AreEqual(target.ModifyPersonAssignment, DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);

            target.ViewUnpublishedSchedules = DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules;
            Assert.AreEqual(target.ViewUnpublishedSchedules, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

            target.AccessToReports = DefinedRaptorApplicationFunctionPaths.AccessToReports;
            Assert.AreEqual(target.AccessToReports, DefinedRaptorApplicationFunctionPaths.AccessToReports);
        }
    }
}