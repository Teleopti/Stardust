using System.Collections.Generic;
using NPOI.SS.UserModel;
using Teleopti.Ccc.Web.Areas.People.Core.Models;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class AgentExtractionResult
	{
		public AgentDataModel Agent;
		public RawAgent Raw;
		public List<string> ErrorMessages { get; }

		public AgentExtractionResult()
		{
			ErrorMessages = new List<string>();
		}
	}

	public interface IImportAgentFileValidator
	{
		List<string> ExtractColumnNames(IWorkbook workbook);
		List<string> ValidateColumnNames(List<string> columnNames);
		List<AgentExtractionResult> ExtractAgentInfoValues(IWorkbook workbook);
	}
}