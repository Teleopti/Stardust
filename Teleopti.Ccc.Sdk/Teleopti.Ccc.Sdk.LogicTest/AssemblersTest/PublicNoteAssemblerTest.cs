using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PublicNoteAssemblerTest
    {
        private PublicNoteAssembler _target;
        private IPublicNote _publicNoteDomain;
        private PublicNoteDto _publicNoteDto;
        private MockRepository _mocks;
        private IPublicNoteRepository _repository;
        private IAssembler<IPerson, PersonDto> _personAssembler;
        IPerson _personDomain;

        [SetUp]
        public void Setup()
        {
            _personDomain = new Person {Name = new Name("Bosse", "Bäver")};
            _personDomain.SetId(Guid.NewGuid());
            var date = new DateOnly(2011, 1, 12);

            _publicNoteDomain = new PublicNote(_personDomain, date, new Scenario("Default scenario"), "Work harder!");
            _publicNoteDomain.SetId(Guid.NewGuid());

            _publicNoteDto = new PublicNoteDto{Id = _publicNoteDomain.Id};

            _mocks = new MockRepository();
            _repository = _mocks.StrictMock<IPublicNoteRepository>();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();

            _target = new PublicNoteAssembler(_repository, _personAssembler);
        }

        [Test]
        public void ShouldTransformDomainObjectToDto()
        {
            using (_mocks.Record())
            {
                Expect.Call(_personAssembler.DomainEntityToDto(_personDomain)).Return(new PersonDto
                                                                                          {
                                                                                              Id = _personDomain.Id,
                                                                                              Name =_personDomain.Name.ToString()
                                                                                          });
            }

            using (_mocks.Playback())
            {
                _publicNoteDto = _target.DomainEntityToDto(_publicNoteDomain);
                Assert.AreEqual(_publicNoteDomain.Id, _publicNoteDto.Id);
                Assert.AreEqual(_publicNoteDomain.Person.Id, _publicNoteDto.Person.Id);
                Assert.AreEqual(_publicNoteDomain.Person.Name.ToString(), _publicNoteDto.Person.Name);
                Assert.AreEqual(_publicNoteDomain.ScheduleNote, _publicNoteDto.ScheduleNote);
                Assert.AreEqual(_publicNoteDomain.NoteDate.Date, _publicNoteDto.Date.DateTime);
            }
        }

        [Test]
        public void ShouldTransformDtoObjectToDomain()
        {
            Assert.IsTrue(_publicNoteDto.Id.HasValue);
            using (_mocks.Record())
            {
                Expect.Call(_repository.Get(_publicNoteDto.Id.Value)).Return(_publicNoteDomain);
            }

            using (_mocks.Playback())
            {
                IPublicNote domainEntity = _target.DtoToDomainEntity(_publicNoteDto);
                Assert.IsNotNull(domainEntity);
            }
        }
    }
}