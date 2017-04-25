using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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
			ErrorMessages = new List<string>();
			WarningAgents = new List<AgentExtractionResult>();
			FailedAgents = new List<AgentExtractionResult>();
			SucceedAgents = new List<AgentExtractionResult>();
		}

		public List<string> ErrorMessages { get; set; }
		public DetailLevel DetailLevel { get; set; }
		public List<AgentExtractionResult> WarningAgents { get; }
		public List<AgentExtractionResult> FailedAgents { get; }
		public List<AgentExtractionResult> SucceedAgents { get; }
		public int SuccessCount => SucceedAgents?.Count ?? 0;
		public int FailedCount => FailedAgents?.Count ?? 0;
		public int WarningCount => WarningAgents?.Count ?? 0;
	}

	public class AgentFileExtractionResult
	{
		public AgentFileExtractionResult()
		{
			Feedback = new Feedback();
			RawResults = new List<AgentExtractionResult>();
		}

		public IList<AgentExtractionResult> RawResults { get; }
		public Feedback Feedback { get; }

		public bool HasError => Feedback.ErrorMessages.Any();
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