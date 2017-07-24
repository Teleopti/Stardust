using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Payroll;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.PayrollTest
{
    [TestFixture, Parallelizable]
    public class TeleoptiTimeExportTest : BaseExportTest
    {
        protected override void ConcreteSetup()
        {
            Target = new TeleoptiTimeExport();
        }

        [Test]
        public void VerifyPayrollFormat()
        {
            PayrollFormatDto payrollFormatDto = Target.PayrollFormat;
            Assert.AreEqual("Teleopti Time Export",payrollFormatDto.Name);
            Guid guid = new Guid("{5A888BEC-5954-466d-B245-639BFEDA1BB5}");
            Assert.AreEqual(guid, payrollFormatDto.FormatId);
        }

        [Test]
        public void VerifyPayrollTimeExport()
		{
			DateTimePeriodDtoForPayrollTest dateTimePeriodDto = new DateTimePeriodDtoForPayrollTest(new DateTime(2009, 2, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2009, 2, 1, 0, 0, 0, DateTimeKind.Utc));

			TargetDateOnlyPeriodDto = new DateOnlyPeriodDtoForPayrollTest(
				new DateOnly(2009, 2, 1),
				new DateOnly(2009, 2, 1));
			
			ITeleoptiOrganizationService organizationService = MockRepository.GenerateMock<ITeleoptiOrganizationService>();
			ITeleoptiSchedulingService schedulingService = MockRepository.GenerateMock<ITeleoptiSchedulingService>();
			SchedulePartDto schedulePartDto1 = new SchedulePartDto();
			SchedulePartDto schedulePartDto2 = new SchedulePartDto();
			SchedulePartDto schedulePartDto3 = new SchedulePartDto();
			SchedulePartDto schedulePartDto4 = new SchedulePartDto();
			schedulePartDto1.ContractTime = DateTime.MinValue.AddMinutes(450);
			schedulePartDto2.ContractTime = DateTime.MinValue.AddMinutes(510);
			schedulePartDto1.PersonAssignmentCollection.Clear();
			schedulePartDto1.PersonAssignmentCollection.Add(new PersonAssignmentDto());
			schedulePartDto2.PersonAssignmentCollection.Clear();
			schedulePartDto2.PersonAssignmentCollection.Add(new PersonAssignmentDto());

			schedulePartDto1.LocalPeriod = dateTimePeriodDto;
			schedulePartDto2.LocalPeriod = dateTimePeriodDto;
			schedulePartDto1.LocalPeriod.LocalStartDateTime = dateTimePeriodDto.LocalStartDateTime.Date;
			schedulePartDto2.LocalPeriod.LocalStartDateTime = dateTimePeriodDto.LocalStartDateTime.Date;
			schedulePartDto3.Date = new DateOnlyDto(2009, 2, 1);
			schedulePartDto4.Date = new DateOnlyDto(2009, 2, 1);
			DateOnlyDto dateOnlyDto = TargetDateOnlyPeriodDto.StartDate;
			PersonDto person1 = new PersonDtoForPayrollTest(Guid.NewGuid());
			PersonDto person2 = new PersonDtoForPayrollTest(Guid.NewGuid());
			PersonDto person3 = new PersonDtoForPayrollTest(Guid.NewGuid());
			PersonDto person4 = new PersonDtoForPayrollTest(Guid.NewGuid());
			IList<PersonDto> personDtos = new List<PersonDto>
                                              {
                                                  person1,
                                                  person2,
                                                  person3,
                                                  person4
                                              };
			schedulePartDto1.PersonId = person1.Id.Value;
			schedulePartDto2.PersonId = person2.Id.Value;
			schedulePartDto3.PersonId = person3.Id.Value;
			schedulePartDto4.PersonId = person4.Id.Value;
			person3.PersonPeriodCollection.Add(new PersonPeriodDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2009, 2, 2) }
			});
			person4.TerminationDate = new DateOnlyDto(2009, 1, 31);

			schedulingService.Stub(x => x.GetTeleoptiTimeExportData(new[] { person1, person2 }, dateOnlyDto, dateOnlyDto,
																	"Utc")).Return(
																		new List<PayrollBaseExportDto>
			                                                                {
				                                                                new PayrollBaseExportDto
					                                                                {
																						EmploymentNumber = person1.EmploymentNumber,
																						ContractTime = new TimeSpan(07,30,00)
					                                                                },
																					new PayrollBaseExportDto
																						{
																							EmploymentNumber = person2.EmploymentNumber,
																							ContractTime = new TimeSpan(08,30,00)
																						}

			                                                                }).IgnoreArguments();
			
			IXPathNavigable result = Target.ProcessPayrollData(schedulingService, organizationService, new PayrollExportDtoForPayrollTest(personDtos, TargetDateOnlyPeriodDto, null) { TimeZoneId = "Utc" });
			XPathNavigator navigator = result.CreateNavigator();
			XPathNavigator xmlNodeList = navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']", person1.Id.Value));
			Assert.IsNotNull(xmlNodeList);
			Assert.AreEqual(person1.Id.Value.ToString(), xmlNodeList.Value);
			Assert.AreEqual(TimeSpan.FromHours(7.5),
							XmlConvert.ToTimeSpan(xmlNodeList.SelectSingleNode("parent::node()/ContractTime").Value));

			xmlNodeList = navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']", person2.Id.Value));
			Assert.IsNotNull(xmlNodeList);
			Assert.AreEqual(person2.Id.Value.ToString(), xmlNodeList.Value);
			Assert.AreEqual(TimeSpan.FromHours(8.5),
							XmlConvert.ToTimeSpan(xmlNodeList.SelectSingleNode("parent::node()/ContractTime").Value));

			xmlNodeList = navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']", person3.Id.Value));
			Assert.IsNull(xmlNodeList);

			xmlNodeList = navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']", person4.Id.Value));
			Assert.IsNull(xmlNodeList);
        }

	    [Test]
	    public void BatchingShouldNotHaveIntersection()
		{
			var schedulingService = MockRepository.GenerateMock<ITeleoptiSchedulingService>();

			var payrollExportDto = new PayrollExportDto
			{
				TimeZoneId = "Utc",
				DatePeriod = new DateOnlyPeriodDtoForPayrollTest(
					new DateOnly(2009, 2, 1),
					new DateOnly(2009, 2, 1))
			};
			for (var i = 0; i < 51; i++)
				payrollExportDto.PersonCollection.Add(new PersonDto());

			schedulingService.Stub(s => s.GetTeleoptiTimeExportData(null, null, null, null))
							 .IgnoreArguments()
							 .Return(new List<PayrollBaseExportDto>
			                     {
				                     new PayrollBaseExportDto(),
				                     new PayrollBaseExportDto()
			                     });
			
			Target.ProcessPayrollData(schedulingService, null, payrollExportDto);
			var argumentsUsed =
				schedulingService.GetArgumentsForCallsMadeOn(s => s.GetTeleoptiTimeExportData(null, null, null, null));

			var firstList = (PersonDto[])argumentsUsed[0][0];
			var secondList = (PersonDto[])argumentsUsed[1][0];

			var intersection = firstList.Intersect(secondList);
			intersection.Count().Should().Be.EqualTo(0);
	    }
    }
}
