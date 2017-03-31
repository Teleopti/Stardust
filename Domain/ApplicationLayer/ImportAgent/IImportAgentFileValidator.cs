using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
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

		public void Merge(Feedback other)
		{
			ErrorMessages.AddRange(other.ErrorMessages);
			WarningMessages.AddRange(other.WarningMessages);
		}
	}

	public class AgentFileProcessResult
	{
		public AgentFileProcessResult()
		{
			ErrorMessages = new List<string>();
		}

		public IList<string> ErrorMessages { get; set; }
		public DetailLevel DetailLevel { get; set; }
		public List<AgentExtractionResult> WarningAgents { get; internal set; }
		public List<AgentExtractionResult> FaildAgents { get; internal set; }
		public List<AgentExtractionResult> SucceedAgents { get; internal set; }
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