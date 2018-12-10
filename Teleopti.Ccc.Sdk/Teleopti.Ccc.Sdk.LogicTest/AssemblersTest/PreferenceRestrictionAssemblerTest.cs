using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PreferenceRestrictionAssemblerTest
    {
	    [Test]
	    public void ShouldMapDtoToDomainEntity()
	    {
		    var personRepository = new FakePersonRepositoryLegacy();
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
		    var activityRepository = new FakeActivityRepository();
		    var activityAssembler = new ActivityAssembler(activityRepository);
		    var dayOffTemplateRepository = new FakeDayOffTemplateRepository();
		    var dayOffAssembler = new DayOffAssembler(dayOffTemplateRepository);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(shiftCategoryAssembler,
				    dayOffAssembler, activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy());
		    var restrictionAssembler =
			    new RestrictionAssembler<IPreferenceRestriction, PreferenceRestrictionDto, IActivityRestriction>(
				    new PreferenceRestrictionConstructor(), shiftCategoryAssembler,
					    dayOffAssembler,
					    new ActivityRestrictionAssembler<IActivityRestriction>(new ActivityRestrictionDomainObjectCreator(),
						    activityAssembler), absenceAssembler);
			var target = new PreferenceDayAssembler(restrictionAssembler, personAssembler);

			var person = PersonFactory.CreatePerson().WithId();
			var activity = ActivityFactory.CreateActivity("Activity").WithId();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Category").WithId();
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("DayOff")).WithId();
			var mustHave = true;

			personRepository.Add(person);
			activityRepository.Add(activity);
			shiftCategoryRepository.Add(shiftCategory);
			dayOffTemplateRepository.Add(dayOffTemplate);

			var dto = new PreferenceRestrictionDto();
			dto.DayOff = new DayOffInfoDto { Id = dayOffTemplate.Id, Name = dayOffTemplate.Description.Name };
			dto.ShiftCategory = new ShiftCategoryDto
			{
				Name = shiftCategory.Description.Name,
				Id = shiftCategory.Id
			};
			dto.ActivityRestrictionCollection.Add(new ActivityRestrictionDto
			{
				Activity = new ActivityDto { Description = activity.Description.Name, PayrollCode = activity.PayrollCode, Id = activity.Id }
			});
			dto.RestrictionDate = new DateOnlyDto { DateTime = new DateOnly(2001,1,1).Date };
			dto.Person = new PersonDto { Id = person.Id, Name = person.Name.ToString() };
			dto.MustHave = mustHave;
			dto.TemplateName = "template name";

		    IPreferenceDay domainEntity = target.DtoToDomainEntity(dto);
		    Assert.AreEqual(dto.Person.Name, domainEntity.Person.Name.ToString());
		    Assert.AreEqual(dto.MustHave, domainEntity.Restriction.MustHave);
		    Assert.AreEqual(dto.TemplateName, domainEntity.TemplateName);
	    }

	    [Test]
	    public void ShouldMapDomainEntityToDto()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var activityRepository = new FakeActivityRepository();
			var activityAssembler = new ActivityAssembler(activityRepository);
			var dayOffTemplateRepository = new FakeDayOffTemplateRepository();
			var dayOffAssembler = new DayOffAssembler(dayOffTemplateRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy());
			var restrictionAssembler =
				new RestrictionAssembler<IPreferenceRestriction, PreferenceRestrictionDto, IActivityRestriction>(
					new PreferenceRestrictionConstructor(), shiftCategoryAssembler,
						dayOffAssembler,
						new ActivityRestrictionAssembler<IActivityRestriction>(new ActivityRestrictionDomainObjectCreator(),
							activityAssembler), absenceAssembler);
			var target = new PreferenceDayAssembler(restrictionAssembler, personAssembler);
			var person = PersonFactory.CreatePerson().WithId();
			var activity = ActivityFactory.CreateActivity("Activity").WithId();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Category").WithId();
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("DayOff")).WithId();
			const bool mustHave = true;
			
			IPreferenceRestriction restrictionNew = new PreferenceRestriction();

			restrictionNew.AddActivityRestriction(new ActivityRestriction(activity));

			restrictionNew.ShiftCategory = shiftCategory;
			restrictionNew.DayOffTemplate = dayOffTemplate;
			IPreferenceDay domainEntity = new PreferenceDay(person, new DateOnly(2001,1,1), restrictionNew);
			domainEntity.Restriction.MustHave = mustHave;
			domainEntity.TemplateName = "template name";

			PreferenceRestrictionDto dto = target.DomainEntityToDto(domainEntity);
		    Assert.AreEqual(domainEntity.Person.Name.ToString(), dto.Person.Name);
		    Assert.AreEqual(domainEntity.RestrictionDate.Date, dto.RestrictionDate.DateTime);
		    Assert.AreEqual(domainEntity.Restriction.MustHave, dto.MustHave);
		    Assert.AreEqual(domainEntity.TemplateName, dto.TemplateName);
	    }
    }
}
