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
    public class TeleoptiDetailedExportTest : BaseExportTest
    {
        [Test]
        public void VerifyPayrollFormat()
        {
            PayrollFormatDto payrollFormatDto = Target.PayrollFormat;
            Assert.AreEqual("Teleopti Detailed Export", payrollFormatDto.Name);
            Assert.AreEqual(new Guid("{605b87c4-b98a-4fe1-9ea2-9b7308caa947}"), payrollFormatDto.FormatId);
        }

        protected override void ConcreteSetup()
        {
            Target = new TeleoptiDetailedExport();
        }

        [Test]
        public void VerifyPayrollTimeExport()
        {
            MockRepository mocks = new MockRepository();

            DateTimePeriodDtoForPayrollTest dateTimePeriodDto = new DateTimePeriodDtoForPayrollTest(new DateTime(2009, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2009, 2, 28, 0, 0, 0, DateTimeKind.Utc));

            TargetDateOnlyPeriodDto = new DateOnlyPeriodDtoForPayrollTest(
                new DateOnly(2009, 2, 1),
                new DateOnly(2009, 2, 28));

            
            ITeleoptiOrganizationService organizationService = mocks.StrictMock<ITeleoptiOrganizationService>();
            ITeleoptiSchedulingService schedulingService = mocks.StrictMock<ITeleoptiSchedulingService>();

            PersonDto person1 = new PersonDtoForPayrollTest(Guid.NewGuid());
            PersonDto person2 = new PersonDtoForPayrollTest(Guid.NewGuid());
            IList<PersonDto> personDtos = new List<PersonDto>
                                              {
                                                  person1,
                                                  person2
                                              };

            AbsenceDto absence = new AbsenceDto {PayrollCode = "801", Id = Guid.Empty};
            ProjectedLayerDto projectedLayerDto = new ProjectedLayerDto();
            projectedLayerDto.IsAbsence = true;
            projectedLayerDto.PayloadId = absence.Id.Value;
            
            projectedLayerDto.Period = dateTimePeriodDto;
            projectedLayerDto.ContractTime = TimeSpan.FromHours(7.5);
            MultiplicatorDto multiplicator = new MultiplicatorDto(null) { PayrollCode = "371" };
            SchedulePartDto schedulePartDto1 = new SchedulePartDto();
            SchedulePartDto schedulePartDto2 = new SchedulePartDto();
            //schedulePartDto1.ProjectedLayerCollection.Clear();
            schedulePartDto1.ProjectedLayerCollection.Add(projectedLayerDto);
            //schedulePartDto2.ProjectedLayerCollection.Clear();
            DateOnlyDto dateOnlyDto = TargetDateOnlyPeriodDto.StartDate;
            schedulePartDto1.PersonId = person1.Id.Value;
            schedulePartDto2.PersonId = person2.Id.Value;

            Expect.Call(schedulingService.GetAbsences(new AbsenceLoadOptionDto {LoadDeleted = true})).Return(
                new List<AbsenceDto> {absence}).IgnoreArguments();
#pragma warning disable 612,618
            Expect.Call(schedulingService.GetSchedulePartsForPersons(new[] { person1 , person2}, dateOnlyDto, dateOnlyDto, "Utc")).Return(
#pragma warning restore 612,618
                new List<SchedulePartDto> { schedulePartDto1, schedulePartDto2 }).IgnoreArguments();

            Expect.Call(schedulingService.GetPersonMultiplicatorDataForPersons(new[] { person1, person2 }, dateOnlyDto, dateOnlyDto, "Utc")).Return(new List<MultiplicatorDataDto>
                    {
                        new MultiplicatorDataDto
                            {
                                Amount = TimeSpan.FromHours(8.25),
                                Date = TargetDateOnlyPeriodDto.StartDate.DateTime,
                                Multiplicator = multiplicator,
                                PersonId = person2.Id
                            }
                    }).IgnoreArguments();

            mocks.ReplayAll();

            IXPathNavigable result = Target.ProcessPayrollData(schedulingService, organizationService, new PayrollExportDtoForPayrollTest(personDtos, TargetDateOnlyPeriodDto, null){TimeZoneId = "Utc"});
            XPathNavigator navigator = result.CreateNavigator();
            XPathNavigator xmlNodeList = navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']", person1.Id.Value));
            Assert.AreEqual(TargetDateOnlyPeriodDto.StartDate.DateTime, XmlConvert.ToDateTime(xmlNodeList.SelectSingleNode("parent::node()/Date").Value, XmlDateTimeSerializationMode.Unspecified));
            Assert.AreEqual("801", xmlNodeList.SelectSingleNode("parent::node()/PayrollCode").Value);
            Assert.AreEqual(TimeSpan.FromHours(7.5), XmlConvert.ToTimeSpan(xmlNodeList.SelectSingleNode("parent::node()/Time").Value));
            Assert.IsNotNull(xmlNodeList);
            Assert.AreEqual(person1.Id.Value.ToString(), xmlNodeList.Value);

            xmlNodeList = navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']", person2.Id.Value));
            Assert.AreEqual(TargetDateOnlyPeriodDto.StartDate.DateTime, XmlConvert.ToDateTime(xmlNodeList.SelectSingleNode("parent::node()/Date").Value, XmlDateTimeSerializationMode.Unspecified));
            Assert.AreEqual("371", xmlNodeList.SelectSingleNode("parent::node()/PayrollCode").Value);
            Assert.AreEqual(TimeSpan.FromHours(8.25), XmlConvert.ToTimeSpan(xmlNodeList.SelectSingleNode("parent::node()/Time").Value));
            Assert.IsNotNull(xmlNodeList);
            Assert.AreEqual(person2.Id.Value.ToString(), xmlNodeList.Value);

            mocks.VerifyAll();
        }
    }
}
