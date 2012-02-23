using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Matrix;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Matrix
{
    [TestFixture]
    public class MatrixNavigationPresenterTest
    {
        private MatrixNavigationPresenter _target;
        private MockRepository _mocks;
        private IMatrixNavigationModel _model;
        private IMatrixNavigationView _view;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _model = _mocks.StrictMock<IMatrixNavigationModel>();
            _view = _mocks.StrictMock<IMatrixNavigationView>();
            _target = new MatrixNavigationPresenter(_model, _view);
        }

        [Test]
        public void ShouldHandleLinkClick()
        {
            var applicationFunction = _mocks.Stub<IApplicationFunction>();
            applicationFunction.ForeignId = "1";
            using (_mocks.Record())
            {
                Expect.Call(_model.AuthenticationType).Return(AuthenticationTypeOption.Windows);
                var guid = Guid.NewGuid();
                Expect.Call(_model.BusinessUnitId).Return(guid);
                Expect.Call(_model.MatrixWebsiteUrl).Return(new Uri("http://matrixWebsiteUrl/?ReportID={0}&forceformslogin={1}&buid={2}"));
                Expect.Call(() =>_view.OpenUrl(new Uri("http://matrixWebsiteUrl/?ReportID=1&forceformslogin=false&buid=" + guid)));
            }
            using (_mocks.Playback())
            {
                _target.LinkClick(applicationFunction, false);
            }
        }

        [Test]
        public void ShouldInitialize()
        {
            var groupedMatrixGroups = new List<IMatrixFunctionGroup>();
            var orphanMatrixGroups = new List<IApplicationFunction>();
            var onlineReportsFunctions = new List<IApplicationFunction>();
            _mocks.Record();
            Expect.Call(_model.GroupedPermittedMatrixFunctions).Return(groupedMatrixGroups);
            Expect.Call(_model.OrphanPermittedMatrixFunctions).Return(orphanMatrixGroups);
            Expect.Call(_model.PermittedOnlineReportFunctions).Return(onlineReportsFunctions);
            Expect.Call(() => _view.SetColor()).IgnoreArguments();
            Expect.Call(() => _view.CreateAndAddTreeNodes(null, null)).IgnoreArguments();
            _mocks.ReplayAll();
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCreateTree()
        {
            var groupedMatrixFunction = _mocks.Stub<IApplicationFunction>();
            SetupResult.For(groupedMatrixFunction.LocalizedFunctionDescription).Return("Grouped Report Name");
            var matrixFunctionGroup = new MatrixFunctionGroup
                                          {
                                              LocalizedDescription = Resources.ScheduleAnalysis,
                                              ApplicationFunctions =
                                                  new List<IApplicationFunction> {groupedMatrixFunction}
                                          };
            var groupedMatrixFunctions = new List<IMatrixFunctionGroup> {matrixFunctionGroup};

            var orphanMatrixFunction = _mocks.Stub<IApplicationFunction>();
            SetupResult.For(orphanMatrixFunction.LocalizedFunctionDescription).Return("Orphan Report Name");
            var orphanMatrixFunctions = new List<IApplicationFunction> { orphanMatrixFunction };

            var tree = _target.CreateTree(groupedMatrixFunctions, orphanMatrixFunctions);

            Assert.That(tree.Count(), Is.EqualTo(2));
            var rootNode = tree.First();
            Assert.That(rootNode.DisplayName, Is.EqualTo(Resources.HistoricalReports));
            Assert.That(rootNode.ImageIndex, Is.EqualTo(0));
            Assert.That(rootNode.ApplicationFunction, Is.Null);
            var groupNode = rootNode.Nodes.First();
            Assert.That(groupNode.DisplayName, Is.EqualTo(matrixFunctionGroup.LocalizedDescription));
            var matrixFunctionNode = groupNode.Nodes.Single();
            Assert.That(matrixFunctionNode.DisplayName, Is.EqualTo(groupedMatrixFunction.LocalizedFunctionDescription));
            var orphanMatrixFunctionNode = tree.ElementAt(1);
            Assert.That(orphanMatrixFunctionNode.DisplayName,
                        Is.EqualTo(Resources.CustomReports));
        }

        [Test]
        public void ShouldCreateRealTimeTree()
        {
            var realTimeFunction = _mocks.Stub<IApplicationFunction>();
            SetupResult.For(realTimeFunction.LocalizedFunctionDescription).Return("Real Time Report Name");
            var realTimeFunctions = new List<IApplicationFunction> { realTimeFunction };

            var tree = _target.CreateTree(realTimeFunctions);

            Assert.AreEqual(1, tree.Count());
            var rootNode = tree.First();
            Assert.AreEqual(Resources.RealTime, rootNode.DisplayName);
            Assert.AreEqual(0, rootNode.ImageIndex);
            Assert.IsNull(rootNode.ApplicationFunction);
            var realTimeFunctionNode = rootNode.Nodes.First();
            Assert.AreEqual(realTimeFunction.LocalizedFunctionDescription, realTimeFunctionNode.DisplayName);
        }

        [Test]
        public void ShouldCreateTreeNodeFromApplicationFunction()
        {
            var applicationFunction = _mocks.Stub<IApplicationFunction>();
            SetupResult.For(applicationFunction.LocalizedFunctionDescription).Return("Localized Description");
            var node = _target.CreateTreeNode(applicationFunction);
            Assert.That(node.DisplayName, Is.EqualTo(applicationFunction.LocalizedFunctionDescription));
            Assert.That(node.ImageIndex, Is.EqualTo(2));
            Assert.That(node.ApplicationFunction, Is.SameAs(applicationFunction));
            Assert.IsFalse(node.RealTime);
        }

        [Test]
        public void ShouldCreateTreeNodeFromMatrixFunctionGroup()
        {
            var matrixFunctionGroup = _mocks.Stub<IMatrixFunctionGroup>();
            matrixFunctionGroup.LocalizedDescription = "Group Description";
            var node = _target.CreateTreeNode(matrixFunctionGroup);
            Assert.That(node.DisplayName, Is.EqualTo(matrixFunctionGroup.LocalizedDescription));
            Assert.That(node.ImageIndex, Is.EqualTo(1));
            Assert.That(node.ApplicationFunction, Is.Null);
        }
    }
}