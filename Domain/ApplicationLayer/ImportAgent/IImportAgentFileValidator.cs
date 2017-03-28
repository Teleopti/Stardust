using System.Collections.Generic;
using NPOI.SS.UserModel;

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

	public class FileProcessResult
	{
		public FileProcessResult()
		{
			ExtractedResults = new List<AgentExtractionResult>();
		}
		
		public IList<string> ErrorMessages { get; set; }

		public  IList<AgentExtractionResult> ExtractedResults { get;  set; }

		
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
	}

	public interface IImportAgentFileValidator
	{
		AgentDataModel MapRawData(RawAgent raw, out Feedback feedback);
		void SetDefaultValues(ImportAgentDefaults defaultValues);
	}
}