using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class RestrictionAssemblerTest
    {
	    [Test]
	    public void VerifyDomainToDto()
	    {
		    var activity = ActivityFactory.CreateActivity("Phone").WithId();
		    var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
		    var shiftCategoryAssembler = new ShiftCategoryAssembler(new FakeShiftCategoryRepository());
		    var activityRestrictionAssembler =
			    new ActivityRestrictionAssembler<IActivityRestriction>(new ActivityRestrictionDomainObjectCreator(),
				    new ActivityAssembler(new FakeActivityRepository()));
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var domainAndDtoConstructor = new PreferenceRestrictionConstructor();
		    var shiftCategory = ShiftCategoryFactory.CreateShiftCategory().WithId();
		    var dayOffTemplate = DayOffFactory.CreateDayOff().WithId();
		    var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
		    var activityRestriction = new ActivityRestriction(activity);
		    var restriction = new PreferenceRestriction
		    {
			    ShiftCategory = shiftCategory,
			    DayOffTemplate = dayOffTemplate,
			    Absence = absence,
			    StartTimeLimitation =
				    new StartTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(8)),
			    EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(17)),
			    WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(9))
		    };
		    restriction.AddActivityRestriction(activityRestriction);
		    var target =
			    new RestrictionAssembler<IPreferenceRestriction, PreferenceRestrictionDto, IActivityRestriction>(
				    domainAndDtoConstructor, shiftCategoryAssembler, dayOffAssembler, activityRestrictionAssembler,
					    absenceAssembler);

		    var dto = target.DomainEntityToDto(restriction);
		    Assert.AreEqual(dayOffTemplate.Id.Value, dto.DayOff.Id);
		    Assert.AreEqual(shiftCategory.Id.Value, dto.ShiftCategory.Id);
		    Assert.AreEqual(absence.Id.Value, dto.Absence.Id);
		    Assert.AreEqual(restriction.StartTimeLimitation.StartTime, dto.StartTimeLimitation.MinTime);
		    Assert.AreEqual(restriction.EndTimeLimitation.StartTime, dto.EndTimeLimitation.MinTime);
		    Assert.AreEqual(restriction.WorkTimeLimitation.StartTime, dto.WorkTimeLimitation.MinTime);
		    Assert.AreEqual(1, dto.ActivityRestrictionCollection.Count);
	    }

	    [Test]
	    public void VerifyDtoToDomain()
		{
			var dayOffTemplateRepository = new FakeDayOffTemplateRepository();
			var absenceRepository = new FakeAbsenceRepository();
			var dayOffAssembler = new DayOffAssembler(dayOffTemplateRepository);
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
		    var activityRepository = new FakeActivityRepository();
		    var activityRestrictionAssembler =
				new ActivityRestrictionAssembler<IActivityRestriction>(new ActivityRestrictionDomainObjectCreator(),
					new ActivityAssembler(activityRepository));
		    var absenceAssembler = new AbsenceAssembler(absenceRepository);
			var domainAndDtoConstructor = new PreferenceRestrictionConstructor();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory().WithId();
			var dayOffTemplate = DayOffFactory.CreateDayOff().WithId();
			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			var activity = ActivityFactory.CreateActivity("Phone").WithId();

			dayOffTemplateRepository.Add(dayOffTemplate);
			absenceRepository.Add(absence);
			shiftCategoryRepository.Add(shiftCategory);
			activityRepository.Add(activity);

			var shiftCategoryDto = new ShiftCategoryDto {Id = shiftCategory.Id};
		    var dayOffInfoDto = new DayOffInfoDto {Id = dayOffTemplate.Id};
		    var absenceDto = new AbsenceDto {Id = absence.Id};
		    var activityDto = new ActivityDto {Id = activity.Id};
		    var activityRestrictionDto = new ActivityRestrictionDto {Activity = activityDto};
		    var preferenceRestrictionDto = new PreferenceRestrictionDto
		    {
			    DayOff = dayOffInfoDto,
			    ShiftCategory = shiftCategoryDto,
			    Absence = absenceDto,
			    StartTimeLimitation =
				    new TimeLimitationDto
				    {MinTime = TimeSpan.FromHours(8)},
			    EndTimeLimitation =
				    new TimeLimitationDto
				    {MinTime = TimeSpan.FromHours(16)},
			    WorkTimeLimitation =
				    new TimeLimitationDto
				    {MinTime = TimeSpan.FromHours(8)}
		    };
		    preferenceRestrictionDto.ActivityRestrictionCollection.Add(activityRestrictionDto);

			var target =
				new RestrictionAssembler<IPreferenceRestriction, PreferenceRestrictionDto, IActivityRestriction>(
					domainAndDtoConstructor, shiftCategoryAssembler, dayOffAssembler, activityRestrictionAssembler,
						absenceAssembler);

			var domainEntity = target.DtoToDomainEntity(preferenceRestrictionDto);
		    Assert.AreEqual(dayOffTemplate, domainEntity.DayOffTemplate);
		    Assert.AreEqual(shiftCategory, domainEntity.ShiftCategory);
		    Assert.AreEqual(absence, domainEntity.Absence);
		    Assert.AreEqual(preferenceRestrictionDto.StartTimeLimitation.MinTime, domainEntity.StartTimeLimitation.StartTime);
		    Assert.AreEqual(preferenceRestrictionDto.EndTimeLimitation.MinTime, domainEntity.EndTimeLimitation.StartTime);
		    Assert.AreEqual(preferenceRestrictionDto.WorkTimeLimitation.MinTime, domainEntity.WorkTimeLimitation.StartTime);
		    Assert.AreEqual(preferenceRestrictionDto.ActivityRestrictionCollection.Count,
			    domainEntity.ActivityRestrictionCollection.Count);
	    }
    }
}
