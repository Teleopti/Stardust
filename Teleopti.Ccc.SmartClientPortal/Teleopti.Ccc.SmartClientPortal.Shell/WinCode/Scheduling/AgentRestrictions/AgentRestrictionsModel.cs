using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsModel
	{

		IList<AgentRestrictionsDisplayRow> DisplayRows { get; }
		AgentRestrictionsDisplayRow DisplayRowFromRowIndex(int rowIndex);
		IList<AgentRestrictionsDisplayRow> LoadDisplayRows(IAgentRestrictionsDisplayRowCreator agentRestrictionsDisplayRowCreator, IList<IPerson> persons);
		void SortAgentName(bool ascending);
		void SortWarnings(bool ascending);
		void SortPeriodType(bool ascending);
		void SortStartDate(bool ascending);
		void SortEndDate(bool ascending);
		void SortContractTargetTime(bool ascending);
		void SortTargetDayOffs(bool ascending);
		void SortContractCurrentTime(bool ascending);
		void SortCurrentDayOffs(bool ascending);
		void SortMinimumPossibleTime(bool ascending);
		void SortMaximumPossibleTime(bool ascending);
		void SortScheduledAndRestrictionDayOffs(bool ascending);
		void SortOk(bool ascending);
	}

	public class AgentRestrictionsModel : IAgentRestrictionsModel
	{
		private IList<AgentRestrictionsDisplayRow> _displayRows;
		public IList<AgentRestrictionsDisplayRow> DisplayRows { get { return _displayRows; } }
		
		public AgentRestrictionsModel()
		{
			_displayRows = new List<AgentRestrictionsDisplayRow>();		
		}

		public AgentRestrictionsDisplayRow DisplayRowFromRowIndex(int rowIndex)
		{
			if (rowIndex < int.MinValue + 2) throw new ArgumentOutOfRangeException("rowIndex");
			if (_displayRows.Count == 0) return null;
			return _displayRows[rowIndex - 1 - 1];
		}

		public IList<AgentRestrictionsDisplayRow> LoadDisplayRows(IAgentRestrictionsDisplayRowCreator agentRestrictionsDisplayRowCreator, IList<IPerson> persons)
		{
			if (agentRestrictionsDisplayRowCreator == null) throw new ArgumentNullException("agentRestrictionsDisplayRowCreator");
			var rows = agentRestrictionsDisplayRowCreator.Create(persons);

			foreach (var agentRestrictionsDisplayRow in rows)
			{
				_displayRows.Add(agentRestrictionsDisplayRow);
			}

			return rows;
		}

		public void SortAgentName(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.AgentName ascending select d).ToList() : (from d in _displayRows orderby d.AgentName descending select d).ToList();	
		}

		public void SortWarnings(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.Warnings ascending select d).ToList() : (from d in _displayRows orderby d.Warnings descending select d).ToList();	
		}

		public void SortPeriodType(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.PeriodType ascending select d).ToList() : (from d in _displayRows orderby d.PeriodType descending select d).ToList();	
		}

		public void SortStartDate(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.Matrix.SchedulePeriod.DateOnlyPeriod.StartDate ascending select d).ToList() : (from d in _displayRows orderby d.Matrix.SchedulePeriod.DateOnlyPeriod.StartDate descending select d).ToList();	
		}

		public void SortEndDate(bool ascending)
		{	
			_displayRows = @ascending ? (from d in _displayRows orderby d.Matrix.SchedulePeriod.DateOnlyPeriod.EndDate ascending select d).ToList() : (from d in _displayRows orderby d.Matrix.SchedulePeriod.DateOnlyPeriod.EndDate descending select d).ToList();	
		}

		public void SortContractTargetTime(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.ContractTargetTime ascending select d).ToList() : (from d in _displayRows orderby d.ContractTargetTime descending select d).ToList();	
		}

		public void SortTargetDayOffs(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.TargetDaysOff ascending select d).ToList() : (from d in _displayRows orderby d.TargetDaysOff descending select d).ToList();	
		}

		public void SortContractCurrentTime(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.ContractCurrentTime ascending select d).ToList() : (from d in _displayRows orderby d.ContractCurrentTime descending select d).ToList();	
		}

		public void SortCurrentDayOffs(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.CurrentDaysOff ascending select d).ToList() : (from d in _displayRows orderby d.CurrentDaysOff descending select d).ToList();	
		}

		public void SortMinimumPossibleTime(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby ((IAgentDisplayData)d).MinimumPossibleTime ascending select d).ToList() : (from d in _displayRows orderby ((IAgentDisplayData)d).MinimumPossibleTime descending select d).ToList();	
		}

		public void SortMaximumPossibleTime(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby ((IAgentDisplayData)d).MaximumPossibleTime ascending select d).ToList() : (from d in _displayRows orderby ((IAgentDisplayData)d).MaximumPossibleTime descending select d).ToList();	
		}

		public void SortScheduledAndRestrictionDayOffs(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby ((IAgentDisplayData)d).ScheduledAndRestrictionDaysOff ascending select d).ToList() : (from d in _displayRows orderby ((IAgentDisplayData)d).ScheduledAndRestrictionDaysOff descending select d).ToList();	
		}

		public void SortOk(bool ascending)
		{
			_displayRows = @ascending ? (from d in _displayRows orderby d.Ok ascending select d).ToList() : (from d in _displayRows orderby d.Ok descending select d).ToList();	
		}
	}
}
