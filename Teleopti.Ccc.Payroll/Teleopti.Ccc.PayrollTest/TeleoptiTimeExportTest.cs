using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Payroll;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.PayrollTest
{
    [TestFixture]
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

            MockRepository mocks = new MockRepository();
            ITeleoptiOrganizationService organizationService = mocks.StrictMock<ITeleoptiOrganizationService>();
            ITeleoptiSchedulingService schedulingService = mocks.StrictMock<ITeleoptiSchedulingService>();
            SchedulePartDto schedulePartDto1 = new SchedulePartDto();
            schedulePartDto1.ContractTime = DateTime.MinValue.AddMinutes(450);
            SchedulePartDto schedulePartDto2 = new SchedulePartDto();
            schedulePartDto2.ContractTime = DateTime.MinValue.AddMinutes(510);
            schedulePartDto1.PersonAssignmentCollection.Clear();
            schedulePartDto1.PersonAssignmentCollection.Add(new PersonAssignmentDto());
            schedulePartDto2.PersonAssignmentCollection.Clear();
            schedulePartDto2.PersonAssignmentCollection.Add(new PersonAssignmentDto());
           
            schedulePartDto1.LocalPeriod = dateTimePeriodDto;
            schedulePartDto2.LocalPeriod = dateTimePeriodDto;
            schedulePartDto1.LocalPeriod.LocalStartDateTime = dateTimePeriodDto.LocalStartDateTime.Date;
            schedulePartDto2.LocalPeriod.LocalStartDateTime = dateTimePeriodDto.LocalStartDateTime.Date;
            DateOnlyDto dateOnlyDto = TargetDateOnlyPeriodDto.StartDate;
            PersonDto person1 = new PersonDtoForPayrollTest(Guid.NewGuid());
            PersonDto person2 = new PersonDtoForPayrollTest(Guid.NewGuid());
            IList<PersonDto> personDtos = new List<PersonDto>
                                              {
                                                  person1,
                                                  person2
                                              };
            schedulePartDto1.PersonId = person1.Id.Value;
            schedulePartDto2.PersonId = person2.Id.Value;
#pragma warning disable 612,618
            Expect.Call(schedulingService.GetSchedulePartsForPersons(new[] { person1 ,person2}, dateOnlyDto, dateOnlyDto, "Utc")).Return(
#pragma warning restore 612,618
                new List<SchedulePartDto> { schedulePartDto1, schedulePartDto2 }).IgnoreArguments();

            mocks.ReplayAll();

            IXPathNavigable result = Target.ProcessPayrollData(schedulingService, organizationService, new PayrollExportDtoForPayrollTest(personDtos, TargetDateOnlyPeriodDto, null) { TimeZoneId = "Utc" });
            XPathNavigator navigator = result.CreateNavigator();
            XPathNavigator xmlNodeList = navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']", person1.Id.Value));
            Assert.IsNotNull(xmlNodeList);
            Assert.AreEqual(person1.Id.Value.ToString(), xmlNodeList.Value);
            Assert.AreEqual(TimeSpan.FromHours(7.5),
                            XmlConvert.ToTimeSpan(xmlNodeList.SelectSingleNode("parent::node()/Time").Value));

            xmlNodeList = navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']", person2.Id.Value));
            Assert.IsNotNull(xmlNodeList);
            Assert.AreEqual(person2.Id.Value.ToString(), xmlNodeList.Value);
            Assert.AreEqual(TimeSpan.FromHours(8.5),
                            XmlConvert.ToTimeSpan(xmlNodeList.SelectSingleNode("parent::node()/Time").Value));

            mocks.VerifyAll();
        } 
    }
}
