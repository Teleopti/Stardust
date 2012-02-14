using System.Collections.Generic;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferencePersisterTest
	{
		[Test]
		public void ShouldAddPreference()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var input = new PreferenceDayInput();

			mapper.Stub(x => x.Map<PreferenceDayInput, IPreferenceDay>(input)).Return(preferenceDay);

			target.Persist(input);

			preferenceDayRepository.AssertWasCalled(x => x.Add(preferenceDay));
			mapper.AssertWasNotCalled(x => x.Map<PreferenceDayInput, IPreferenceDay>(input, null));
		}

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var target = new PreferencePersister(MockRepository.GenerateMock<IPreferenceDayRepository>(), mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var input = new PreferenceDayInput();
			var inputResult = new PreferenceDayInputResult();

			mapper.Stub(x => x.Map<PreferenceDayInput, IPreferenceDay>(input)).Return(preferenceDay);
			mapper.Stub(x => x.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay)).Return(inputResult);

			var result = target.Persist(input);

			result.Should().Be.SameInstanceAs(inputResult);
		}

		[Test]
		public void ShouldUpdateExistingPreference()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);

			target.Persist(input);

			mapper.AssertWasCalled(x => x.Map(input, preferenceDay));
			preferenceDayRepository.AssertWasNotCalled(x => x.Add(preferenceDay));
		}


		[Test]
		public void ShouldReturnFormResultModelOnUpdate()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var target = new PreferencePersister(preferenceDayRepository, mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var viewModel = new PreferenceDayInputResult();

			preferenceDayRepository.Stub(x => x.Find(input.Date, null)).Return(new List<IPreferenceDay> { preferenceDay });
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);
			mapper.Stub(x => x.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay)).Return(viewModel);

			var result = target.Persist(input);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldDelete()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var target = new PreferencePersister(preferenceDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IPreferenceDay> { preferenceDay });

			target.Delete(DateOnly.Today);

			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay));
		}

		[Test]
		public void ShouldReturnEmptyInputResultOnDelete()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var target = new PreferencePersister(preferenceDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IPreferenceDay> { preferenceDay });

			var result = target.Delete(DateOnly.Today);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
			result.PreferenceRestriction.Should().Be.Null();
		}

		[Test]
		public void ShouldThrowHttp404OIfPreferenceDoesNotExists()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var target = new PreferencePersister(preferenceDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IPreferenceDay>());

			var exception = Assert.Throws<HttpException>(() => target.Delete(DateOnly.Today));
			exception.GetHttpCode().Should().Be(404);
		}

	}
}
