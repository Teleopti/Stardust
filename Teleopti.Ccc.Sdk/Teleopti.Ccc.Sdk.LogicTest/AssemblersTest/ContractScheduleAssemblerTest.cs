using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ContractScheduleAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var contractScheduleRep = new FakeContractScheduleRepository();
			var target = new ContractScheduleAssembler(contractScheduleRep);

			var contractScheduleDomain = createContractSchedule();
			contractScheduleRep.Has(contractScheduleDomain);

	        var contractScheduleDto = target.DomainEntityToDto(contractScheduleDomain);

            Assert.AreEqual(contractScheduleDomain.Id, contractScheduleDto.Id);
            Assert.AreEqual(contractScheduleDomain.Description.Name, contractScheduleDto.Description);
            Assert.IsFalse(contractScheduleDto.IsDeleted);
        }

	    private static ContractSchedule createContractSchedule()
	    {
		    var contractScheduleDomain = new ContractSchedule("My contractSchedule").WithId();

		    var week1 = new ContractScheduleWeek();
		    week1.Add(DayOfWeek.Monday, true);
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
		    contractScheduleDomain.AddContractScheduleWeek(week1);
		    contractScheduleDomain.AddContractScheduleWeek(week2);
		    return contractScheduleDomain;
	    }

	    [Test]
		public void ShouldExposeWorkWeekDetailsInDto()
		{
			var contractScheduleRep = new FakeContractScheduleRepository();
			var target = new ContractScheduleAssembler(contractScheduleRep);

			var contractScheduleDomain = createContractSchedule();
			contractScheduleRep.Has(contractScheduleDomain);
			
			var contractScheduleDto = target.DomainEntityToDto(contractScheduleDomain);

			contractScheduleDto.Weeks.Length.Should().Be.EqualTo(2);
			contractScheduleDto.Weeks[0].WeekNumber.Should().Be.EqualTo(1);
			contractScheduleDto.Weeks[0].WorkingDays.Should().Have.SameSequenceAs(new [] {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Saturday });
			contractScheduleDto.Weeks[1].WeekNumber.Should().Be.EqualTo(2);
			contractScheduleDto.Weeks[1].WorkingDays.Should().Have.SameSequenceAs(new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday });
		}

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var contractScheduleRep = new FakeContractScheduleRepository();
		    var target = new ContractScheduleAssembler(contractScheduleRep);

		    var contractScheduleDomain = createContractSchedule();
		    contractScheduleRep.Has(contractScheduleDomain);

		    var contractScheduleDto = new ContractScheduleDto {Id = contractScheduleDomain.Id};

		    IContractSchedule contractSchedule = target.DtoToDomainEntity(contractScheduleDto);
		    Assert.IsNotNull(contractSchedule);
	    }
    }
}
