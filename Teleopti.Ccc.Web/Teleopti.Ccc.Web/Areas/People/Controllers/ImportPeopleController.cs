using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Core;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class ImportPeopleController : ApiController
	{
		private const string newExcelFileContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
		private const string oldExcelFileContentType = "application/vnd.ms-excel";

		private readonly IPeoplePersister _peoplePersister;
		private readonly IFileProcessor _fileProcessor;
		private readonly IMultipartHttpContentExtractor _multipartHttpContentExtractor;

		public ImportPeopleController(IPeoplePersister peoplePersister, IFileProcessor fileProcessor, IMultipartHttpContentExtractor multipartHttpContentExtractor)
		{
			_peoplePersister = peoplePersister;
			_fileProcessor = fileProcessor;
			_multipartHttpContentExtractor = multipartHttpContentExtractor;
		}

		[Route("api/People/UploadPeople"), HttpPost]
		public async Task<HttpResponseMessage> Post()
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
			}

			var provider = new MultipartMemoryStreamProvider();
			await Request.Content.ReadAsMultipartAsync(provider);
			var fileTemplate = new UserFileTemplate();

			var fileData = _multipartHttpContentExtractor.ExtractFileData(provider.Contents);
			var workbook = _fileProcessor.ParseFile(fileData.FirstOrDefault());
			var isXlsx = workbook is XSSFWorkbook;

			var sheet = workbook.GetSheetAt(0);
			var rowEnumerator = sheet.GetRowEnumerator();
			// Read first row and check if missing any column.
			while (rowEnumerator.MoveNext())
			{
				var headerRow = (IRow) rowEnumerator.Current;
				string errorMessage;
				if (anyColumnMissing(headerRow, out errorMessage, fileTemplate))
				{
					var errorMsg = string.Format(Resources.MissingColumnX, errorMessage.Trim(',', ' '));
					var invalidFileResponse = Request.CreateResponse((HttpStatusCode)422);
					invalidFileResponse.Headers.Clear();
					invalidFileResponse.Content = new StringContent(errorMsg);
					invalidFileResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

					return invalidFileResponse;
				}
				break;
			}

			var userList = parseWorkBook(workbook, fileTemplate);
			var invalidUsers = _peoplePersister.Persist(userList);
			var successCount = userList.Count - invalidUsers.Count;
			var failedCount = invalidUsers.Count;

			var response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.Clear();
			response.Headers.Add("Message", string.Format("success count:{0}, failed count:{1}", successCount, failedCount));

			if (invalidUsers.Count == 0)
			{
				return response;
			}

			var ms = constructReturnedFile(isXlsx, invalidUsers, true);
			response.Content = new ByteArrayContent(ms.ToArray());
			response.Content.Headers.ContentType =
				new MediaTypeHeaderValue(isXlsx ? newExcelFileContentType : oldExcelFileContentType);

			return response;
		}

		[Route("api/People/UserTemplate"), HttpPost]
		public HttpResponseMessage GetFileTemplate()
		{
			var template = new UserFileTemplate();
			var ms = template.GetFileTemplateWithDemoData();
			var response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.Clear();
			response.Content = new ByteArrayContent(ms.ToArray());
			response.Content.Headers.ContentType = new MediaTypeHeaderValue(oldExcelFileContentType);

			return response;
		}

		private static MemoryStream constructReturnedFile(bool isXlsx, IList<RawUser> users, bool includeErrorMessage = false)
		{
			const string invalidUserSheetName = "Users";
			var ms = new MemoryStream();
			IRow row;
			var userFileTemplate = new UserFileTemplate();
			var returnedFile = getTemplateFile(isXlsx, includeErrorMessage, invalidUserSheetName, userFileTemplate);
			var newSheet = returnedFile.GetSheet(invalidUserSheetName);

			for (var i = 0; i < users.Count; i++)
			{
				var user = users[i];
				row = newSheet.CreateRow(i + 1);

				row.CreateCell(userFileTemplate.ColumnHeaderMap["Firstname"]).SetCellValue(user.Firstname);
				row.CreateCell(userFileTemplate.ColumnHeaderMap["Lastname"]).SetCellValue(user.Lastname);
				row.CreateCell(userFileTemplate.ColumnHeaderMap["WindowsUser"]).SetCellValue(user.WindowsUser);
				row.CreateCell(userFileTemplate.ColumnHeaderMap["ApplicationUserId"]).SetCellValue(user.ApplicationUserId);
				row.CreateCell(userFileTemplate.ColumnHeaderMap["Password"]).SetCellValue(user.Password);
				row.CreateCell(userFileTemplate.ColumnHeaderMap["Role"]).SetCellValue(user.Role);

				if (includeErrorMessage)
				{
					row.CreateCell(userFileTemplate.ColumnHeaderNames.Length).SetCellValue(user.ErrorMessage);
				}
			}
			returnedFile.Write(ms);
			return ms;
		}
		private static IWorkbook getTemplateFile(bool isXlsx, bool includeErrorMessage, string invalidUserSheetName, UserFileTemplate fileTemplate )
		{
			var returnedFile = isXlsx
				? (IWorkbook) new XSSFWorkbook()
				: new HSSFWorkbook();
			var newsheet = returnedFile.CreateSheet(invalidUserSheetName);

			var row = newsheet.CreateRow(0);
			for (var i = 0; i < fileTemplate.ColumnHeaderNames.Length; i++)
			{
				row.CreateCell(i).SetCellValue(fileTemplate.ColumnHeaderNames[i]);
			}
			if (includeErrorMessage)
			{
				row.CreateCell(fileTemplate.ColumnHeaderNames.Length).SetCellValue("ErrorMessage");
			}
			return returnedFile;
		}
		private static IList<RawUser> parseWorkBook(IWorkbook workbook, UserFileTemplate fileTemplate)
		{
			var userList = new List<RawUser>();
			var sheet = workbook.GetSheetAt(0);
			var rowIndex = 1;
			var rowEnumerator = sheet.GetRowEnumerator();
			while (rowEnumerator.MoveNext())
			{
				if (rowIndex == 1)
				{
					rowIndex++;
					continue;
				}

				var row = (IRow) rowEnumerator.Current;
				var user = new RawUser
				{
					Firstname = getCellValue(row, fileTemplate.ColumnHeaderMap["Firstname"]),
					Lastname = getCellValue(row, fileTemplate.ColumnHeaderMap["Lastname"]),
					WindowsUser = getCellValue(row, fileTemplate.ColumnHeaderMap["WindowsUser"]),
					ApplicationUserId = getCellValue(row, fileTemplate.ColumnHeaderMap["ApplicationUserId"]),
					Password = getCellValue(row, fileTemplate.ColumnHeaderMap["Password"]),
					Role = getCellValue(row, fileTemplate.ColumnHeaderMap["Role"])
				};
				userList.Add(user);
				rowIndex++;
			}
			return userList;
		}

		private static bool anyColumnMissing(IRow headerRow, out string errorMessage, UserFileTemplate fileTemplate)
		{
			//check column header
			var firstNameCol = getCellValue(headerRow, fileTemplate.ColumnHeaderMap["Firstname"]);
			var lastNameCol = getCellValue(headerRow, fileTemplate.ColumnHeaderMap["Lastname"]);
			var windowsUserCol = getCellValue(headerRow, fileTemplate.ColumnHeaderMap["WindowsUser"]);
			var applicationUserCol = getCellValue(headerRow, fileTemplate.ColumnHeaderMap["ApplicationUserId"]);
			var passwordCol = getCellValue(headerRow, fileTemplate.ColumnHeaderMap["Password"]);
			var rolCol = getCellValue(headerRow, fileTemplate.ColumnHeaderMap["Role"]);

			var missingCol = new StringBuilder("");
			var isFirstnameColMissing = isColumnMissing(firstNameCol, "Firstname", missingCol);
			var isLastnameColMissing = isColumnMissing(lastNameCol, "Lastname", missingCol);
			var isWindowsUserColMissing = isColumnMissing(windowsUserCol, "WindowsUser", missingCol);
			var isApplicationUserColMissing = isColumnMissing(applicationUserCol, "ApplicationUserId", missingCol);
			var isPasswordColMissing = isColumnMissing(passwordCol, "Password", missingCol);
			var isRoleColMissing = isColumnMissing(rolCol, "Role", missingCol);
			errorMessage = missingCol.ToString();

			return isFirstnameColMissing || isLastnameColMissing || isWindowsUserColMissing || isApplicationUserColMissing ||
				   isPasswordColMissing || isRoleColMissing;
		}

		private static bool isColumnMissing(string colName, string expectColName, StringBuilder missingColMsg)
		{
			var columnMissing = string.Compare(colName.Trim(), expectColName.Trim(), true, CultureInfo.CurrentCulture) != 0;
			if (columnMissing)
			{
				missingColMsg.AppendFormat("{0}, ", expectColName);
			}
			return columnMissing;
		}

		private static string getCellValue(IRow row, int columnIndex)
		{
			var obj = row.GetCell(columnIndex);
			return obj == null ? string.Empty : obj.ToString();
		}

	}
}
