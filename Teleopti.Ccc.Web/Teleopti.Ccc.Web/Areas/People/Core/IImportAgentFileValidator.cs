using System.Collections.Generic;
using NPOI.SS.UserModel;
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
		public RawAgent Raw { get; set; }
		public Feedback Feedback { get; }

		public AgentExtractionResult()
		{
			Feedback = new Feedback();
		}
	}

	public interface IImportAgentFileValidator
	{
		List<string> ExtractColumnNames(IWorkbook workbook);
		List<string> ValidateColumnNames(List<string> columnNames);
		List<AgentExtractionResult> ExtractAgentInfoValues(IWorkbook workbook);
		void SetDefaultValues(ImportAgentFormData defaultValues);
	}
}