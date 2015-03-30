using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class StudentAvailabilityAssemblerTest
    {        
        private StudentAvailabilityAssembler _target;
        private IPerson _person;
        private DateOnly _dateOnly;
        private MockRepository _mocks;
        private IAssembler<IPerson, PersonDto> _personAssembler;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _person = new Person();
            _person.SetId(Guid.Empty);
            _person.Name = new Name("ett namn bara","");
            _dateOnly = new DateOnly(2009,2,2);
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson,PersonDto>>();
            _target = new StudentAvailabilityAssembler();
            _target.PersonAssembler = _personAssembler;
        }

        [Test]
        public void CanCreateDtoOfDomainObject()
        {
            
            IStudentAvailabilityRestriction restriction1 = new StudentAvailabilityRestriction();
            restriction1.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8,0,0),null );
            restriction1.EndTimeLimitation = new EndTimeLimitation(null,new TimeSpan(15,0,0));

            IStudentAvailabilityRestriction restriction2 = new StudentAvailabilityRestriction();
            restriction2.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(10, 0, 0), null);
            restriction2.EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(17, 0, 0));
            restriction2.WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(3,0,0),new TimeSpan(6,0,0) );
            IList<IStudentAvailabilityRestriction> restrictions = new List<IStudentAvailabilityRestriction>{restriction1,restriction2};
            
            using(_mocks.Record())
            {
                Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
                                                                                    {
                                                                                        Id = _person.Id,
                                                                                        Name = _person.Name.ToString()
                                                                                    });
            }
            using (_mocks.Playback())
            {
                IStudentAvailabilityDay day = new StudentAvailabilityDay(_person, _dateOnly, restrictions);
                StudentAvailabilityDayDto dto = _target.DomainEntityToDto(day);

                Assert.AreEqual(day.RestrictionCollection.Count, dto.StudentAvailabilityRestrictions.Count);
                Assert.AreEqual(day.NotAvailable, dto.NotAvailable);
                Assert.AreEqual(day.RestrictionCollection[0].StartTimeLimitation.StartTime, dto.StudentAvailabilityRestrictions[0].StartTimeLimitation.MinTime);
                Assert.AreEqual(day.RestrictionCollection[0].EndTimeLimitation.EndTime, dto.StudentAvailabilityRestrictions[0].EndTimeLimitation.MaxTime);

                Assert.AreEqual(day.RestrictionCollection[1].StartTimeLimitation.StartTime, dto.StudentAvailabilityRestrictions[1].StartTimeLimitation.MinTime);
                Assert.AreEqual(day.RestrictionCollection[1].EndTimeLimitation.EndTime, dto.StudentAvailabilityRestrictions[1].EndTimeLimitation.MaxTime);
                Assert.AreEqual(day.RestrictionCollection[1].WorkTimeLimitation.StartTime, dto.StudentAvailabilityRestrictions[1].WorkTimeLimitation.MinTime);
                Assert.AreEqual(day.RestrictionCollection[1].WorkTimeLimitation.EndTime, dto.StudentAvailabilityRestrictions[1].WorkTimeLimitation.MaxTime);
                Assert.AreEqual(_person.Name.FirstName, dto.Person.Name);
            }
        }

        [Test]
        public void CanCreateDomainObjectOfDtoNotAvailable()
        {
            
            StudentAvailabilityDayDto dto = new StudentAvailabilityDayDto();
            dto.Person = new PersonDto{Id = _person.Id,Name = _person.Name.ToString()};
			dto.RestrictionDate = new DateOnlyDto { DateTime = _dateOnly.Date };
            dto.NotAvailable = true;
            
            using (_mocks.Record())
            {
                Expect.Call(_personAssembler.DtoToDomainEntity(dto.Person)).Return(_person);
            }
            using (_mocks.Playback())
            {
                IStudentAvailabilityDay day = _target.DtoToDomainEntity(dto);
                Assert.AreEqual(dto.StudentAvailabilityRestrictions.Count, day.RestrictionCollection.Count);
                Assert.IsTrue(day.NotAvailable);
                Assert.AreEqual(_person, day.Person);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanCreateDomainObjectOfDto()
        {
            StudentAvailabilityDayDto dto = new StudentAvailabilityDayDto();
            dto.Person = new PersonDto { Id = _person.Id, Name = _person.Name.ToString() };
			dto.RestrictionDate = new DateOnlyDto { DateTime = _dateOnly.Date };
            StudentAvailabilityRestrictionDto restrictionDto = new StudentAvailabilityRestrictionDto();

            var startLimitation = new StartTimeLimitation(new TimeSpan(5, 0, 0), null);
            var endLimitation = new EndTimeLimitation(null, new TimeSpan(14, 0, 0));
            restrictionDto.StartTimeLimitation = new TimeLimitationDto{MinTime = startLimitation.StartTime};
            restrictionDto.EndTimeLimitation = new TimeLimitationDto {MaxTime = endLimitation.EndTime};
            restrictionDto.WorkTimeLimitation = new TimeLimitationDto
                                                    {MinTime = new TimeSpan(4, 0, 0), MaxTime = new TimeSpan(9, 0, 0)};
            dto.StudentAvailabilityRestrictions.Add(restrictionDto);

            using (_mocks.Record())
            {
                Expect.Call(_personAssembler.DtoToDomainEntity(dto.Person)).Return(_person);
            }
            using (_mocks.Playback())
            {

                IStudentAvailabilityDay day = _target.DtoToDomainEntity(dto);
                Assert.AreEqual(dto.StudentAvailabilityRestrictions.Count, day.RestrictionCollection.Count);
                Assert.AreEqual(day.RestrictionCollection[0].StartTimeLimitation.StartTime,
                                dto.StudentAvailabilityRestrictions[0].StartTimeLimitation.MinTime);
                Assert.AreEqual(day.RestrictionCollection[0].EndTimeLimitation.EndTime,
                                dto.StudentAvailabilityRestrictions[0].EndTimeLimitation.MaxTime);
                Assert.AreEqual(day.RestrictionCollection[0].WorkTimeLimitation.StartTime,
                                dto.StudentAvailabilityRestrictions[0].WorkTimeLimitation.MinTime);
                Assert.AreEqual(day.RestrictionCollection[0].WorkTimeLimitation.EndTime,
                                dto.StudentAvailabilityRestrictions[0].WorkTimeLimitation.MaxTime);
            }
        }
        
    }

    
}
