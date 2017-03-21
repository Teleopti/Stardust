using NPOI.SS.UserModel;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class CellExtensions
	{
		public static bool IsBlank(this ICell cell)
		{
			return cell == null || cell.CellType == CellType.Blank ||
				   (cell.CellType == CellType.String && cell.StringCellValue.IsNullOrEmpty());
		}
	}
}