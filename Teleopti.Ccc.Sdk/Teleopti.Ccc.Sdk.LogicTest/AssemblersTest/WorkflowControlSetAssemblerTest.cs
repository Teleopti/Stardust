using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class WorkflowControlSetAssemblerTest
    {
        [Test]
        public void ShouldMapDomainEntityToDto()
        {
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryAssembler = new ShiftCategoryAssembler(new FakeShiftCategoryRepository());
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var target = new WorkflowControlSetAssembler(shiftCategoryAssembler, dayOffAssembler, new ActivityAssembler(new FakeActivityRepository()), absenceAssembler);

			IShiftCategory shiftCategory = new ShiftCategory("shiftCategory").WithId();
            IDayOffTemplate dayOff = new DayOffTemplate(new Description("dayOff")).WithId();
            IAbsence absence = new Absence().WithId();
            
            IWorkflowControlSet domainObject = new WorkflowControlSet("controlset") { AllowedPreferenceActivity = new Activity("Lunch").WithId() }.WithId();
            domainObject.AddAllowedPreferenceShiftCategory(shiftCategory);
            domainObject.AddAllowedPreferenceDayOff(dayOff);
            domainObject.AddAllowedPreferenceAbsence(absence);
            domainObject.PreferencePeriod = new DateOnlyPeriod(2010, 2, 1, 2010, 2, 2);
            domainObject.PreferenceInputPeriod = new DateOnlyPeriod(2010, 2, 3, 2010, 2, 4);
            domainObject.StudentAvailabilityPeriod = new DateOnlyPeriod(2008, 2, 1, 2009, 2, 2);
            domainObject.StudentAvailabilityInputPeriod = new DateOnlyPeriod(2008, 2, 3, 2009, 2, 4);

            var dto = target.DomainEntityToDto(domainObject);
			
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
            Assert.That(dto.AllowedPreferenceShiftCategories.Single().Id, Is.EqualTo(shiftCategory.Id));
            Assert.That(dto.AllowedPreferenceDayOffs.Single().Id, Is.EqualTo(dayOff.Id));
            Assert.That(dto.AllowedPreferenceAbsences.Single().Id, Is.EqualTo(absence.Id));
        }

        [Test]
        public void ShouldMapDomainEntityWithoutAllowedPreferenceActivityToDto()
        {
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryAssembler = new ShiftCategoryAssembler(new FakeShiftCategoryRepository());
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var target = new WorkflowControlSetAssembler(shiftCategoryAssembler, dayOffAssembler, new ActivityAssembler(new FakeActivityRepository()), absenceAssembler);

			var workflowControlSetDomain = new WorkflowControlSet("controlset").WithId();
            
            var dto = target.DomainEntityToDto(workflowControlSetDomain);

            Assert.That(dto.AllowedPreferenceActivity, Is.Null);
        }
    }
}
