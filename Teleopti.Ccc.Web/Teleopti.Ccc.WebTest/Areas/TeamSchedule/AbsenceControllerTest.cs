using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture]
	public class AbsenceControllerTest
	{
		private Absence requestableAbsence;
		private Absence notRequestableAbsence;
		private Absence deletedAbsence;
		private AbsenceController target;

		[OneTimeSetUp]
		public void Setup()
		{
			requestableAbsence = new Absence
			{
				Description = new Description("Requestable"),
				Requestable = true
			}.WithId();
			notRequestableAbsence = new Absence
			{
				Description = new Description("Unrequestable"),
				Requestable = false
			}.WithId();
			deletedAbsence = new Absence
			{
				Description = new Description("Deleted"),
				Requestable = true
			}.WithId();
			deletedAbsence.SetDeleted();

			var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(requestableAbsence);
			absenceRepository.Add(notRequestableAbsence);
			absenceRepository.Add(deletedAbsence);
			
			target = new AbsenceController(absenceRepository);
		}

		[Test]
		public void ShouldGetAllAvailableAbsences()
		{
			var availableAbsences = new[] { requestableAbsence, notRequestableAbsence };
			var result = target.GetAvailableAbsences().ToList();

			Assert.AreEqual(result.Count, availableAbsences.Length);
			foreach (var absence in availableAbsences)
			{
				Assert.True(result.Any(x => (
					x.Id == absence.Id.ToString() &&
					x.Name == absence.Description.Name &&
					x.ShortName == absence.Description.ShortName)));
			}
		}

		[Test]
		public void ShouldGetRequestableAbsences()
		{
			var result = target.GetRequestableAbsences().ToList();

			Assert.AreEqual(result.Count, 1);

			var resultAbsence = result.Single();
			Assert.True(
				resultAbsence.Id == requestableAbsence.Id.ToString() &&
				resultAbsence.Name == requestableAbsence.Description.Name &&
				resultAbsence.ShortName == requestableAbsence.Description.ShortName);
		}
	}
}
