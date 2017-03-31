using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.People.Core;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class MultipartHttpContentExtractorTest
	{
		private MultipartHttpContentExtractor target;

		private class testFormModel
		{
			public string TestProp1 { get; set; }
			public string TestProp2 { get; set; }
		}

		[SetUp]
		public void SetUp()
		{
			target = new MultipartHttpContentExtractor();
		}

		[Test]
		public void ShouldExtractFormModel()
		{
			var multipart = new MultipartFormDataContent();

			var prop1Content = new StringContent("value1");
			prop1Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
			{
				Name = "TestProp1"
			};

			var prop2Content = new StringContent("value2");
			prop2Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
			{
				Name = "TestProp2"
			};

			multipart.Add(prop1Content);
			multipart.Add(prop2Content);

			var result = target.ExtractFormModel<testFormModel>(multipart);

			result.TestProp1.Should().Be.EqualTo("value1");
			result.TestProp2.Should().Be.EqualTo("value2");
		}

		[Test]
		public void ShouldExtractFile()
		{
			var multipart = new MultipartFormDataContent();

			var fileContent = new ByteArrayContent(Encoding.ASCII.GetBytes("Text"));
			fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("file")
			{
				Name = "file",
				FileName = "textfile"
			};
			multipart.Add(fileContent);
			var result = target.ExtractFileData(multipart).Single();

			result.FileName.Should().Be.EqualTo("textfile");
			result.Data.Should().Have.SameSequenceAs(Encoding.ASCII.GetBytes("Text"));
		}
	}
}

