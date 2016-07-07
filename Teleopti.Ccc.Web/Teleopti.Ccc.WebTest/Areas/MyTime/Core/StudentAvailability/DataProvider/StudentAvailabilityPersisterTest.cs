using System.Collections.Generic;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	[TestFixture]
	public class StudentAvailabilityPersisterTest
	{
		[Test]
		public void ShouldAddStudentAvailability()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var form = new StudentAvailabilityDayInput();

			mapper.Stub(x => x.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(form)).Return(studentAvailabilityDay);

			target.Persist(form);

			studentAvailabilityDayRepository.AssertWasCalled(x => x.Add(studentAvailabilityDay));
		}

		[Test]
		public void ShouldReturnFormResultModelOnAdd()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var target = new StudentAvailabilityPersister(MockRepository.GenerateMock<IStudentAvailabilityDayRepository>(), mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var form = new StudentAvailabilityDayInput();
			var viewModel = new StudentAvailabilityDayViewModel();

			mapper.Stub(x => x.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(form)).Return(studentAvailabilityDay);
			mapper.Stub(x => x.Map<IStudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailabilityDay)).Return(viewModel);

			var result = target.Persist(form);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldUpdateExistingStudentAvailability()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var form = new StudentAvailabilityDayInput {Date = DateOnly.Today};
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, mapper, MockRepository.GenerateMock<ILoggedOnUser>());

			studentAvailabilityDayRepository.Stub(x => x.Find(form.Date, null)).Return(new List<IStudentAvailabilityDay> { studentAvailabilityDay });
			mapper.Stub(x => x.Map(form, studentAvailabilityDay)).Return(studentAvailabilityDay);

			target.Persist(form);

			mapper.AssertWasCalled(x => x.Map(form, studentAvailabilityDay));
		}

		[Test]
		public void ShouldReturnFormResultModelOnUpdate()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var form = new StudentAvailabilityDayInput { Date = DateOnly.Today };
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var viewModel = new StudentAvailabilityDayViewModel();

			studentAvailabilityDayRepository.Stub(x => x.Find(form.Date, null)).Return(new List<IStudentAvailabilityDay> { studentAvailabilityDay });
			mapper.Stub(x => x.Map(form, studentAvailabilityDay)).Return(studentAvailabilityDay);
			mapper.Stub(x => x.Map<IStudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailabilityDay)).Return(viewModel);

			var result = target.Persist(form);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldDelete()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			studentAvailabilityDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IStudentAvailabilityDay> { studentAvailabilityDay });

			target.Delete(DateOnly.Today);

			studentAvailabilityDayRepository.AssertWasCalled(x => x.Remove(studentAvailabilityDay));
		}

		[Test]
		public void ShouldReturnEmptyInputResultOnDelete()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			studentAvailabilityDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IStudentAvailabilityDay> { studentAvailabilityDay });

			var result = target.Delete(DateOnly.Today);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
			result.AvailableTimeSpan.Should().Be.Null();
		}

		[Test]
		public void ShouldThrowHttp404OIfStudentAvailabilityDoesNotExists()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			studentAvailabilityDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IStudentAvailabilityDay>());

			var exception = Assert.Throws<HttpException>(() => target.Delete(DateOnly.Today));
			exception.GetHttpCode().Should().Be(404);
		}

	}
}
