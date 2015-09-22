using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class ImportPeopleController : ApiController
	{
		private const int colIndexFirstname = 0;
		private const int colIndexLastname = 1;
		private const int colIndexWindowsUser = 2;
		private const int colIndexApplicationUserId = 3;
		private const int colIndexPassword = 4;
		private const int colIndexRole = 5;
		private const int colIndexErrorMessage = 6;

		private readonly IPeoplePersister _peoplePersister;

		public ImportPeopleController(IPeoplePersister peoplePersister)
		{
			_peoplePersister = peoplePersister;
		}

		[Route("api/People/UploadPeople"), HttpPost]
		public HttpResponseMessage Post()
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
			}

			var provider = new MultipartMemoryStreamProvider();
			var result = Request.Content.ReadAsMultipartAsync(provider).Result;

			
			var workbook = parseFiles(provider.Contents.First());
			var isXlsx = workbook.GetType() == typeof(XSSFWorkbook);

			var sheet = workbook.GetSheetAt(0);
			var rowEnumerator = sheet.GetRowEnumerator();
			while (rowEnumerator.MoveNext())
			{
				var row = (IRow)rowEnumerator.Current;
				string errorMessage;
				if (validateColumnHeaders(row, out errorMessage))
				{
					var errorMsg = string.Format(UserTexts.Resources.MissingColumnX, errorMessage.Trim(new[] {',', ' '}));
					var invalidFileResponse = Request.CreateResponse(HttpStatusCode.InternalServerError);
					invalidFileResponse.Headers.Clear();
					invalidFileResponse.Content = new StringContent(errorMsg);
					invalidFileResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

					return invalidFileResponse;
				}
				break;
			}
			var userList = parseWorkBook(workbook);

			var invalidUsers = _peoplePersister.Persist(userList);
			var response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.Clear();
			var successCount = userList.Count - invalidUsers.Count;
			var failedCount = invalidUsers.Count;
			response.Headers.Add("Message", new[] { successCount.ToString(), failedCount.ToString() });

			if (invalidUsers.Count == 0) 
				return Request.CreateResponse(HttpStatusCode.OK);

			var ms = constructReturnedFile(isXlsx, invalidUsers);
			response.Content = new ByteArrayContent(ms.ToArray());
			var contentType = isXlsx ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
				"application/vnd.ms-excel";
			response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

			return response;
		}

		private static MemoryStream constructReturnedFile(bool isXlsx, IList<RawUser> invalidUsers)
		{
			const string invalidUserSheetName = "invalidUsers";
			var ms = new MemoryStream();
			var returnedFile = isXlsx
				? (IWorkbook) new XSSFWorkbook()
				: new HSSFWorkbook();
			returnedFile.CreateSheet(invalidUserSheetName);
			var newsheet = returnedFile.GetSheet(invalidUserSheetName);

			var row = newsheet.CreateRow(0);
			row.CreateCell(colIndexFirstname).SetCellValue("Firstname");
			row.CreateCell(colIndexLastname).SetCellValue("Lastname");
			row.CreateCell(colIndexWindowsUser).SetCellValue("WindowsUser");
			row.CreateCell(colIndexApplicationUserId).SetCellValue("ApplicationUserId");
			row.CreateCell(colIndexPassword).SetCellValue("Password");
			row.CreateCell(colIndexRole).SetCellValue("Role");
			row.CreateCell(colIndexErrorMessage).SetCellValue("ErrorMessage");

			for (var i = 0; i < invalidUsers.Count; i++)
			{
				var user = invalidUsers[i];
				row = newsheet.CreateRow(i + 1);
				row.CreateCell(colIndexFirstname).SetCellValue(user.Firstname);
				row.CreateCell(colIndexLastname).SetCellValue(user.Lastname);
				row.CreateCell(colIndexWindowsUser).SetCellValue(user.WindowsUser);
				row.CreateCell(colIndexApplicationUserId).SetCellValue(user.ApplicationUserId);
				row.CreateCell(colIndexPassword).SetCellValue(user.Password);
				row.CreateCell(colIndexRole).SetCellValue(user.Role);
				row.CreateCell(colIndexErrorMessage).SetCellValue(user.ErrorMessage);
			}
			returnedFile.Write(ms);
			return ms;
		}

		private static IWorkbook parseFiles(HttpContent file)
		{
			var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
			var dataStream = file.ReadAsStreamAsync().Result;
			var isXlsx = fileName.EndsWith("xlsx");
			return isXlsx
				? (IWorkbook) new XSSFWorkbook(dataStream)
				: new HSSFWorkbook(dataStream);
		}

		private static IList<RawUser> parseWorkBook(IWorkbook workbook)
		{
			var userList = new List<RawUser>();
			var sheet = workbook.GetSheetAt(0);
			var rowIndex = 1;
			var rowEnumerator = sheet.GetRowEnumerator();
			while (rowEnumerator.MoveNext())
			{
				var row = (IRow) rowEnumerator.Current;
				if (rowIndex == 1)
				{
					rowIndex++;
					continue;
				}
				var user = new RawUser
				{
					Firstname = getCellValue(row, colIndexFirstname),
					Lastname = getCellValue(row, colIndexLastname),
					WindowsUser = getCellValue(row, colIndexWindowsUser),
					ApplicationUserId = getCellValue(row, colIndexApplicationUserId),
					Password = getCellValue(row, colIndexPassword),
					Role = getCellValue(row, colIndexRole)
				};
				userList.Add(user);
				rowIndex++;
			}
			return userList;
		}

		private static bool validateColumnHeaders(IRow headerRow, out string errorMessage)
		{
			//check column header
			var firstNameCol = getCellValue(headerRow, colIndexFirstname);
			var lastNameCol = getCellValue(headerRow, colIndexLastname);
			var windowsUserCol = getCellValue(headerRow, colIndexWindowsUser);
			var applicationUserCol = getCellValue(headerRow, colIndexApplicationUserId);
			var passwordCol = getCellValue(headerRow, colIndexPassword);
			var rolCol = getCellValue(headerRow, colIndexRole);

			var missingCol = new StringBuilder("");
			var isFirstnameColMissing = isColumnExist(firstNameCol, "Firstname", missingCol);
			var isLastnameColMissing = isColumnExist(lastNameCol, "Lastname", missingCol);
			var isWindowsUserColMissing = isColumnExist(windowsUserCol, "WindowsUser", missingCol);
			var isApplicationUserColMissing = isColumnExist(applicationUserCol, "ApplicationUserId", missingCol);
			var isPasswordColMissing = isColumnExist(passwordCol, "Password", missingCol);
			var isRoleColMissing = isColumnExist(rolCol, "Role", missingCol);
			errorMessage = missingCol.ToString();

			return isFirstnameColMissing || isLastnameColMissing || isWindowsUserColMissing ||
				   isApplicationUserColMissing || isPasswordColMissing || isRoleColMissing;
		}

		private static bool isColumnExist(string colHeader, string colName, StringBuilder missingColMsg)
		{
			if (colHeader == colName) return false;
			var colText = colName + ", ";
			missingColMsg.Append(colText);
			return true;
		}

		private static string getCellValue(IRow row, int columnIndex)
		{
			var obj = row.GetCell(columnIndex);
			return obj == null ? string.Empty : obj.ToString();
		}

	}

	public class RawUserData
	{
		public IList<RawUser> Users { get; set; }
	}

	public class RawUser
	{
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public string WindowsUser { get; set; }
		public string ApplicationUserId { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
		public string ErrorMessage { get; set; }
	}
}