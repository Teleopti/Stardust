using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ContractScheduleAssemblerTest
    {
        private ContractScheduleAssembler _target;
        private ContractScheduleDto _contractScheduleDto;
        private MockRepository _mocks;
        private IContractScheduleRepository _contractScheduleRep;
        private IContractSchedule _contractScheduleDomain;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _contractScheduleRep = _mocks.StrictMock<IContractScheduleRepository>();
            _target = new ContractScheduleAssembler(_contractScheduleRep);

            // Create domain object
            _contractScheduleDomain = new ContractSchedule("My contractSchedule");

            var week1 = new ContractScheduleWeek();
            week1.Add(DayOfWeek.Monday,true);
            week1.Add(DayOfWeek.Tuesday, true);
            week1.Add(DayOfWeek.Wednesday, true);
            week1.Add(DayOfWeek.Thursday, true);
            week1.Add(DayOfWeek.Friday, false);
            week1.Add(DayOfWeek.Saturday, true);
            week1.Add(DayOfWeek.Sunday, false);
            var week2 = new ContractScheduleWeek();
            week2.Add(DayOfWeek.Monday, true);
            week2.Add(DayOfWeek.Tuesday, true);
            week2.Add(DayOfWeek.Wednesday, true);
            week2.Add(DayOfWeek.Thursday, true);
            week2.Add(DayOfWeek.Friday, false);
            week2.Add(DayOfWeek.Saturday, false);
            week2.Add(DayOfWeek.Sunday, false);
            _contractScheduleDomain.AddContractScheduleWeek(week1);
            _contractScheduleDomain.AddContractScheduleWeek(week2);
            _contractScheduleDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _contractScheduleDto = new ContractScheduleDto { Id = _contractScheduleDomain.Id };
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            ContractScheduleDto contractScheduleDto = _target.DomainEntityToDto(_contractScheduleDomain);

            Assert.AreEqual(_contractScheduleDomain.Id, contractScheduleDto.Id);
            Assert.AreEqual(_contractScheduleDomain.Description.Name, contractScheduleDto.Description);
            Assert.IsFalse(contractScheduleDto.IsDeleted);
        }

		[Test]
		public void ShouldExposeWorkWeekDetailsInDto()
		{
			ContractScheduleDto contractScheduleDto = _target.DomainEntityToDto(_contractScheduleDomain);

			contractScheduleDto.Weeks.Length.Should().Be.EqualTo(2);
			contractScheduleDto.Weeks[0].WeekNumber.Should().Be.EqualTo(1);
			contractScheduleDto.Weeks[0].WorkingDays.Should().Have.SameSequenceAs(new [] {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Saturday });
			contractScheduleDto.Weeks[1].WeekNumber.Should().Be.EqualTo(2);
			contractScheduleDto.Weeks[1].WorkingDays.Should().Have.SameSequenceAs(new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday });
		}

		[Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_contractScheduleRep.Get(_contractScheduleDto.Id.Value)).Return(_contractScheduleDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                IContractSchedule contractSchedule = _target.DtoToDomainEntity(_contractScheduleDto);
                Assert.IsNotNull(contractSchedule);
            }
        }
    }
}
