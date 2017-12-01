using System;
using System.Collections.Generic;
using System.IO;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface IExternalPerformanceInfoFileProcessor
	{
		ExternalPerformanceInfoProcessResult Process(ImportFileData importFileData, Action<string> sendProgress);
	}

	public class PerformanceInfoExtractionResult
	{
		public string Row;
		public DetailLevel DetailLevel
		{
			get
			{
				return DetailLevel.Info;
			}
		}
	}

	public class ExternalPerformanceInfoProcessResult
	{
		public ExternalPerformanceInfoProcessResult()
		{
			ValidRecords = new List<PerformanceInfoExtractionResult>();
			InvalidRecords = new List<string>();
		}
		public bool HasError { get; set; }
		public string ErrorMessages { get; set; }
			
		public int TotalRecordCount => InvalidRecords.Count + ValidRecords.Count;
		public IList<string> InvalidRecords { get; set; }

		public List<PerformanceInfoExtractionResult> ValidRecords { get; set; }
	}

	public class ExternalPerformanceInfoFileProcessor : IExternalPerformanceInfoFileProcessor
	{
		public ExternalPerformanceInfoProcessResult Process(ImportFileData importFileData, Action<string> sendProgress)
		{
			var processResult = new ExternalPerformanceInfoProcessResult();

			if (!isFileTypeValid(importFileData))
			{
				processResult.HasError = true;
				processResult.ErrorMessages = Resources.InvalidInput;

				var msg = string.Join(", ", processResult.ErrorMessages);
				sendProgress($"ExternalPerformanceInfoFileProcessor: Extract file has error:{msg}.");
				return processResult;
			}

			var fileProcessResult = extractFileWithoutSeperateSession(importFileData.Data, sendProgress);
			return fileProcessResult;
		}
		
		private ExternalPerformanceInfoProcessResult extractFileWithoutSeperateSession(byte[] rawData, Action<string> sendProgress)
		{
			sendProgress($"ExternalPerformanceInfoFileProcessor: Start to extract file.");
			var processResult = validateFileContent(rawData);

			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.ErrorMessages);
				sendProgress($"ExternalPerformanceInfoFileProcessor: Extract file has error:{msg}.");
				return processResult;
			}

			sendProgress($"ExternalPerformanceInfoFileProcessor: Extract file succeed.");
			return processResult;
		}

		private ExternalPerformanceInfoProcessResult validateFileContent(byte[] content)
		{
			var processResult = new ExternalPerformanceInfoProcessResult();
			var allLines = byteArrayToString(content);

			foreach (var eachLine in allLines)
			{
				var columns = eachLine.Split(',');
				if (columns.Length != 8)
				{
					processResult.HasError = true;
					var errorMessage = string.Format(Resources.InvalidNumberOfFields, 8, columns.Length);
					processResult.InvalidRecords.Add($"{eachLine},{errorMessage}");
				}
			}

			return processResult;
		}

		private bool isFileTypeValid(ImportFileData importFileData)
		{
			var fileName = importFileData.FileName;
			if (!fileName.ToLower().EndsWith("csv"))
				return false;
			
			return true;
		}

		private IList<string> byteArrayToString(byte[] byteArray)
		{
			//var records = new List<string>();
			//using (var ms = new MemoryStream(byteArray))
			//{
			//	using (var sr = new StreamReader(ms))
			//	{

			//		while (true)
			//		{
			//			var record = sr.ReadLine();
						
			//			if (string.IsNullOrEmpty(record))
			//			{
			//				break;
			//			}
			//			records.Add(record);
			//		}
			//	}
			//}
			//return records;
			using (var ms = new MemoryStream(byteArray))
			{
				var tr = new StreamReader(ms);
				return tr.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			}
		}
	}
}
