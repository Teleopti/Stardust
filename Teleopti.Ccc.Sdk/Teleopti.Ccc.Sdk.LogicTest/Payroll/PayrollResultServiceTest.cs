using System;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.PayrollFormatter;
using Teleopti.Ccc.Sdk.Logic.Payroll;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.Payroll
{
	[TestFixture]
	public class PayrollResultServiceTest
	{
		[Test]
		public void ShouldCreateFile()
		{
			var personOwner = PersonFactory.CreatePerson();
			var resultRep = new FakePayrollResultRepository();
			var target = new PayrollResultService(resultRep);
			var resultGuid = new Guid("AAB2796D-C440-4A85-BD96-719D8FCBA8ED");
			var payrollExport = new PayrollExport();
			payrollExport.Name = "The Ex\\port";
			payrollExport.PayrollFormatId = Guid.NewGuid();
			payrollExport.PayrollFormatName = "test";
			payrollExport.Period = new DateOnlyPeriod();
			var timestamp = DateTime.UtcNow;

			var result = new PayrollResult(payrollExport, personOwner, timestamp).WithId(resultGuid);
			result.PayrollExport = payrollExport;
			resultRep.Add(result);

			var document = new XmlDocument();
			document.AppendChild(document.CreateElement("TheResultStuffInHere"));
			result.XmlResult.SetResult(document);

			var format = DocumentFormat.LoadFromXml(new XmlDocument());
			var bytes = target.CreatePayrollResultFileNameById(resultGuid);
			var s = format.Encoding.GetString(bytes);
			s.Should().Be.EqualTo("<TheResultStuffInHere />");
		}
	}
}
