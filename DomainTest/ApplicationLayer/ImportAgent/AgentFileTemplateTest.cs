using System;
using System.IO;
using NPOI.HSSF.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using System.Collections.Generic;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportAgent
{
	[TestFixture]
	public class AgentFileTemplateTest
	{
		[Test]
		[Ignore("should check manually")]
		public void ShouldGetFileTemplate()
		{
			var target = new AgentFileTemplate();
			var file = target.GetFileTemplate(target.GetDefaultAgent());
			using (var outputFile = File.Create(@"C:\TeleoptiWFM\SourceCode\main\Teleopti\Logs\agent_file_template.xls"))
			{
				file.Seek(0, SeekOrigin.Begin);
				file.CopyTo(outputFile);
			}
		}

		[Test]
		public void EachColumnInFileTemplateShouldHaveDesiredType()
		{
			var target = new AgentFileTemplate();
			var defaultAgent = target.GetDefaultAgent();
			var file = target.GetFileTemplate(defaultAgent);

			var workbook = new HSSFWorkbook(file);

			var cells = workbook.GetSheetAt(0).GetRow(1).Cells;

			defaultAgent.Firstname.Should().Be.EqualTo(cells[0].StringCellValue);
			defaultAgent.Lastname.Should().Be.EqualTo(cells[1].StringCellValue);
			defaultAgent.WindowsUser.Should().Be.EqualTo(cells[2].StringCellValue);
			defaultAgent.ApplicationUserId.Should().Be.EqualTo(cells[3].StringCellValue);
			defaultAgent.Password.Should().Be.EqualTo(cells[4].StringCellValue);
			defaultAgent.Role.Should().Be.EqualTo(cells[5].StringCellValue);
			defaultAgent.StartDate.Should().Be.EqualTo(cells[6].DateCellValue); // Date
			defaultAgent.Organization.Should().Be.EqualTo(cells[7].StringCellValue);
			defaultAgent.Skill.Should().Be.EqualTo(cells[8].StringCellValue);
			defaultAgent.ExternalLogon.Should().Be.EqualTo(cells[9].StringCellValue);
			defaultAgent.Contract.Should().Be.EqualTo(cells[10].StringCellValue);
			defaultAgent.ContractSchedule.Should().Be.EqualTo(cells[11].StringCellValue);
			defaultAgent.PartTimePercentage.Should().Be.EqualTo(cells[12].StringCellValue);
			defaultAgent.ShiftBag.Should().Be.EqualTo(cells[13].StringCellValue);
			defaultAgent.SchedulePeriodType.Should().Be.EqualTo(cells[14].StringCellValue);
			defaultAgent.SchedulePeriodLength.Should().Be.EqualTo(cells[15].NumericCellValue); // Number
		}

		[Test]
		[Ignore("get test file for develop test")]
		public void GenerateBigAgentFile()
		{
			var agents = new List<RawAgent>();
			var target = new AgentFileTemplate();

			for (int i = 0; i < 1000; i++)
			{
				var defaultAgent = target.GetDefaultAgent();
				defaultAgent.Firstname += Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);
				var identityUser = $"{defaultAgent.Firstname.ToLower()}.{defaultAgent.Lastname.ToLower()}@teleopti.com";
				defaultAgent.ApplicationUserId = identityUser;
				defaultAgent.WindowsUser = identityUser;
				agents.Add(defaultAgent);
			}
			var fileStream = new AgentFileTemplate().GetFileTemplate(agents.ToArray()).ToArray();
			File.WriteAllBytes(@"C:\big_agents.xls", fileStream);
		}

	}
}