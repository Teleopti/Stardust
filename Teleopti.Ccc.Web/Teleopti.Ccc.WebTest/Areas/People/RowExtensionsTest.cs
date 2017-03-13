using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Web.Areas.People.Core;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class RowExtensionsTest
	{
		[Test]
		public void ShouldCopyAllCellsWhenCopyRow()
		{
			var workbook = new XSSFWorkbook();
			var sheet = workbook.CreateSheet("Test sheet");
			var row = sheet.CreateRow(0);
			var dateCell = row.CreateCell(0);
			dateCell.SetCellValue(DateTime.Now);
			var cellStyle = workbook.CreateCellStyle();
			cellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("M/d/yyyy");
			dateCell.CellStyle = cellStyle;

			var nextRow = sheet.CreateRow(1);
			row.CopyTo(nextRow);


			Assert.AreEqual(nextRow.Cells.Count, row.Cells.Count);
			Assert.AreEqual(nextRow.Cells[0].CellStyle, row.Cells[0].CellStyle);
		}


	}
}
