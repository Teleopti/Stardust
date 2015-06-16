using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.PayrollFormatter;
using Teleopti.Ccc.Sdk.Logic.Payroll;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.Payroll
{
	[TestFixture]
	public class PayrollResultServiceTest
	{
		private PayrollResultService target;
		private MockRepository mocks;
		private IPayrollResultRepository resultRep;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private Guid resultGuid;
		private PayrollResult result;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			var personOwner = PersonFactory.CreatePerson();
			resultRep = mocks.StrictMock<IPayrollResultRepository>();
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			target = new PayrollResultService(currentUnitOfWorkFactory, resultRep);
			resultGuid = new Guid("AAB2796D-C440-4A85-BD96-719D8FCBA8ED");
			var payrollExport = new PayrollExport();
			payrollExport.Name = "The Ex\\port";
			payrollExport.PayrollFormatId = Guid.NewGuid();
			payrollExport.PayrollFormatName = "test";
			payrollExport.Period = new DateOnlyPeriod();
			var timestamp = DateTime.UtcNow;
			result = new PayrollResult(payrollExport, personOwner, timestamp);
			result.PayrollExport = payrollExport; //ehh strange...
			result.SetId(resultGuid);
			var document = new XmlDocument();
			document.AppendChild(document.CreateElement("TheResultStuffInHere"));
			result.XmlResult.AddResult(document);
		}

		[Test]
		public void ShouldCreateFile()
		{
			var unitOfWork = mocks.StrictMock<IUnitOfWork>();
			using(mocks.Record())
			{
				Expect.Call(resultRep.Load(resultGuid)).Return(result);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
				Expect.Call(unitOfWork.Dispose);
			}

			using (mocks.Playback())
			{
                var format = DocumentFormat.LoadFromXml(new XmlDocument());
				var bytes = target.CreatePayrollResultFileNameById(resultGuid);
                var s = format.Encoding.GetString(bytes);
			    s.Should().Be.EqualTo("<TheResultStuffInHere />");
			}
		}
	}
}
