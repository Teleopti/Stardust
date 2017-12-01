using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportExternalPerformance
{
	[TestFixture, DomainTest]
	public class ImportExternalPerformanceInfoProcessorTest : ISetup
	{
		public IExternalPerformanceInfoFileProcessor Target;
		public IStardustJobFeedback Feedback;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ExternalPerformanceInfoFileProcessor>().For<IExternalPerformanceInfoFileProcessor>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
		}

		[Test]
		public void ShouldNotAllowOtherFileTypeThanCSV()
		{
			var fileData = new ImportFileData(){FileName = "test.xls"};
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.HasError.Should().Be.True();
			result.ErrorMessages.Should().Be.Equals(Resources.InvalidInput);
		}

		[Ignore("need to use memory instead of hard disk file"), Test]
		public void ShouldOnlyHave8FieldsForEachLine()
		{
			var validLine = "2017-11-20,1,Kalle,Pettersson,Quality Score,1,Procent,87";
			var invalidLine = "2017-11-20,1,Kalle,Pettersson,Sales result,2,Number,2000,extraline";
			var lines = new List<string> {validLine, invalidLine};
			var data = stringToArray(lines);


			var fileData = new ImportFileData() { FileName = "test.csv", Data = data };
			var expectedErrorMsg = invalidLine + "," + string.Format(Resources.InvalidNumberOfFields, 8, 9);
			var result = Target.Process(fileData, Feedback.SendProgress);

			result.HasError.Should().Be.True();
			result.InvalidRecords.Count.Should().Be.EqualTo(1);
			result.InvalidRecords[0].Should().Be.EqualTo(expectedErrorMsg);
		}

		private byte[] stringToArray(IList<string> lines)
		{
			byte[] data;

			//using (MemoryStream ms = new MemoryStream())
			//{
			//	using (StreamWriter sw = new StreamWriter(ms))
			//	{
			//		foreach (var line in lines)
			//		{
			//			sw.WriteLine(line);
						
			//		}
			//		sw.Close();
			//	}

			//	data = ms.GetBuffer();

			//}

			var path = Path.Combine(Environment.CurrentDirectory, "test.csv");
			using (var sw = File.CreateText(path))
			{

				foreach (var line in lines)
				{
					sw.WriteLine(line);

				}
				sw.Flush();
			}

			using (FileStream fs = File.OpenRead(path))
			{
				var r = new BinaryReader(fs);
				r.BaseStream.Seek(0, SeekOrigin.Begin);
				data = r.ReadBytes((int)r.BaseStream.Length);
			}

			return data;
		}
	}
}
