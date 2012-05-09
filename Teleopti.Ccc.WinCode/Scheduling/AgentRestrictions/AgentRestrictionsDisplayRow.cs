using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface  IAgentRestrictionsDisplayRow
	{
		Name AgentName { get; set; }
	}

	public interface IAgentDisplayData
	{
		IScheduleMatrixPro Matrix { get; }
		TimeSpan MinimumPossibleTime { get; set; }
		TimeSpan MaximumPossibleTime { get; set; }
	}

	public sealed class AgentRestrictionsDisplayRow : IAgentRestrictionsDisplayRow, IAgentDisplayData
	{
		private readonly IScheduleMatrixPro _matrix;
		TimeSpan IAgentDisplayData.MinimumPossibleTime { get; set; }
		TimeSpan IAgentDisplayData.MaximumPossibleTime { get; set; }

		public AgentRestrictionsDisplayRow(IScheduleMatrixPro matrix)
		{
			_matrix = matrix;
		}

		public IScheduleMatrixPro Matrix
		{
			get { return _matrix; }
		}



		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public Name AgentName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}
}
