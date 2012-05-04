using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IAgentRestrictionsView
	{
		
	}

	public class AgentRestrictionsPresenter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private IAgentRestrictionsView _view;
		private readonly IAgentRestrictionsModel _model;
		private const int ColCount = 12;

		public AgentRestrictionsPresenter(IAgentRestrictionsView view, IAgentRestrictionsModel model)
		{
			_view = view;
			_model = model;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public int GridQueryColCount
		{
			get { return ColCount; }
		}

		public int GridQueryRowCount
		{
			get { return _model.DisplayRows.Count; }
		}
	}
}
