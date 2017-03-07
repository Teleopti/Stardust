using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.People.Core.Models;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class AgentFileTemplateTest
	{
		[Test, Ignore("should check manually")]
		public void ShouldGetFileTemplate()
		{
			var target = new AgentFileTemplate();
			var file = target.GetFileTemplate(target.GetDefaultAgent());
			using (var outputFile = File.Create(@"C:\TeleoptiWFM\SourceCode\main\Teleopti\Logs\template.xls"))
			{
				file.Seek(0, SeekOrigin.Begin);
				file.CopyTo(outputFile);
			}
		}
	}
}