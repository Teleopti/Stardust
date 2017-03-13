using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public static class RowExtensions
	{
		public static void CopyTo(this IRow sourceRow, IRow targetRow)
		{
			if (sourceRow == null || targetRow == null)
			{
				return;
			}
			var sheet = targetRow.Sheet;
			var workbook = sheet.Workbook;

			for (int i = 0; i < sourceRow.LastCellNum; i++)
			{
				var oldCell = sourceRow.GetCell(i);
				var newCell = targetRow.CreateCell(i);
				if (oldCell == null)
				{
					newCell = null;
					continue;
				}
				var newCellStyle = workbook.CreateCellStyle();
				newCellStyle.CloneStyleFrom(oldCell.CellStyle); ;
				newCell.CellStyle = newCellStyle;
				if (newCell.CellComment != null) newCell.CellComment = oldCell.CellComment;
				if (oldCell.Hyperlink != null) newCell.Hyperlink = oldCell.Hyperlink;
				newCell.SetCellType(oldCell.CellType);
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
			for (int i = 0; i < sheet.NumMergedRegions; i++)
			{
				var cellRangeAddress = sheet.GetMergedRegion(i);
				if (cellRangeAddress.FirstRow == sourceRow.RowNum)
				{
					var newCellRangeAddress = new NPOI.SS.Util.CellRangeAddress(targetRow.RowNum,
																				(targetRow.RowNum +
																				 (cellRangeAddress.FirstRow -
																				  cellRangeAddress.LastRow)),
																				cellRangeAddress.FirstColumn,
																				cellRangeAddress.LastColumn);
					sheet.AddMergedRegion(newCellRangeAddress);
				}
			}

		}
	}
}