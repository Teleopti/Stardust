using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface  IAgentRestrictionsDisplayRow
	{
		
	}

	public interface IAgentDisplayData
	{
		IScheduleMatrixPro Matrix { get; }
	}

	public class AgentRestrictionsDisplayRow : IAgentRestrictionsDisplayRow, IAgentDisplayData
	{
		private readonly IScheduleMatrixPro _matrix;

		public AgentRestrictionsDisplayRow(IScheduleMatrixPro matrix)
		{
			_matrix = matrix;
		}

		IScheduleMatrixPro IAgentDisplayData.Matrix
		{
			get { return _matrix; }
		}
	}
}
