using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class AbsenceAssemblerTest
    {
        private AbsenceAssembler _target;
        private IAbsence _absenceDomain;
        private AbsenceDto _absenceDto;
        private MockRepository _mocks;
        private IAbsenceRepository _absenceRep;

        [SetUp]
        public void Setup()
        {
            _mocks=new MockRepository();
            _absenceRep = _mocks.StrictMock<IAbsenceRepository>();
            _target = new AbsenceAssembler(_absenceRep);

            // Create domain object
            _absenceDomain = new Absence
                                 {
                                     Description = new Description("name", "nm"),
                                     Priority = 1,
                                     InContractTime = true,
                                     DisplayColor = Color.Blue,
                                     PayrollCode = "OT",
                                     Tracker = Tracker.CreateOvertimeTracker(),
                                     InPaidTime = true,
                                     InWorkTime = true
                                 };
            _absenceDomain.SetId(Guid.NewGuid());

            // Create Dto object
            _absenceDto = new AbsenceDto {Id = _absenceDomain.Id};
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            AbsenceDto absenceDto = _target.DomainEntityToDto(_absenceDomain);

            Assert.AreEqual(_absenceDomain.Id, absenceDto.Id);
            Assert.AreEqual(_absenceDomain.Description.Name, absenceDto.Name);
            Assert.AreEqual(_absenceDomain.Description.ShortName, absenceDto.ShortName);
            Assert.AreEqual(_absenceDomain.Priority, absenceDto.Priority);
            Assert.AreEqual(_absenceDomain.InContractTime, absenceDto.InContractTime);
            Assert.AreEqual(_absenceDomain.DisplayColor.ToArgb(), absenceDto.DisplayColor.ToColor().ToArgb());
            Assert.AreEqual(_absenceDomain.PayrollCode, absenceDto.PayrollCode);
            Assert.AreEqual(_absenceDomain.Tracker != null, absenceDto.IsTrackable);
            Assert.IsFalse(absenceDto.IsDeleted);
            Assert.IsTrue(absenceDto.InPaidTime);
            Assert.IsTrue(absenceDto.InWorkTime);
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            using (_mocks.Record())
            {
                Expect.Call(_absenceRep.Get(_absenceDto.Id.Value)).Return(_absenceDomain).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                IAbsence absenceDomain = _target.DtoToDomainEntity(_absenceDto);
                Assert.IsNotNull(absenceDomain);
            }

            //Assert.AreEqual(_absenceDto.Id, absenceDomain.Id);
            //Assert.AreEqual(_absenceDto.Name, absenceDomain.Description.Name);
            //Assert.AreEqual(_absenceDto.ShortName, absenceDomain.Description.ShortName);
            //Assert.AreEqual(_absenceDto.Priority, absenceDomain.Priority);
            //Assert.AreEqual(_absenceDto.InContractTime, absenceDomain.InContractTime);
            //Assert.AreEqual(_absenceDto.DisplayColor, new ColorDto(absenceDomain.DisplayColor));
            //Assert.AreEqual(_absenceDto.PayrollCode, absenceDomain.PayrollCode);
            //Assert.AreEqual(_absenceDto.IsTrackable, absenceDomain.Tracker != null);
        }
    }
}
