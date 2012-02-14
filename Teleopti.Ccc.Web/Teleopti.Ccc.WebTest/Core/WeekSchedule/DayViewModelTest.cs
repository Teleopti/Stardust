using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule
{
	[TestFixture]
	public class DayViewModelTest
	{
		private DayViewModel target;

		[SetUp]
		public void Setup()
		{
			target = new DayViewModel();
		}

		[Test]
		public void ShouldReportNoNoteWhenNull()
		{
			target.HasNote.Should().Be.False();
		}

		[Test]
		public void ShouldReportNoNoteWhenNoteIsEmpty()
		{
			target.Note = new NoteViewModel();
			target.HasNote.Should().Be.False();
		}

		[Test]
		public void ShouldReportHasNoteWhenNoteHasMessage()
		{
			target.Note = new NoteViewModel { Message = "My Note" };
			target.HasNote.Should().Be.True();
		}
	}
}