using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class RestrictionAssemblerTest
    {
        private RestrictionAssembler<IPreferenceRestriction, PreferenceRestrictionDto, IActivityRestriction> target;
        private MockRepository mocks;
        private IAssembler<IDayOffTemplate, DayOffInfoDto> dayOffAssembler;
        private IAssembler<IShiftCategory, ShiftCategoryDto> shiftCategoryAssembler;
        private IAssembler<IAbsence, AbsenceDto> _absenceAssembler;
        private IPreferenceRestriction restriction;
        private IShiftCategory shiftCategory;
        private IDayOffTemplate dayOffTemplate;
        private IAssembler<IActivityRestriction, ActivityRestrictionDto> activityRestrictionAssembler;
        private IActivityRestriction activityRestriction;
        private IActivity activity;
        private IDomainAndDtoConstructor<IPreferenceRestriction, PreferenceRestrictionDto> domainAndDtoConstructor;
        private IAbsence _absence;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            activity = mocks.StrictMock<IActivity>();
            dayOffAssembler = mocks.StrictMock<IAssembler<IDayOffTemplate, DayOffInfoDto>>();
            shiftCategoryAssembler = mocks.StrictMock<IAssembler<IShiftCategory, ShiftCategoryDto>>();
            activityRestrictionAssembler = mocks.StrictMock<IAssembler<IActivityRestriction, ActivityRestrictionDto>>();
            _absenceAssembler = mocks.StrictMock<IAssembler<IAbsence, AbsenceDto>>();
            domainAndDtoConstructor = mocks.StrictMock<IDomainAndDtoConstructor<IPreferenceRestriction, PreferenceRestrictionDto>>();
            shiftCategory = mocks.StrictMock<IShiftCategory>();
            dayOffTemplate = mocks.StrictMock<IDayOffTemplate>();
            _absence = mocks.StrictMock<IAbsence>();
            activityRestriction = new ActivityRestriction(activity);
            restriction = new PreferenceRestriction
                              {
                                  ShiftCategory = shiftCategory,
                                  DayOffTemplate = dayOffTemplate,
                                  Absence = _absence,
                                  StartTimeLimitation =
                                      new StartTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(8)),
                                  EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(17)),
                                  WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(9))
                              };
            restriction.AddActivityRestriction(activityRestriction);
            target =
                new RestrictionAssembler<IPreferenceRestriction, PreferenceRestrictionDto, IActivityRestriction>(domainAndDtoConstructor, shiftCategoryAssembler, dayOffAssembler, activityRestrictionAssembler, _absenceAssembler);
        }

        [Test]
        public void VerifyDomainToDto()
        {
            ShiftCategoryDto shiftCategoryDto = new ShiftCategoryDto();
            DayOffInfoDto dayOffInfoDto = new DayOffInfoDto();
            AbsenceDto absenceDto = new AbsenceDto();
            ActivityRestrictionDto activityRestrictionDto = new ActivityRestrictionDto();
            using (mocks.Record())
            {
                Expect.Call(shiftCategoryAssembler.DomainEntityToDto(shiftCategory)).Return(shiftCategoryDto);
                Expect.Call(dayOffAssembler.DomainEntityToDto(dayOffTemplate)).Return(dayOffInfoDto);
                Expect.Call(_absenceAssembler.DomainEntityToDto(_absence)).Return(absenceDto);
                Expect.Call(activityRestrictionAssembler.DomainEntityToDto(activityRestriction)).Return(
                    activityRestrictionDto);
                Expect.Call(domainAndDtoConstructor.CreateNewDto()).Return(new PreferenceRestrictionDto());
            }
            using (mocks.Playback())
            {
                var dto = target.DomainEntityToDto(restriction);
                Assert.AreEqual(dayOffInfoDto, dto.DayOff);
                Assert.AreEqual(shiftCategoryDto,dto.ShiftCategory);
                Assert.AreEqual(absenceDto, dto.Absence);
                Assert.AreEqual(restriction.StartTimeLimitation.StartTime,dto.StartTimeLimitation.MinTime);
                Assert.AreEqual(restriction.EndTimeLimitation.StartTime,dto.EndTimeLimitation.MinTime);
                Assert.AreEqual(restriction.WorkTimeLimitation.StartTime,dto.WorkTimeLimitation.MinTime);
                Assert.AreEqual(1,dto.ActivityRestrictionCollection.Count);
            }
        }

        [Test]
        public void VerifyDtoToDomain()
        {
            ShiftCategoryDto shiftCategoryDto = new ShiftCategoryDto();
            DayOffInfoDto dayOffInfoDto = new DayOffInfoDto();
            AbsenceDto absenceDto = new AbsenceDto();
            ActivityRestrictionDto activityRestrictionDto = new ActivityRestrictionDto();
            PreferenceRestrictionDto preferenceRestrictionDto = new PreferenceRestrictionDto
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
            using (mocks.Record())
            {
                Expect.Call(shiftCategoryAssembler.DtoToDomainEntity(shiftCategoryDto)).Return(shiftCategory);
                Expect.Call(dayOffAssembler.DtoToDomainEntity(dayOffInfoDto)).Return(dayOffTemplate);
                Expect.Call(_absenceAssembler.DtoToDomainEntity(absenceDto)).Return(_absence);
                Expect.Call(activityRestrictionAssembler.DtoToDomainEntity(activityRestrictionDto)).Return(
                    activityRestriction);
                Expect.Call(domainAndDtoConstructor.CreateNewDomainObject()).Return(new PreferenceRestriction());
            }
            using (mocks.Playback())
            {
                var domainEntity = target.DtoToDomainEntity(preferenceRestrictionDto);
                Assert.AreEqual(dayOffTemplate, domainEntity.DayOffTemplate);
                Assert.AreEqual(shiftCategory, domainEntity.ShiftCategory);
                Assert.AreEqual(_absence,domainEntity.Absence);
                Assert.AreEqual(preferenceRestrictionDto.StartTimeLimitation.MinTime, domainEntity.StartTimeLimitation.StartTime);
                Assert.AreEqual(preferenceRestrictionDto.EndTimeLimitation.MinTime, domainEntity.EndTimeLimitation.StartTime);
                Assert.AreEqual(preferenceRestrictionDto.WorkTimeLimitation.MinTime, domainEntity.WorkTimeLimitation.StartTime);
                Assert.AreEqual(preferenceRestrictionDto.ActivityRestrictionCollection.Count,
                                domainEntity.ActivityRestrictionCollection.Count);
            }
        }
    }
}
