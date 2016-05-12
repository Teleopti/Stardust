using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Interfaces.Domain;
using AbsenceViewModel = Teleopti.Ccc.Web.Areas.TeamSchedule.Models.AbsenceViewModel;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture]
	public class AbsenceControllerTest
	{
		private Absence requestableAbsence;
		private Absence notRequestableAbsence;
		private Absence deletedAbsence;
		private AbsenceController target;

		[TestFixtureSetUp]
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

			checkAbsenceExisting(result, availableAbsences);
		}

		[Test]
		public void ShouldGetAllAbsences()
		{
			var allAbsences = new[] { requestableAbsence, notRequestableAbsence, deletedAbsence };
			var result = target.GetAllAbsences().ToList();
			
			checkAbsenceExisting(result, allAbsences);
		}

		private void checkAbsenceExisting(IEnumerable<AbsenceViewModel> resultAbsences, IEnumerable<IAbsence> expectedAbsences)
		{
			var result = resultAbsences.ToList();
			var expected = expectedAbsences.ToList();

			Assert.AreEqual(result.Count, expected.Count);
			foreach (var absence in expected)
			{
				Assert.True(result.Any(x => (
					x.Id == absence.Id.ToString() &&
					x.Name == absence.Description.Name &&
					x.ShortName == absence.Description.ShortName)));
			}
		}
	}
}
