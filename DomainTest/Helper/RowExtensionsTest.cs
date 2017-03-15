﻿using NUnit.Framework;
using System;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class RowExtensionsTest
	{
		private XSSFWorkbook _workBook;
		private ISheet _sheet;
		private IDataFormat _dataFormat;

		[SetUp]
		public void Setup()
		{
			_workBook = new XSSFWorkbook();
			_sheet = _workBook.CreateSheet("Test sheet");
			_dataFormat = _workBook.CreateDataFormat();

		}
		[Test]
		public void ShouldCopyAllCellsWhenCopyRow()
		{
			var row = _sheet.CreateRow(0);
			createDateCell(row, 0);

			var nextRow = _sheet.CreateRow(1);
			row.CopyTo(nextRow);

			Assert.AreEqual(nextRow.Cells.Count, row.Cells.Count);
			Assert.AreEqual(nextRow.Cells[0].CellStyle, row.Cells[0].CellStyle);
		}

		[Test]
		public void ShouldCopyCellsInRangeWhenCopyRow()
		{
			var row = _sheet.CreateRow(0);
			createDateCell(row, 0, DateTime.Now);
			createDateCell(row, 1, DateTime.Now);
			createDateCell(row, 2);
			createDateCell(row, 3);

			var nextRow = _sheet.CreateRow(1);
			row.CopyTo(nextRow, 0, 2);

			Assert.AreEqual(nextRow.Cells.Count, 3);
		}

		[Test]
		public void ShouldIsBankRowWhenAllCellsNoValue()
		{
			var row = _sheet.CreateRow(0);
			row.CreateCell(0, CellType.String);
			row.CreateCell(1, CellType.String).SetCellValue("");
			createDateCell(row, 2);

			Assert.IsTrue(row.IsBlank());
		}

		[Test]
		public void ShouldIsNotBlankRowWhenAnyCellsHasValue()
		{
			var row = _sheet.CreateRow(0);
			row.CreateCell(0);
			Console.WriteLine(row.GetCell(0).CellType);
			row.CreateCell(1, CellType.String).SetCellValue("xx");
			createDateCell(row, 2, DateTime.Now);

			Assert.IsFalse(row.IsBlank());
		}

		private void createDateCell(IRow row, int cellIndex, DateTime? value = null)
		{
			var dateCell = row.CreateCell(cellIndex);
			if (value.HasValue)
			{
				dateCell.SetCellValue(value.Value);
			}
			var cellStyle = _workBook.CreateCellStyle();
			cellStyle.DataFormat = _dataFormat.GetFormat("M/d/yyyy");
			dateCell.CellStyle = cellStyle;
		}
	}
}
