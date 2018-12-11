using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Matrix;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.WinCodeTest.Matrix
{
	[TestFixture]
	public class MatrixNavigationPresenterTest
	{
		private MatrixNavigationPresenter _target;
		private MockRepository _mocks;
		private IReportNavigationModel _model;
		private IMatrixNavigationView _view;
		private IReportUrl _urlConstructor;
		private IToggleManager _toggleManager;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_model = MockRepository.GenerateMock<IReportNavigationModel>();
			_view = MockRepository.GenerateMock<IMatrixNavigationView>();
			_urlConstructor = MockRepository.GenerateMock<IReportUrl>();
			_toggleManager = new FakeToggleManager();
			_target = new MatrixNavigationPresenter(_model, _view, _urlConstructor, _toggleManager);
		}

		[Test]
		public void ShouldHandleLinkClick()
		{
			var applicationFunction = MockRepository.GenerateMock<IApplicationFunction>();
			applicationFunction.Stub(x => x.ForeignId).Return("1");
			_urlConstructor.Stub(x => x.Build(applicationFunction)).Return("http://matrixWebsiteUrl/Reporting/report/1");
			_view.Stub(x => x.OpenUrl(new Uri("http://matrixWebsiteUrl/Reporting/report/1")));

			_target.LinkClick(applicationFunction, false);

		}

		[Test]
		public void ShouldInitialize()
		{
			var groupedMatrixGroups = new List<IMatrixFunctionGroup>();
			var orphanMatrixGroups = new List<IApplicationFunction>();
			var onlineReportsFunctions = new List<IApplicationFunction>();
	
			_model.Stub(x => x.PermittedCategorizedReportFunctions).Return(groupedMatrixGroups);
			_model.Stub(x => x.PermittedCustomReportFunctions).Return(orphanMatrixGroups);
			_model.Stub(x => x.PermittedRealTimeReportFunctions).Return(onlineReportsFunctions);
			_view.Stub(x => x.SetColor()).IgnoreArguments();
			_view.Stub(x => x.CreateAndAddTreeNodes(null, null)).IgnoreArguments();
	
			_target.Initialize();
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
															 new List<IApplicationFunction> { groupedMatrixFunction }
													};
			var groupedMatrixFunctions = new List<IMatrixFunctionGroup> { matrixFunctionGroup };

			var orphanMatrixFunction = _mocks.Stub<IApplicationFunction>();
			SetupResult.For(orphanMatrixFunction.LocalizedFunctionDescription).Return("Orphan Report Name");
			var orphanMatrixFunctions = new List<IApplicationFunction> { orphanMatrixFunction };

			var tree = _target.CreateTree(groupedMatrixFunctions, orphanMatrixFunctions).ToList();

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

			var tree = _target.CreateTree(realTimeFunctions).ToList();

			Assert.AreEqual(1, tree.Count());
			var rootNode = tree.First();
			Assert.AreEqual(Resources.RealTime, rootNode.DisplayName);
			Assert.AreEqual(0, rootNode.ImageIndex);
			Assert.IsNull(rootNode.ApplicationFunction);
			var realTimeFunctionNode = rootNode.Nodes.First();
			Assert.AreEqual(realTimeFunction.LocalizedFunctionDescription, realTimeFunctionNode.DisplayName);
		}

		[Test]
		public void ShouldNotCreateRealTimeTreeIfNoChildNodes()
		{
			var realTimeFunctions = new List<IApplicationFunction> {  };

			var tree = _target.CreateTree(realTimeFunctions).ToList();

			Assert.AreEqual(0, tree.Count());
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