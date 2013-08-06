using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
		    var mocks = new MockRepository();

		    var dateTimePeriodDto =
			    new DateTimePeriodDtoForPayrollTest(new DateTime(2009, 2, 1, 0, 0, 0, DateTimeKind.Utc),
			                                        new DateTime(2009, 2, 28, 0, 0, 0, DateTimeKind.Utc));

		    TargetDateOnlyPeriodDto = new DateOnlyPeriodDtoForPayrollTest(
			    new DateOnly(2009, 2, 1),
			    new DateOnly(2009, 2, 28));


		    var organizationService = mocks.StrictMock<ITeleoptiOrganizationService>();
		    var schedulingService = mocks.StrictMock<ITeleoptiSchedulingService>();

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
		    var absence = new AbsenceDto {PayrollCode = "801", Id = Guid.Empty};
		    var projectedLayerDto = new ProjectedLayerDto
			    {
				    IsAbsence = true,
				    PayloadId = absence.Id.Value,
				    Period = dateTimePeriodDto,
				    ContractTime = TimeSpan.FromHours(7.5)
			    };

		    var schedulePartDto1 = new SchedulePartDto();
		    var schedulePartDto2 = new SchedulePartDto();
		    var schedulePartDto3 = new SchedulePartDto {Date = new DateOnlyDto(2009, 2, 1)};
		    var schedulePartDto4 = new SchedulePartDto {Date = new DateOnlyDto(2009, 2, 16)};
		    //schedulePartDto1.ProjectedLayerCollection.Clear();
		    schedulePartDto1.ProjectedLayerCollection.Add(projectedLayerDto);
		    //schedulePartDto2.ProjectedLayerCollection.Clear();
		    var startDate = TargetDateOnlyPeriodDto.StartDate;
		    var endDate = TargetDateOnlyPeriodDto.EndDate;
		    schedulePartDto1.PersonId = person1.Id.Value;
		    schedulePartDto2.PersonId = person2.Id.Value;
		    schedulePartDto3.PersonId = person3.Id.Value;
		    schedulePartDto4.PersonId = person4.Id.Value;

			Expect.Call(schedulingService.GetTeleoptiDetailedExportData(new[] { person1, person2, person3, person4 }, startDate,
													 endDate, "Utc")).Return(new Collection<PayrollBaseExportDto>
				                                         {
					                                         new PayrollBaseExportDto
						                                         {
							                                         EmploymentNumber = person1.EmploymentNumber,
																	 Date = new DateTime(2009,02,01),
																	 PayrollCode = "801",
																	 Time = new TimeSpan(07,30,00)
						                                         },
					                                         new PayrollBaseExportDto
						                                         {
							                                         EmploymentNumber = person2.EmploymentNumber,
																	 Date = new DateTime(2009,02,01),
																	 PayrollCode = "371",
																	 Time = new TimeSpan(08,15,00)
						                                         }
				                                         })
							 .IgnoreArguments();
		    
		    mocks.ReplayAll();

		    var result = Target.ProcessPayrollData(schedulingService, organizationService,
		                                                       new PayrollExportDtoForPayrollTest(personDtos,
		                                                                                          TargetDateOnlyPeriodDto,
		                                                                                          null) {TimeZoneId = "Utc"});
		    var navigator = result.CreateNavigator();
		    var xmlNodeList =
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



	    [Test]
	    public void BatchingShouldNotHaveIntersection()
	    {
		    var schedulingService = MockRepository.DynamicMock<ITeleoptiSchedulingService>();
		    var personId = Guid.NewGuid();

		    var payrollExportDto = new PayrollExportDto
			    {
				    TimeZoneId = "Utc",
				    DatePeriod = new DateOnlyPeriodDtoForPayrollTest(
					    new DateOnly(2009, 2, 1),
					    new DateOnly(2009, 2, 1))
			    };

		    payrollExportDto.PersonCollection.Add(new PersonDto
			    {
				    Id = personId,
				    EmploymentNumber = ""
			    });
		    for (var i = 0; i < 50; i++)
			    payrollExportDto.PersonCollection.Add(new PersonDto
				    {
					    Id = Guid.NewGuid(),
					    EmploymentNumber = ""
				    });
		    var multiplicatorDto = new MultiplicatorDto
			    {
				    PayrollCode = ""
			    };

		    schedulingService.Expect(s => s.GetAbsences(null))
		                     .IgnoreArguments()
		                     .Return(new List<AbsenceDto> {new AbsenceDto()});

#pragma warning disable 612,618
		    schedulingService.Expect(s => s.GetSchedulePartsForPersons(null, null, null, null))

		                     .IgnoreArguments()
		                     .Return(new List<SchedulePartDto>
			                     {
				                     new SchedulePartDto
					                     {
						                     PersonId = personId,
					                     }
			                     });

		    schedulingService.Expect(s => s.GetPersonMultiplicatorDataForPersons(null, null, null, null))
		                     .IgnoreArguments()
		                     .Return(new List<MultiplicatorDataDto>
			                     {
				                     new MultiplicatorDataDto
					                     {
						                     PersonId = personId,
						                     Date = new DateTime(),
						                     Multiplicator = multiplicatorDto,
						                     Amount = new TimeSpan()
					                     }
			                     });


		    MockRepository.ReplayAll();

		    Target.ProcessPayrollData(schedulingService, null, payrollExportDto);

		    var multiplicatorArgs =
			    schedulingService.GetArgumentsForCallsMadeOn(s => s.GetPersonMultiplicatorDataForPersons(null, null, null, null));
		    var firstBatchMultiplicatorAgentList = (PersonDto[]) multiplicatorArgs[0][0];
		    var secondBatchMultiplicatorAgentListList = (PersonDto[]) multiplicatorArgs[1][0];
		    var multiplicatorBatchIntersection =
			    firstBatchMultiplicatorAgentList.Intersect(secondBatchMultiplicatorAgentListList);

		    var schedulePartArgs =
			    schedulingService.GetArgumentsForCallsMadeOn(s => s.GetSchedulePartsForPersons(null, null, null, null));
		    var firstBatchSchedulePartsAgentList = (PersonDto[]) schedulePartArgs[0][0];
		    var secondBatchSchedulePartsAgentList = (PersonDto[]) schedulePartArgs[1][0];
		    var schedulePartBatchIntersection = firstBatchSchedulePartsAgentList.Intersect(secondBatchSchedulePartsAgentList);

		    multiplicatorBatchIntersection.Count().Should().Be.EqualTo(0);
		    schedulePartBatchIntersection.Count().Should().Be.EqualTo(0);
#pragma warning restore 612,618
	    }
    }
}
