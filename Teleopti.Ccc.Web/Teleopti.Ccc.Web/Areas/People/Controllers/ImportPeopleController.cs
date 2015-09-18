using System.Collections.Generic;
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
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class ImportPeopleController : ApiController
	{
		private readonly IPeoplePersister _peoplePersister;

		public ImportPeopleController(IPeoplePersister peoplePersister)
		{
			_peoplePersister = peoplePersister;
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

			const int colIndexFirstname = 0;
			const int colIndexLastname = 1;
			const int colIndexWindowsUser = 2;
			const int colIndexApplicationUserId = 3;
			const int colIndexPassword = 4;
			const int colIndexRole = 5;
			var userList = new List<RawUser>();
			var ms = new MemoryStream();
			var isXlsx = false;

			foreach (var file in provider.Contents)
			{
				var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
				var dataStream = await file.ReadAsStreamAsync();
				isXlsx = fileName.EndsWith("xlsx");
				var workbook = isXlsx
					? (IWorkbook)new XSSFWorkbook(dataStream)
					: new HSSFWorkbook(dataStream);
				for (var sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
				{
					var sheet = workbook.GetSheetAt(sheetIndex);

					var rowIndex = 1;
					var rowEnumerator = sheet.GetRowEnumerator();
					while (rowEnumerator.MoveNext())
					{
						var row = (IRow)rowEnumerator.Current;
						if (rowIndex == 1)
						{
							//check column header
							var firstNameCol = getCellValue(row, colIndexFirstname);
							var lastNameCol = getCellValue(row, colIndexLastname);
							var windowsUserCol = getCellValue(row, colIndexWindowsUser);
							var applicationUserCol = getCellValue(row, colIndexApplicationUserId);
							var passwordCol = getCellValue(row, colIndexPassword);
							var rolCol = getCellValue(row, colIndexRole);
							var missingCol = new StringBuilder("");
							var isFirstnameColMissing = isColumnExist(firstNameCol, "Firstname", missingCol);
							var isLastnameColMissing = isColumnExist(lastNameCol, "Lastname", missingCol);
							var isWindowsUserColMissing = isColumnExist(windowsUserCol, "WindowsUser", missingCol);
							var isApplicationUserColMissing = isColumnExist(applicationUserCol, "ApplicationUserId", missingCol);
							var isPasswordColMissing = isColumnExist(passwordCol, "Password", missingCol);
							var isRoleColMissing = isColumnExist(rolCol, "Role", missingCol);
							var isMissingCol = isFirstnameColMissing || isLastnameColMissing || isWindowsUserColMissing ||
											   isApplicationUserColMissing || isPasswordColMissing || isRoleColMissing;

							if (isMissingCol)
							{
								var errorMsg =string.Format(UserTexts.Resources.MissingColumnX,  missingCol.ToString().Substring(0, missingCol.ToString().Length - 2));
								var response = Request.CreateResponse(HttpStatusCode.InternalServerError);
								response.Headers.Clear();
								response.Content = new StringContent(errorMsg);
								response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

								return response;
							}
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
				}

			}
			var invalidUsers = InternalPersist(userList).ToList();
			
			if (invalidUsers.Count > 0)
			{
				var returnedFile = isXlsx
					? (IWorkbook)new XSSFWorkbook()
					: new HSSFWorkbook();
				returnedFile.CreateSheet("invalidUsers");
				var sheet = returnedFile.GetSheet("invalidUsers");

				for (var i = 0; i < invalidUsers.Count + 1; i++)
				{
					var row = sheet.CreateRow(i);
					if (i == 0)
					{
						row.CreateCell(0).SetCellValue("Firstname");
						row.CreateCell(1).SetCellValue("Lastname");
						row.CreateCell(2).SetCellValue("WindowsUser");
						row.CreateCell(3).SetCellValue("ApplicationUserId");
						row.CreateCell(4).SetCellValue("Password");
						row.CreateCell(5).SetCellValue("Role");
						row.CreateCell(6).SetCellValue("ErrorMessage");
					}
					else
					{
						row.CreateCell(0).SetCellValue(invalidUsers[i - 1].Firstname);
						row.CreateCell(1).SetCellValue(invalidUsers[i - 1].Lastname);
						row.CreateCell(2).SetCellValue(invalidUsers[i - 1].WindowsUser);
						row.CreateCell(3).SetCellValue(invalidUsers[i - 1].ApplicationUserId);
						row.CreateCell(4).SetCellValue(invalidUsers[i - 1].Password);
						row.CreateCell(5).SetCellValue(invalidUsers[i - 1].Role);
						row.CreateCell(6).SetCellValue(invalidUsers[i - 1].ErrorMessage);
					}
				}
				returnedFile.Write(ms);
				var response = Request.CreateResponse(HttpStatusCode.OK);
				response.Headers.Clear();
				response.Content = new ByteArrayContent(ms.ToArray());
				var contentType = isXlsx ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" :
				"application/vnd.ms-excel";
				response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

				return response;
			}

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		private static bool isColumnExist(string colHeader, string colName, StringBuilder missingColMsg)
		{
			if (colHeader != colName)
			{
				var colText = colName + ", ";
				missingColMsg.Append(colText);
				return true;
			}
			return false;
		}

		[TenantUnitOfWork]
		[UnitOfWork]
		protected virtual IEnumerable<RawUser> InternalPersist(IEnumerable<RawUser> users)
		{
			return _peoplePersister.Persist(users);
		} 
		private string getCellValue(IRow row, int columnIndex)
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