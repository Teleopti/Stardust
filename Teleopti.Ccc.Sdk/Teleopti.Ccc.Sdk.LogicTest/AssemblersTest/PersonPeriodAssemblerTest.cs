using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[TestFixture]
	public class PersonPeriodAssemblerTest
	{
		private PersonPeriodAssembler _personPeriodAssembler;
		private IPersonPeriod _personPeriod;
		private IPerson _person;
		private DateOnly _dateOnly;
		private MockRepository _mockRepository;
		private ITeam _team;
		private IPersonContract _personContract;
		private IContract _contract;
		private IPartTimePercentage _partTimePercentage;
		private IContractSchedule _contractSchedule;
		private IExternalLogOn _externalLogOn;
		private IList<IExternalLogOn> _externalLogOnList;
		private IAssembler<IExternalLogOn, ExternalLogOnDto> _externalLogonAssembler;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_dateOnly = new DateOnly(2010,1,1);
			_person = _mockRepository.StrictMock<IPerson>();
			_personPeriod = _mockRepository.StrictMock<IPersonPeriod>();
			_person = _mockRepository.StrictMock<IPerson>();
			_externalLogonAssembler = new ExternalLogOnAssembler();
			_personPeriodAssembler = new PersonPeriodAssembler(_externalLogonAssembler);
			_team = _mockRepository.StrictMock<ITeam>();
			_personContract = _mockRepository.StrictMock<IPersonContract>();
			_contract = ContractFactory.CreateContract("Contract");
			_contract.SetId(Guid.NewGuid());
			_partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("PartTimePercentage");
			_partTimePercentage.SetId(Guid.NewGuid());
			_contractSchedule = ContractScheduleFactory.CreateContractSchedule("ContractSchedule");
			_contractSchedule.SetId(Guid.NewGuid());
			_externalLogOn = _mockRepository.StrictMock<IExternalLogOn>();
			_externalLogOnList = new List<IExternalLogOn> {_externalLogOn};
			
		}

		[Test]
		public void ShouldCreateDtoFromEntity()
		{
			using(_mockRepository.Record())
			{
				Expect.Call(_personPeriod.Parent).Return(_person);
				Expect.Call(_person.Id).Return(Guid.NewGuid());
				Expect.Call(_personPeriod.StartDate).Return(_dateOnly);
				Expect.Call(_team.Id).Return(Guid.NewGuid());
				Expect.Call(_team.Description).Return(new Description("Description"));
				Expect.Call(_team.SiteAndTeam).Return("SiteAndTeam");
				Expect.Call(_personPeriod.Team).Return(_team);
				Expect.Call(_personPeriod.Note).Return("Note");
				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(3);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_personContract.PartTimePercentage).Return(_partTimePercentage);
				Expect.Call(_personContract.ContractSchedule).Return(_contractSchedule);
				Expect.Call(_externalLogOn.AcdLogOnOriginalId).Return("OriginalId");
				Expect.Call(_externalLogOn.AcdLogOnName).Return("LogOnName");
				Expect.Call(_personPeriod.ExternalLogOnCollection).Return(new ReadOnlyCollection<IExternalLogOn>(_externalLogOnList));

			}
			 
			using(_mockRepository.Playback())
			{
				var personPeriodDto = _personPeriodAssembler.DomainEntityToDto(_personPeriod);
				
				Assert.AreNotEqual(Guid.Empty, personPeriodDto.PersonId);
				Assert.IsNotNull(personPeriodDto.StartDate);
				Assert.IsNotNull(personPeriodDto.Team);
				Assert.IsNotNull(personPeriodDto.Note);
				Assert.AreNotEqual(Guid.Empty, personPeriodDto.ContractId);
				Assert.AreNotEqual(Guid.Empty, personPeriodDto.PartTimePercentageId);
				Assert.AreNotEqual(Guid.Empty, personPeriodDto.ContractScheduleId);
				Assert.AreNotEqual(Guid.Empty, personPeriodDto.ContractScheduleId);
				Assert.IsTrue(personPeriodDto.ExternalLogOn.Count == 1);
			}			
		}
	}
}
