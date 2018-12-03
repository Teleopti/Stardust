using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class MustHaveRestrictionSetterTest
	{
		private MustHaveRestrictionSetter _target;
		private MockRepository _mock;
		private IPreferenceDayRepository _preferenceDayRepository;
		private DateOnly _dateOnly;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_preferenceDayRepository = _mock.StrictMock<IPreferenceDayRepository>();
			_person = _mock.StrictMock<IPerson>();
			_target = new MustHaveRestrictionSetter(_preferenceDayRepository);
		}


		[Test]
		public void CanSetMustHaveWithMaxMustHaveUnderLimit()
		{
			_dateOnly = new DateOnly();
			var virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
			var dateOnlyPeriod = new DateOnlyPeriod();
			var preferenceDay = _mock.StrictMock<IPreferenceDay>();
			var preferenceDays = new List<IPreferenceDay> {preferenceDay};

			var schedulePeriod = _mock.StrictMock<ISchedulePeriod>();
			var restriction = _mock.StrictMock<IPreferenceRestriction>();
		

			using (_mock.Record())
			{
				Expect.Call(_person.VirtualSchedulePeriodOrNext(_dateOnly))
					.Return(virtualSchedulePeriod);
				Expect.Call(virtualSchedulePeriod.DateOnlyPeriod)
					.Return(dateOnlyPeriod);
				Expect.Call(_preferenceDayRepository.Find(dateOnlyPeriod, _person))
					.Return(preferenceDays);

				const int maxHaveLimit = 2;

				Expect.Call(_person.SchedulePeriod(_dateOnly))
					.Return(schedulePeriod);
				Expect.Call(schedulePeriod.MustHavePreference)
					.Return(maxHaveLimit);

				Expect.Call(preferenceDay.RestrictionDate)
					.Return(_dateOnly);
				Expect.Call(preferenceDay.Restriction)
					.Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(restriction.MustHave)
					.Return(true);

				// currentHaveLimit = 1

				Expect.Call(restriction.MustHave = true);
			}
			using(_mock.Playback())
			{
				_target.SetMustHave(_dateOnly, _person, true);
			}
		}

		[Test]
		public void CannotSetMustHaveWithMaxMustHaveEqualLimit()
		{
			_dateOnly = new DateOnly();
			var virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
			var dateOnlyPeriod = new DateOnlyPeriod();
			var preferenceDay = _mock.StrictMock<IPreferenceDay>();
			var preferenceDays = new List<IPreferenceDay> { preferenceDay };

			var schedulePeriod = _mock.StrictMock<ISchedulePeriod>();
			var restriction = _mock.StrictMock<IPreferenceRestriction>();


			using (_mock.Record())
			{
				Expect.Call(_person.VirtualSchedulePeriodOrNext(_dateOnly))
					.Return(virtualSchedulePeriod);
				Expect.Call(virtualSchedulePeriod.DateOnlyPeriod)
					.Return(dateOnlyPeriod);
				Expect.Call(_preferenceDayRepository.Find(dateOnlyPeriod, _person))
					.Return(preferenceDays);

				const int maxHaveLimit = 1;

				Expect.Call(_person.SchedulePeriod(_dateOnly))
					.Return(schedulePeriod);
				Expect.Call(schedulePeriod.MustHavePreference)
					.Return(maxHaveLimit);

				Expect.Call(preferenceDay.RestrictionDate)
					.Return(_dateOnly);
				Expect.Call(preferenceDay.Restriction)
					.Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(restriction.MustHave)
					.Return(true).Repeat.AtLeastOnce();

				// currentHaveLimit = 1

			}
			using (_mock.Playback())
			{
				_target.SetMustHave(_dateOnly, _person, true);
			}
		}

		[Test]
		public void CannotSetMustHaveWithMaxMustHaveOverLimit()
		{
			_dateOnly = new DateOnly();
			var virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
			var dateOnlyPeriod = new DateOnlyPeriod();
			var preferenceDay = _mock.StrictMock<IPreferenceDay>();
			var preferenceDays = new List<IPreferenceDay> { preferenceDay };

			var schedulePeriod = _mock.StrictMock<ISchedulePeriod>();
			var restriction = _mock.StrictMock<IPreferenceRestriction>();


			using (_mock.Record())
			{
				Expect.Call(_person.VirtualSchedulePeriodOrNext(_dateOnly))
					.Return(virtualSchedulePeriod);
				Expect.Call(virtualSchedulePeriod.DateOnlyPeriod)
					.Return(dateOnlyPeriod);
				Expect.Call(_preferenceDayRepository.Find(dateOnlyPeriod, _person))
					.Return(preferenceDays);

				const int maxHaveLimit = 0;

				Expect.Call(_person.SchedulePeriod(_dateOnly))
					.Return(schedulePeriod);
				Expect.Call(schedulePeriod.MustHavePreference)
					.Return(maxHaveLimit);

				Expect.Call(preferenceDay.RestrictionDate)
					.Return(_dateOnly);
				Expect.Call(preferenceDay.Restriction)
					.Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(restriction.MustHave)
					.Return(true).Repeat.AtLeastOnce();

				// currentHaveLimit = 1

			}
			using (_mock.Playback())
			{
				_target.SetMustHave(_dateOnly, _person, true);
			}
		}
	}
}
