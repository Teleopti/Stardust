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

		    DateTimePeriodDtoForPayrollTest dateTimePeriodDto =
			    new DateTimePeriodDtoForPayrollTest(new DateTime(2009, 2, 1, 0, 0, 0, DateTimeKind.Utc),
			                                        new DateTime(2009, 2, 28, 0, 0, 0, DateTimeKind.Utc));

		    TargetDateOnlyPeriodDto = new DateOnlyPeriodDtoForPayrollTest(
			    new DateOnly(2009, 2, 1),
			    new DateOnly(2009, 2, 28));


		    ITeleoptiOrganizationService organizationService = mocks.StrictMock<ITeleoptiOrganizationService>();
		    ITeleoptiSchedulingService schedulingService = mocks.StrictMock<ITeleoptiSchedulingService>();

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

		    person3.PersonPeriodCollection.Add(new PersonPeriodDto
			    {
				    Period =
					    new DateOnlyPeriodDto {StartDate = new DateOnlyDto(2009, 2, 2)}
			    });
		    person4.TerminationDate = new DateOnlyDto(2009, 2, 15);
		    AbsenceDto absence = new AbsenceDto {PayrollCode = "801", Id = Guid.Empty};
		    ProjectedLayerDto projectedLayerDto = new ProjectedLayerDto();
		    projectedLayerDto.IsAbsence = true;
		    projectedLayerDto.PayloadId = absence.Id.Value;

		    projectedLayerDto.Period = dateTimePeriodDto;
		    projectedLayerDto.ContractTime = TimeSpan.FromHours(7.5);
		    MultiplicatorDto multiplicator = new MultiplicatorDto() {PayrollCode = "371"};
		    SchedulePartDto schedulePartDto1 = new SchedulePartDto();
		    SchedulePartDto schedulePartDto2 = new SchedulePartDto();
		    SchedulePartDto schedulePartDto3 = new SchedulePartDto {Date = new DateOnlyDto(2009, 2, 1)};
		    SchedulePartDto schedulePartDto4 = new SchedulePartDto {Date = new DateOnlyDto(2009, 2, 16)};
		    //schedulePartDto1.ProjectedLayerCollection.Clear();
		    schedulePartDto1.ProjectedLayerCollection.Add(projectedLayerDto);
		    //schedulePartDto2.ProjectedLayerCollection.Clear();
		    DateOnlyDto startDate = TargetDateOnlyPeriodDto.StartDate;
		    DateOnlyDto endDate = TargetDateOnlyPeriodDto.EndDate;
		    schedulePartDto1.PersonId = person1.Id.Value;
		    schedulePartDto2.PersonId = person2.Id.Value;
		    schedulePartDto3.PersonId = person3.Id.Value;
		    schedulePartDto4.PersonId = person4.Id.Value;

		    Expect.Call(schedulingService.GetAbsences(new AbsenceLoadOptionDto {LoadDeleted = true})).Return(
			    new List<AbsenceDto> {absence}).IgnoreArguments();
#pragma warning disable 612,618
		    Expect.Call(schedulingService.GetSchedulePartsForPersons(new[] {person1, person2, person3, person4}, startDate,
		                                                             endDate, "Utc")).Return(
#pragma warning restore 612,618
			    new List<SchedulePartDto> {schedulePartDto1, schedulePartDto2, schedulePartDto3, schedulePartDto4})
		          .IgnoreArguments();

		    Expect.Call(schedulingService.GetPersonMultiplicatorDataForPersons(new[] {person1, person2, person3, person4},
		                                                                       startDate, endDate, "Utc"))
		          .Return(new List<MultiplicatorDataDto>
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

		    IXPathNavigable result = Target.ProcessPayrollData(schedulingService, organizationService,
		                                                       new PayrollExportDtoForPayrollTest(personDtos,
		                                                                                          TargetDateOnlyPeriodDto,
		                                                                                          null) {TimeZoneId = "Utc"});
		    XPathNavigator navigator = result.CreateNavigator();
		    XPathNavigator xmlNodeList =
			    navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']",
			                                             person1.Id.Value));
		    Assert.AreEqual(TargetDateOnlyPeriodDto.StartDate.DateTime,
		                    XmlConvert.ToDateTime(xmlNodeList.SelectSingleNode("parent::node()/Date").Value,
		                                          XmlDateTimeSerializationMode.Unspecified));
		    Assert.AreEqual("801", xmlNodeList.SelectSingleNode("parent::node()/PayrollCode").Value);
		    Assert.AreEqual(TimeSpan.FromHours(7.5),
		                    XmlConvert.ToTimeSpan(xmlNodeList.SelectSingleNode("parent::node()/Time").Value));
		    Assert.IsNotNull(xmlNodeList);
		    Assert.AreEqual(person1.Id.Value.ToString(), xmlNodeList.Value);

		    xmlNodeList =
			    navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']",
			                                             person2.Id.Value));
		    Assert.AreEqual(TargetDateOnlyPeriodDto.StartDate.DateTime,
		                    XmlConvert.ToDateTime(xmlNodeList.SelectSingleNode("parent::node()/Date").Value,
		                                          XmlDateTimeSerializationMode.Unspecified));
		    Assert.AreEqual("371", xmlNodeList.SelectSingleNode("parent::node()/PayrollCode").Value);
		    Assert.AreEqual(TimeSpan.FromHours(8.25),
		                    XmlConvert.ToTimeSpan(xmlNodeList.SelectSingleNode("parent::node()/Time").Value));
		    Assert.IsNotNull(xmlNodeList);
		    Assert.AreEqual(person2.Id.Value.ToString(), xmlNodeList.Value);

		    xmlNodeList =
			    navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']",
			                                             person3.Id.Value));
		    Assert.IsNull(xmlNodeList);

		    xmlNodeList =
			    navigator.SelectSingleNode(string.Format(System.Globalization.CultureInfo.InvariantCulture, "//Person[.='{0}']",
			                                             person4.Id.Value));
		    Assert.IsNull(xmlNodeList);

		    mocks.VerifyAll();
	    }
    }
}
