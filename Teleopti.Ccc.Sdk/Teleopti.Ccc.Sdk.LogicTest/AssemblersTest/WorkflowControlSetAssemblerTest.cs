using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class WorkflowControlSetAssemblerTest
    {
        private WorkflowControlSetAssembler _target;
        private MockRepository mocks;
        private IAssembler<IDayOffTemplate, DayOffInfoDto> _dayOffAssembler;
        private IAssembler<IShiftCategory, ShiftCategoryDto> _shiftCategoryAssembler;
        private IAssembler<IAbsence, AbsenceDto> _absenceAssembler;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            _dayOffAssembler = mocks.StrictMock<IAssembler<IDayOffTemplate, DayOffInfoDto>>();
            _shiftCategoryAssembler = mocks.StrictMock<IAssembler<IShiftCategory, ShiftCategoryDto>>();
            _absenceAssembler = mocks.StrictMock<IAssembler<IAbsence, AbsenceDto>>();
            _target = new WorkflowControlSetAssembler(_shiftCategoryAssembler,_dayOffAssembler,new ActivityAssembler(null), _absenceAssembler);
        }

        [Test]
        public void ShouldMapDomainEntityToDto()
        {
            IShiftCategory shiftCategory = new ShiftCategory("shiftCategory");
            IDayOffTemplate dayOff = new DayOffTemplate(new Description("dayOff"));
            IAbsence absence = new Absence();
            var dayOffDto = new DayOffInfoDto();
            var shiftCategoryDto = new ShiftCategoryDto();
            var absenceDto = new AbsenceDto();

            using(mocks.Record())
            {
                Expect.Call(_dayOffAssembler.DomainEntityToDto(dayOff)).Return(dayOffDto);
                Expect.Call(_shiftCategoryAssembler.DomainEntityToDto(shiftCategory)).Return(shiftCategoryDto);
                Expect.Call(_absenceAssembler.DomainEntityToDto(absence)).Return(absenceDto);
            }
            
            IWorkflowControlSet domainObject = new WorkflowControlSet("controlset")
            {
                AllowedPreferenceActivity = new Activity("Lunch"),
            };
            domainObject.SetId(Guid.NewGuid());
            domainObject.AllowedPreferenceActivity.SetId(Guid.NewGuid());
            domainObject.AddAllowedPreferenceShiftCategory(shiftCategory);
            domainObject.AddAllowedPreferenceDayOff(dayOff);
            domainObject.AddAllowedPreferenceAbsence(absence);
            domainObject.PreferencePeriod = new DateOnlyPeriod(2010, 2, 1, 2010, 2, 2);
            domainObject.PreferenceInputPeriod = new DateOnlyPeriod(2010, 2, 3, 2010, 2, 4);
            domainObject.StudentAvailabilityPeriod = new DateOnlyPeriod(2008, 2, 1, 2009, 2, 2);
            domainObject.StudentAvailabilityInputPeriod = new DateOnlyPeriod(2008, 2, 3, 2009, 2, 4);

            var dto = _target.DomainEntityToDto(domainObject);

            IEnumerator<ShiftCategoryDto> enShiftCategory = dto.AllowedPreferenceShiftCategories.GetEnumerator();
            enShiftCategory.MoveNext();

            IEnumerator<AbsenceDto> enAbsence = dto.AllowedPreferenceAbsences.GetEnumerator();
            enAbsence.MoveNext();

            IEnumerator<DayOffInfoDto> enDayOff = dto.AllowedPreferenceDayOffs.GetEnumerator();
            enDayOff.MoveNext();

            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.AllowedPreferenceActivity.Description, Is.EqualTo(domainObject.AllowedPreferenceActivity.Description.Name));
            Assert.That(dto.PreferencePeriod.StartDate.DateTime, Is.EqualTo(domainObject.PreferencePeriod.StartDate.Date));
            Assert.That(dto.PreferencePeriod.EndDate.DateTime, Is.EqualTo(domainObject.PreferencePeriod.EndDate.Date));
            Assert.That(dto.PreferenceInputPeriod.StartDate.DateTime, Is.EqualTo(domainObject.PreferenceInputPeriod.StartDate.Date));
            Assert.That(dto.PreferenceInputPeriod.EndDate.DateTime, Is.EqualTo(domainObject.PreferenceInputPeriod.EndDate.Date));
            Assert.That(dto.StudentAvailabilityPeriod.StartDate.DateTime, Is.EqualTo(domainObject.StudentAvailabilityPeriod.StartDate.Date));
            Assert.That(dto.StudentAvailabilityPeriod.EndDate.DateTime, Is.EqualTo(domainObject.StudentAvailabilityPeriod.EndDate.Date));
            Assert.That(dto.StudentAvailabilityInputPeriod.StartDate.DateTime, Is.EqualTo(domainObject.StudentAvailabilityInputPeriod.StartDate.Date));
            Assert.That(dto.StudentAvailabilityInputPeriod.EndDate.DateTime, Is.EqualTo(domainObject.StudentAvailabilityInputPeriod.EndDate.Date));
            Assert.That(dto.AllowedPreferenceShiftCategories.Count, Is.EqualTo(1));
            Assert.That(enShiftCategory.Current, Is.EqualTo(shiftCategoryDto));
            Assert.That(dto.AllowedPreferenceDayOffs.Count, Is.EqualTo(1));
            Assert.That(enDayOff.Current, Is.EqualTo(dayOffDto));
            Assert.That(dto.AllowedPreferenceAbsences.Count, Is.EqualTo(1));
            Assert.That(enAbsence.Current, Is.EqualTo(absenceDto));
        }

        [Test]
        public void ShouldMapDomainEntityWithoutAllowedPreferenceActivityToDto()
        {
            IWorkflowControlSet workflowControlSetDomain = new WorkflowControlSet("controlset");
            workflowControlSetDomain.SetId(Guid.NewGuid());

            var dto = _target.DomainEntityToDto(workflowControlSetDomain);

            Assert.That(dto.AllowedPreferenceActivity, Is.Null);
        }
    }
}
