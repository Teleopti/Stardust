using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Interfaces.Domain;
using System.Data;
using DataSet = System.Data.DataSet;
namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class ImportPeopleController : ApiController
	{
		private readonly IPeoplePersister _peoplePersister;

		public ImportPeopleController(IPeoplePersister peoplePersister)
		{
			_peoplePersister = peoplePersister;
		}

		[System.Web.Http.Route("api/People/UploadPeople"), System.Web.Http.HttpPost]
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
			MemoryStream ms = new MemoryStream();

			foreach (var file in provider.Contents)
			{
				var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
				var dataStream = await file.ReadAsStreamAsync();

				var workbook = (fileName.EndsWith("xlsx"))
					? (IWorkbook)new XSSFWorkbook(dataStream)
					: new HSSFWorkbook(dataStream);
				for (var sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
				{
					var sheet = workbook.GetSheetAt(sheetIndex);
					Console.WriteLine("Name of sheet [{0}]: {1}", sheetIndex, sheet.SheetName);

					var rowIndex = 1;
					var rowEnumerator = sheet.GetRowEnumerator();
					while (rowEnumerator.MoveNext())
					{
						if (rowIndex == 1)
						{
							Console.WriteLine("First row will been skipped as column header");
							rowIndex++;
							continue;
						}

						var row = (IRow)rowEnumerator.Current;
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
			var result =new
			{
				InvalidUsers = invalidUsers,
				InvalidCount = invalidUsers.Count
			};
			var returnedFile = new XSSFWorkbook();
			if (result.InvalidCount > 0)
			{
				returnedFile.CreateSheet("invalidUsers");
				var sheet = returnedFile.GetSheet("invalidUsers");

				for (var i = 0; i < result.InvalidCount + 1; i++)
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
						row.CreateCell(0).SetCellValue(result.InvalidUsers[i-1].Firstname);
						row.CreateCell(1).SetCellValue(result.InvalidUsers[i-1].Lastname);
						row.CreateCell(2).SetCellValue(result.InvalidUsers[i-1].WindowsUser);
						row.CreateCell(3).SetCellValue(result.InvalidUsers[i-1].ApplicationUserId);
						row.CreateCell(4).SetCellValue(result.InvalidUsers[i-1].Password);
						row.CreateCell(5).SetCellValue(result.InvalidUsers[i-1].Role);
						row.CreateCell(6).SetCellValue(result.InvalidUsers[i-1].ErrorMessage);
					}
				}
				returnedFile.Write(ms);
				var response = Request.CreateResponse(HttpStatusCode.OK);
				response.Headers.Clear();
				response.Content = new ByteArrayContent(ms.ToArray());
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

				return response;
			}

			return Request.CreateResponse(HttpStatusCode.OK);
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

	public class InputFile
	{
		public HttpPostedFileBase File { get; set; }
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