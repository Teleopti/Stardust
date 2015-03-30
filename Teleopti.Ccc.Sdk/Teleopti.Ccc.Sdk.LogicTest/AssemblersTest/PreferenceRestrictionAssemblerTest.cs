using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PreferenceRestrictionAssemblerTest
    {
        private MockRepository _mocks;
        private PreferenceDayAssembler _target;
        private IPerson _person;
        private readonly DateOnly _date = new DateOnly(2009,2,2);
        private IActivity _activity;
        private IShiftCategory _shiftCategory;
        private IDayOffTemplate _dayOffTemplate;
        private IAssembler<IPerson, PersonDto> _personAssembler;
        private bool _mustHave;
        private IAssembler<IPreferenceRestriction, PreferenceRestrictionDto> _restrictionAssembler;
        private const string _templateName = "My template";

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
            _restrictionAssembler = _mocks.StrictMock<IAssembler<IPreferenceRestriction, PreferenceRestrictionDto>>();
            _target = new PreferenceDayAssembler(_restrictionAssembler, _personAssembler);
            _person = PersonFactory.CreatePerson();
            _person.Name = new Name("ett fint namn", "");
            _person.SetId(Guid.NewGuid());
            _activity = ActivityFactory.CreateActivity("Activity");
            _activity.SetId(Guid.NewGuid());
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Category");
            _shiftCategory.SetId(Guid.NewGuid());
            _dayOffTemplate = DayOffFactory.CreateDayOff(new Description("DayOff"));
            _dayOffTemplate.SetId(Guid.NewGuid());
            _mustHave = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldMapDtoToDomainEntity()
        {
            PreferenceRestrictionDto dto = CreatePreferenceRestrictionDto();
            using (_mocks.Record())
            {
                Expect.Call(_restrictionAssembler.DtoToDomainEntity(dto)).Return(new PreferenceRestriction());
                Expect.Call(_personAssembler.DtoToDomainEntity(dto.Person)).Return(_person);
            }
            using (_mocks.Playback())
            {
                IPreferenceDay domainEntity = _target.DtoToDomainEntity(dto);
                Assert.AreEqual(dto.Person.Name, domainEntity.Person.Name.FirstName);
                Assert.AreEqual(dto.MustHave, domainEntity.Restriction.MustHave);
                Assert.AreEqual(dto.TemplateName,domainEntity.TemplateName);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldMapDomainEntityToDto()
        {
            IPreferenceDay domainEntity = CreatePreferenceRestriction();
            using (_mocks.Record())
            {
                Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
                                                                                    {
                                                                                        Id = _person.Id,
                                                                                        Name = _person.Name.ToString()
                                                                                    });
                Expect.Call(_restrictionAssembler.DomainEntityToDto(null))
                    .Return(new PreferenceRestrictionDto())
                    .IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                PreferenceRestrictionDto dto = _target.DomainEntityToDto(domainEntity);
                Assert.AreEqual(domainEntity.Person.Name.FirstName, dto.Person.Name);
                Assert.AreEqual(domainEntity.RestrictionDate.Date, dto.RestrictionDate.DateTime);
                Assert.AreEqual(domainEntity.Restriction.MustHave, dto.MustHave);
                Assert.AreEqual(domainEntity.TemplateName, dto.TemplateName);
            }
        }

        private PreferenceRestrictionDto CreatePreferenceRestrictionDto()
        {
            PreferenceRestrictionDto dto = new PreferenceRestrictionDto();
            dto.DayOff = new DayOffInfoDto{Id = _dayOffTemplate.Id,Name = _dayOffTemplate.Description.Name};
            dto.ShiftCategory = new ShiftCategoryDto
                                    {
                                        Name = _shiftCategory.Description.Name,
                                        Id = _shiftCategory.Id
                                    };
            dto.ActivityRestrictionCollection.Add(new ActivityRestrictionDto
                                                      {
                                                         Activity  = new ActivityDto{Description = _activity.Description.Name,PayrollCode = _activity.PayrollCode, Id = _activity.Id}
                                                      });
			dto.RestrictionDate = new DateOnlyDto { DateTime = _date.Date };
            dto.Person = new PersonDto{Id = _person.Id,Name = _person.Name.ToString()};
            dto.MustHave = _mustHave;
            dto.TemplateName = _templateName;
            
            return dto;
        }

        private IPreferenceDay CreatePreferenceRestriction()
        {
            IPreferenceRestriction restrictionNew = new PreferenceRestriction();
            
            restrictionNew.AddActivityRestriction(new ActivityRestriction(_activity));

            restrictionNew.ShiftCategory = _shiftCategory;
            restrictionNew.DayOffTemplate = _dayOffTemplate;
            IPreferenceDay day = new PreferenceDay(_person, _date, restrictionNew);
            day.Restriction.MustHave = _mustHave;
            day.TemplateName = _templateName;
            return day;
        }
    }

    
}
