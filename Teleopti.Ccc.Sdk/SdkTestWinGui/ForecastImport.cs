using System;
using System.Data.SqlClient;
using System.IO;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;

namespace SdkTestWinGui
{
	public class ForecastImport
	{
		private readonly string _connectionString;
		private readonly ServiceApplication _service;


		public ForecastImport( ServiceApplication service)
		{
			_connectionString = "Data Source=.;Initial Catalog=teleopticcc_DemoSales_TeleoptiCCC7;Integrated Security=True;Current Language=us_english";
			_service = service;
		}

		public  void ImportFile()
		{
			var file = @"C:\Users\ola\Documents\Import-Forecast-Test-File.csv";
			var businessUnitId = "928DD0BC-BF40-412E-B970-9B5E015AADEA";
			var personId = "10957AD5-5489-48E0-959A-9B5E015B2B5C";
			var fileInfo = new FileInfo(file);
			var uploadedFileId = Guid.NewGuid();
			using (
					var connection =
						 new SqlConnection(_connectionString)
					)
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					using (var command = connection.CreateCommand())
					{
						command.Transaction = transaction;
						command.Parameters.AddWithValue("@BusinessUnitId", businessUnitId);
						command.Parameters.AddWithValue("@ImportUser", personId);
						command.Parameters.AddWithValue("@FileId", uploadedFileId);
						command.Parameters.AddWithValue("@FileName", fileInfo.Name);
						command.Parameters.AddWithValue("@FileContent", File.ReadAllBytes(fileInfo.FullName));
						command.CommandText =
							 "INSERT INTO dbo.ForecastFile (Id,UpdatedBy,UpdatedOn,FileName,FileContent,BusinessUnit) VALUES (@FileId,@ImportUser,GETUTCDATE(),@FileName,@FileContent,@BusinessUnitId)";
						command.ExecuteNonQuery();
						transaction.Commit();
					}
				}
			}
			ImportForecast("F08D75B3-FDB4-484A-AE4C-9F0800E2F753", uploadedFileId);
		}
		 public void ImportForecast(string skillId, Guid fileId)
		{
			
			var result = _service.InternalService.ExecuteCommand(new ImportForecastsFileCommandDto
			{
				ImportForecastsMode = ImportForecastsOptionsDto.ImportWorkload,
				ImportForecastsModeSpecified = true,
				TargetSkillId = skillId,
				UploadedFileId = fileId.ToString()
			});

		}
	}
}