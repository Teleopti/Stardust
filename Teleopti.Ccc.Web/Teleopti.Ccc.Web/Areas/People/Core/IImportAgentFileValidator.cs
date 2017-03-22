using NPOI.SS.UserModel;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Web.Areas.People.Core.Models;

namespace Teleopti.Ccc.Web.Areas.People.Core
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