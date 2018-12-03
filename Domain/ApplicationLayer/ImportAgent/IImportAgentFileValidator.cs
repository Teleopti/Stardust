using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class Feedback
	{
		public List<string> ErrorMessages { get; }
		public List<string> WarningMessages { get; }

		public Feedback()
		{
			ErrorMessages = new List<string>();
			WarningMessages = new List<string>();
		}

		public void Merge(params Feedback[] others)
		{
			foreach (var other in others)
			{
				ErrorMessages.AddRange(other.ErrorMessages);
				WarningMessages.AddRange(other.WarningMessages);
			}

		}


	}

	
	public class AgentFileProcessResult : IImportAgentResultCount
	{
		public AgentFileProcessResult()
		{
			Feedback = new Feedback();
			RawResults = new List<AgentExtractionResult>();
		}
		public IList<AgentExtractionResult> RawResults { get; }
		public List<AgentExtractionResult> WarningAgents => RawResults.Where(a => a.DetailLevel == DetailLevel.Warning).ToList();
		public List<AgentExtractionResult> FailedAgents => RawResults.Where(a => a.DetailLevel == DetailLevel.Error).ToList();
		public List<AgentExtractionResult> SucceedAgents => RawResults.Where(a => a.DetailLevel == DetailLevel.Info).ToList();
		public Feedback Feedback { get; }
		public bool HasError => Feedback.ErrorMessages.Any();

		public DetailLevel DetailLevel => HasError || !FailedAgents.IsNullOrEmpty()
			? DetailLevel.Error
			: !WarningAgents.IsNullOrEmpty()
				? DetailLevel.Warning
				: DetailLevel.Info;

		public int SuccessCount => SucceedAgents.Count;
		public int FailedCount => FailedAgents.Count;
		public int WarningCount => WarningAgents.Count;

		public TimeZoneInfo TimezoneForCreator { get; set; }
		public dynamic InputArtifactInfo { get; set; }
	}

	public class AgentExtractionResult
	{
		public AgentDataModel Agent { get; set; }

		public IRow Row { get; set; }
		public RawAgent Raw { get; set; }
		public Feedback Feedback { get; }

		public AgentExtractionResult()
		{
			Feedback = new Feedback();
		}

		public DetailLevel DetailLevel
		{
			get
			{
				if (Feedback.ErrorMessages.Any())
				{
					return DetailLevel.Error;
				}
				if (Feedback.WarningMessages.Any())
				{
					return DetailLevel.Warning;
				}
				return DetailLevel.Info;
			}
		}
	}

	public interface IImportAgentFileValidator
	{
		AgentDataModel MapRawData(RawAgent raw, out Feedback feedback);
		void SetDefaultValues(ImportAgentDefaults defaultValues);
	}
}