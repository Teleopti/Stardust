using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class FileProcessor : IFileProcessor
	{
		private readonly IImportAgentFileValidator _fileValidator;
		private readonly IAgentPersister _agentPersister;
		private readonly IWorkbookHandler _workbookHandler;

		public FileProcessor(IImportAgentFileValidator fileValidator, IAgentPersister agentPersister, IWorkbookHandler workbookHandler)
		{
			_fileValidator = fileValidator;
			_agentPersister = agentPersister;
			_workbookHandler = workbookHandler;
		}

		public IList<AgentExtractionResult> ProcessSheet(ISheet sheet, ImportAgentFormData defaultValues = null)
		{
			if (defaultValues != null)
			{
				_fileValidator.SetDefaultValues(defaultValues);
			}
			var extractedResult = _workbookHandler.ProcessSheet(sheet);
			_agentPersister.Persist(extractedResult);
			return extractedResult.Where(r => r.Feedback.ErrorMessages.Any() || r.Feedback.WarningMessages.Any()).ToList();
		}

		public IList<string> ValidateWorkbook(IWorkbook workbook)
		{
			return _workbookHandler.ValidateSheetColumnHeader(workbook);
		}

		public IWorkbook ParseFile(FileData fileData)
		{
			if (fileData == null) return null;
			var fileName = fileData.FileName;
			if (!fileName.ToLower().EndsWith("xls") && !fileName.ToLower().EndsWith("xlsx"))
				return null;
			var dataStream = new MemoryStream(fileData.Data);
			return fileName.ToLower().EndsWith("xlsx")
				? (IWorkbook)new XSSFWorkbook(dataStream)
				: new HSSFWorkbook(dataStream);
		}


		public MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx)
		{
			const string invalidUserSheetName = "Agents";
			var ms = new MemoryStream();
			var agentTemplate = new AgentFileTemplate();
			var returnedFile = agentTemplate.GetTemplateWorkbook(invalidUserSheetName, isXlsx);
			var newSheet = returnedFile.GetSheet(invalidUserSheetName);
			var errorMessageColumnIndex = agentTemplate.ColumnHeaderNames.Length;
			var warningMessageColumnIndex = agentTemplate.ColumnHeaderNames.Length + 1;

			newSheet.GetRow(0).CreateCell(errorMessageColumnIndex).SetCellValue("ErrorMessage");
			newSheet.GetRow(0).CreateCell(warningMessageColumnIndex).SetCellValue("WarningMessage");

			for (var i = 0; i < agents.Count; i++)
			{
				var rawAgent = agents[i].Raw;
				var sourceRow = agents[i].Row;
				var newRow = newSheet.CreateRow(i + 1);
				CopyRow(newRow, sourceRow);
				newRow.CreateCell(errorMessageColumnIndex).SetCellValue(string.Join(";", agents[i].Feedback.ErrorMessages));
				newRow.CreateCell(warningMessageColumnIndex).SetCellValue(string.Join(";", agents[i].Feedback.WarningMessages));
			}
			returnedFile.Write(ms);
			return ms;
		}

		private void CopyRow(IRow targeRow, IRow sourceRow)
		{
			var sheet = targeRow.Sheet;
			var workbook = sheet.Workbook;

			for (int i = 0; i < sourceRow.LastCellNum; i++)
			{

				// Grab a copy of the old/new cell
				var oldCell = sourceRow.GetCell(i);
				var newCell = targeRow.CreateCell(i);

				// If the old cell is null jump to next cell
				if (oldCell == null)
				{
					newCell = null;
					continue;
				}

				// Copy style from old cell and apply to new cell
				var newCellStyle = workbook.CreateCellStyle();
				newCellStyle.CloneStyleFrom(oldCell.CellStyle); ;
				newCell.CellStyle = newCellStyle;

				// If there is a cell comment, copy
				if (newCell.CellComment != null) newCell.CellComment = oldCell.CellComment;

				// If there is a cell hyperlink, copy
				if (oldCell.Hyperlink != null) newCell.Hyperlink = oldCell.Hyperlink;

				// Set the cell data type
				newCell.SetCellType(oldCell.CellType);

				// Set the cell data value
				switch (oldCell.CellType)
				{
					case CellType.Blank:
						newCell.SetCellValue(oldCell.StringCellValue);
						break;
					case CellType.Boolean:
						newCell.SetCellValue(oldCell.BooleanCellValue);
						break;
					case CellType.Error:
						newCell.SetCellErrorValue(oldCell.ErrorCellValue);
						break;
					case CellType.Formula:
						newCell.SetCellFormula(oldCell.CellFormula);
						break;
					case CellType.Numeric:
						newCell.SetCellValue(oldCell.NumericCellValue);
						break;
					case CellType.String:
						newCell.SetCellValue(oldCell.RichStringCellValue);
						break;
					case CellType.Unknown:
						newCell.SetCellValue(oldCell.StringCellValue);
						break;
				}
			}

			// If there are are any merged regions in the source row, copy to new row
			for (int i = 0; i < sheet.NumMergedRegions; i++)
			{
				var cellRangeAddress = sheet.GetMergedRegion(i);
				if (cellRangeAddress.FirstRow == sourceRow.RowNum)
				{
					var newCellRangeAddress = new NPOI.SS.Util.CellRangeAddress(targeRow.RowNum,
																				(targeRow.RowNum +
																				 (cellRangeAddress.FirstRow -
																				  cellRangeAddress.LastRow)),
																				cellRangeAddress.FirstColumn,
																				cellRangeAddress.LastColumn);
					sheet.AddMergedRegion(newCellRangeAddress);
				}
			}

		}
	}

	public interface IFileProcessor
	{
		IList<AgentExtractionResult> ProcessSheet(ISheet sheet, ImportAgentFormData defaultValues = null);
		IList<string> ValidateWorkbook(IWorkbook workbook);
		IWorkbook ParseFile(FileData fileData);
		MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx);
	}
}