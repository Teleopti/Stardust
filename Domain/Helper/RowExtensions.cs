using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
			var workbook = sheet.Workbook;
			for (int i = startIndex; i < endIndex + 1; i++)
			{
				var oldCell = sourceRow.GetCell(i);
				var newCell = targetRow.CreateCell(i);
				if (oldCell == null)
				{
					continue;
				}

				ICellStyle cellStyle = GetExistsCellStyle(targetRow, oldCell.CellStyle);
				if (cellStyle == null)
				{
					cellStyle = workbook.CreateCellStyle();
					cellStyle.CloneStyleFrom(oldCell.CellStyle);
				}
				newCell.CellStyle = cellStyle;

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

		private static ICellStyle GetExistsCellStyle(IRow row, ICellStyle style)
		{
			for (short i = 0; i < row.Sheet.Workbook.NumCellStyles; i++)
			{
				var currentStyle = row.Sheet.Workbook.GetCellStyleAt(i);
				if (Equals(currentStyle, style, new[] { nameof(currentStyle.Index), nameof(currentStyle.FontIndex) }))
				{
					return currentStyle;
				}
			}
			return null;
		}

		private static bool Equals<T>(T entity1, T entity2, params string[] exceptedProperties)
		{
			var properties = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
				.Where(p => !exceptedProperties.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
			foreach (var pro in properties)
			{
				var value = pro.GetValue(entity1);
				var value2 = pro.GetValue(entity2);
				if (value == null)
				{
					continue;
				}
				if (!value.GetType().IsValueType)
				{
					Equals(value, value2);
				}
				if (!pro.GetValue(entity1).Equals(pro.GetValue(entity2)))
				{
					return false;
				}
			}
			return true;
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