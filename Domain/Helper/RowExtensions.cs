using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class RowExtensions
	{
		public static void CopyTo(this IRow sourceRow, IRow targetRow, int startIndex = 0, int endIndex = 0)
		{
			if (!validateCopyRowParameters(sourceRow, targetRow, startIndex, endIndex))
			{
				return;
			}

			var sheet = targetRow.Sheet;
			for (int i = startIndex; i < endIndex + 1; i++)
			{
				var oldCell = sourceRow.GetCell(i);
				var newCell = targetRow.CreateCell(i);
				if (oldCell == null)
				{
					continue;
				}

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

		public static bool IsBlank(this IRow row)
		{
			return row == null || row.Cells.Sum(c => c.IsBlank() ? 1 : 0) == row.Cells.Count;
		}

		public static IEnumerable<ICell> GetCellsIncludeBlankOrNull(this IRow row)
		{
			if (row == null)
			{
				yield break;
			}
			for (int i = 0; i < row.LastCellNum; i++)
			{
				yield return row.GetCell(i, MissingCellPolicy.RETURN_NULL_AND_BLANK);
			}
		}

		private static bool validateCopyRowParameters(IRow sourceRow, IRow targetRow, int startIndex, int endIndex)
		{
			if (sourceRow == null)
			{
				throw new ArgumentNullException("sourceRow");
			}
			if (targetRow == null)
			{
				throw new ArgumentNullException("targetRow");
			}
			var maxIndex = sourceRow.LastCellNum;
			if (startIndex < 0 || endIndex > maxIndex)
			{
				throw new ArgumentException(string.Format(Resources.OutOfIndexRange, startIndex, endIndex));
			}
			return true;
		}
	}
}