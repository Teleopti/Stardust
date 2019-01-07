using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Matrix
{
	public interface IMatrixNavigationView
	{
		void SetColor();
		void CreateAndAddTreeNodes(IEnumerable<MatrixTreeNode> treeNodes, IEnumerable<MatrixTreeNode> treeNodes2);
		void OpenUrl(Uri url);
		void OpenRealTime(IApplicationFunction func);
	}

	public class MatrixTreeNode
	{
		public string DisplayName { get; set; }
		public int ImageIndex { get; set; }
		public IApplicationFunction ApplicationFunction { get; set; }
		public IEnumerable<MatrixTreeNode> Nodes { get; set; }
		public bool RealTime { get; set; }
	}

	public interface IMatrixNavigationPresenter
	{
		void LinkClick(IApplicationFunction applicationFunction, bool realTime);
		void Initialize();

		IEnumerable<MatrixTreeNode> CreateTree(IEnumerable<IMatrixFunctionGroup> matrixFunctionGroups,
																				 IEnumerable<IApplicationFunction> orphanMatrixFunctions);

		MatrixTreeNode CreateTreeNode(IApplicationFunction applicationFunction);

		MatrixTreeNode CreateTreeNode(IMatrixFunctionGroup matrixFunctionGroup,
																	 IEnumerable<MatrixTreeNode> childNodes);

		MatrixTreeNode CreateTreeNode(IMatrixFunctionGroup matrixFunctionGroup);
	}

	public class MatrixNavigationPresenter : IMatrixNavigationPresenter
	{
		private readonly IReportNavigationModel _model;
		private readonly IMatrixNavigationView _view;
		private readonly IReportUrl _reportUrlConstructor;
		private readonly IToggleManager _toggleManager;

		public MatrixNavigationPresenter(IReportNavigationModel model, IMatrixNavigationView view,
			IReportUrl reportUrlConstructor, IToggleManager toggleManager)
		{
			_model = model;
			_view = view;
			_reportUrlConstructor = reportUrlConstructor;
			_toggleManager = toggleManager;
		}

		public void LinkClick(IApplicationFunction applicationFunction, bool realTime)
		{
			if (realTime)
			{
				_view.OpenRealTime(applicationFunction);
			}
			else
			{
				if (applicationFunction.ForeignId == DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailWebReport || 
					applicationFunction.ForeignId == DefinedRaptorApplicationFunctionForeignIds.GeneralAuditTrailWebReport)
					applicationFunction.IsWebReport = false;
				
				var url = _reportUrlConstructor.Build(applicationFunction);
				_view.OpenUrl(new Uri(url));
			}
		}

		public void Initialize()
		{
			_view.SetColor();
			var matrixFunctionGroups = _model.PermittedCategorizedReportFunctions.ToList();
			
			if(!_toggleManager.IsEnabled(Toggles.Wfm_AuditTrail_GenericAuditTrail_74938))
			{
				var auditTrailGroup =
					matrixFunctionGroups.SingleOrDefault(g => g.ApplicationFunctions.Any(af => af.ForeignId == DefinedRaptorApplicationFunctionForeignIds.GeneralAuditTrailWebReport));
				if(auditTrailGroup != null)
				{
					auditTrailGroup.ApplicationFunctions =
						auditTrailGroup.ApplicationFunctions.Where(af => af.ForeignId != DefinedRaptorApplicationFunctionForeignIds.GeneralAuditTrailWebReport);
				}
			}
			var orphanMatrixFunctions = _model.PermittedCustomReportFunctions;
			var tree = CreateTree(matrixFunctionGroups, orphanMatrixFunctions);
			var treeRealTime = CreateTree(_model.PermittedRealTimeReportFunctions);

			_view.CreateAndAddTreeNodes(treeRealTime, tree);
		}

		public IEnumerable<MatrixTreeNode> CreateTree(IEnumerable<IMatrixFunctionGroup> matrixFunctionGroups,
																	 IEnumerable<IApplicationFunction> orphanMatrixFunctions)
		{
			var rootTreeNode = new MatrixTreeNode
										  {
											  DisplayName = Resources.HistoricalReports,
											  ImageIndex = 0,
											  Nodes = (from g in matrixFunctionGroups
														  let childNodes =
																(from f in g.ApplicationFunctions select CreateTreeNode(f))
														  let node = CreateTreeNode(g, childNodes)
														  select node)
										  };
			
			var customTreeNode = new MatrixTreeNode
											 {
												 DisplayName = Resources.CustomReports,
												 ImageIndex = 0,
												 Nodes = from o in orphanMatrixFunctions
															let node = CreateTreeNode(o)
															select node
											 };
			return new List<MatrixTreeNode> { rootTreeNode, customTreeNode };
		}

		public IEnumerable<MatrixTreeNode> CreateTree(IEnumerable<IApplicationFunction> permittedOnlineReportFunctions)
		{
			if (!permittedOnlineReportFunctions.Any())
				return new List<MatrixTreeNode> { };
			var rootTreeNode = new MatrixTreeNode {DisplayName = Resources.RealTime, ImageIndex = 0};
			IList<MatrixTreeNode> nodes = new List<MatrixTreeNode>();

			foreach (IApplicationFunction func in permittedOnlineReportFunctions)
			{
				MatrixTreeNode node = CreateTreeNode(func);
				node.RealTime = true;
				nodes.Add(node);
			}

			rootTreeNode.Nodes = nodes;
			return new List<MatrixTreeNode> { rootTreeNode };
		}

		public MatrixTreeNode CreateTreeNode(IApplicationFunction applicationFunction)
		{
			var treeNode = new MatrixTreeNode
									 {
										 DisplayName = applicationFunction.LocalizedFunctionDescription,
										 ImageIndex = 2,
										 ApplicationFunction = applicationFunction,
										 RealTime = false
									 };
			return treeNode;
		}

		public MatrixTreeNode CreateTreeNode(IMatrixFunctionGroup matrixFunctionGroup,
														 IEnumerable<MatrixTreeNode> childNodes)
		{
			var treeNode = CreateTreeNode(matrixFunctionGroup);
			treeNode.Nodes = childNodes;
			return treeNode;
		}

		public MatrixTreeNode CreateTreeNode(IMatrixFunctionGroup matrixFunctionGroup)
		{
			var treeNode = new MatrixTreeNode
									 {
										 DisplayName = matrixFunctionGroup.LocalizedDescription,
										 ImageIndex = 1
									 };
			return treeNode;
		}
	}
}