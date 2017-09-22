using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class AbsenceAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
	        var absenceRep = new FakeAbsenceRepository();
			var target = new AbsenceAssembler(absenceRep);
			
			var absenceDomain = new Absence
			{
				Description = new Description("name", "nm"),
				Priority = 1,
				InContractTime = true,
				DisplayColor = Color.Blue,
				PayrollCode = "OT",
				Tracker = Tracker.CreateOvertimeTracker(),
				InPaidTime = true,
				InWorkTime = true
			}.WithId();
			absenceRep.Add(absenceDomain);
			
			AbsenceDto absenceDto = target.DomainEntityToDto(absenceDomain);

            Assert.AreEqual(absenceDomain.Id, absenceDto.Id);
            Assert.AreEqual(absenceDomain.Description.Name, absenceDto.Name);
            Assert.AreEqual(absenceDomain.Description.ShortName, absenceDto.ShortName);
            Assert.AreEqual(absenceDomain.Priority, absenceDto.Priority);
            Assert.AreEqual(absenceDomain.InContractTime, absenceDto.InContractTime);
            Assert.AreEqual(absenceDomain.DisplayColor.ToArgb(), absenceDto.DisplayColor.ToColor().ToArgb());
            Assert.AreEqual(absenceDomain.PayrollCode, absenceDto.PayrollCode);
            Assert.AreEqual(absenceDomain.Tracker != null, absenceDto.IsTrackable);
            Assert.IsFalse(absenceDto.IsDeleted);
            Assert.IsTrue(absenceDto.InPaidTime);
            Assert.IsTrue(absenceDto.InWorkTime);
        }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var absenceRep = new FakeAbsenceRepository();
		    var target = new AbsenceAssembler(absenceRep);

		    var absenceDomain = new Absence
		    {
			    Description = new Description("name", "nm"),
			    Priority = 1,
			    InContractTime = true,
			    DisplayColor = Color.Blue,
			    PayrollCode = "OT",
			    Tracker = Tracker.CreateOvertimeTracker(),
			    InPaidTime = true,
			    InWorkTime = true
		    }.WithId();
		    absenceRep.Add(absenceDomain);

		    IAbsence foundAbsenceDomain = target.DtoToDomainEntity(new AbsenceDto {Id = absenceDomain.Id.GetValueOrDefault()});
		    Assert.IsNotNull(foundAbsenceDomain);
	    }
    }
}
