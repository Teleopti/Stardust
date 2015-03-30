using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.PayrollTest
{
    public abstract class BaseExportTest
    {
        protected IPayrollExportProcessorWithFeedback Target { get; set; }
        protected DateOnlyPeriodDtoForPayrollTest TargetDateOnlyPeriodDto { get; set; }
        protected MockRepository MockRepository { get; private set; }

        [SetUp]
        public void Setup()
        {
            TargetDateOnlyPeriodDto = new DateOnlyPeriodDtoForPayrollTest(
                new DateOnly(2009, 2, 1),
                new DateOnly(2009, 2, 28));
            MockRepository = new MockRepository();
            ConcreteSetup();
            Target.PayrollExportFeedback = MockRepository.DynamicMock<IPayrollExportFeedback>();
        }

        protected abstract void ConcreteSetup();
        
        
        protected class PersonDtoForPayrollTest : PersonDto
        {
            public PersonDtoForPayrollTest(Guid id)
            {
                Id = id;
                EmploymentNumber = id.ToString();
                TimeZoneId = "Utc";
            }
        }

        protected class PayrollExportDtoForPayrollTest : PayrollExportDto
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
            public PayrollExportDtoForPayrollTest(IEnumerable<PersonDto> personCollection, DateOnlyPeriodDto periodDto, PayrollFormatDto payrollFormatDto)
            {
                PayrollFormat = payrollFormatDto;
                DatePeriod = periodDto;
                foreach (PersonDto personDto in personCollection)
                {
                    PersonCollection.Add(personDto);
                }
            }
        }

        protected class DateOnlyPeriodDtoForPayrollTest : DateOnlyPeriodDto
        {
            public DateOnlyPeriodDtoForPayrollTest(DateOnly startDate, DateOnly endDate)
            {
				StartDate = new DateOnlyDto { DateTime = startDate.Date };
				EndDate = new DateOnlyDto { DateTime = endDate.Date };
            }
        }

        protected class DateTimePeriodDtoForPayrollTest : DateTimePeriodDto
        {
            public DateTimePeriodDtoForPayrollTest(DateTime startDateUtc, DateTime endDateUtc)
            {
                UtcStartTime = startDateUtc;
                UtcEndTime = endDateUtc;
            }
        }

    }
}
