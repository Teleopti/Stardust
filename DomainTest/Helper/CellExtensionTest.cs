using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class CellExtensionTest
	{
		private ICell target;

		[Test]
		public void ShouldReturnTrueIfCellIsNull()
		{
			target.IsBlank().Should().Be.True();
		}

		[Test]
		public void ShouldReturnTrueIfCellTypeIsBlank()
		{
			var workbook = new HSSFWorkbook();
			var sheet = workbook.CreateSheet("test");
			target = new HSSFCell(workbook, (HSSFSheet)sheet, 0, 0, CellType.Blank);
			target.IsBlank().Should().Be.True();
		}

		[Test]
		public void ShouldReturnTrueIfCellTypeIsStringWithEmptyValue()
		{
			var workbook = new HSSFWorkbook();
			var sheet = workbook.CreateSheet("test");
			target = new HSSFCell(workbook, (HSSFSheet)sheet, 0, 0, CellType.String);
			target.SetCellValue("");
			target.IsBlank().Should().Be.True();
		}

	}
}