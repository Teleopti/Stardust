using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class PersonDataFactory
	{
		private readonly Name _name;
		private readonly Action<Action<IUnitOfWork>> _unitOfWorkAction;
		private readonly IList<IUserSetup> _userSetups = new List<IUserSetup>();
		private readonly IList<IUserDataSetup> _userDataSetups = new List<IUserDataSetup>();
		private IUserSetup _cultureSetup = new SwedishCulture();
		private IUserSetup _timeZoneSetup = new UtcTimeZone();

		public PersonDataFactory(Name name, Action<Action<IUnitOfWork>> unitOfWorkAction)
		{
			_name = name;
			_unitOfWorkAction = unitOfWorkAction;
		}

		public void Setup(IUserSetup setup)
		{
			_userSetups.Add(setup);
		}

		public void Setup(IUserDataSetup setup)
		{
			_userDataSetups.Add(setup);
		}

		public void SetupCulture(IUserSetup setup)
		{
			_cultureSetup = setup;
		}

		public void SetupTimeZone(IUserSetup setup)
		{
			_timeZoneSetup = setup;
		}

		public void ReplaceSetupByType<T>(IUserSetup setup)
		{
			ReplaceSetupByType<T, IUserSetup>(_userSetups, setup);
		}

		private void ReplaceSetupByType<TReplace, TSetup>(IList<TSetup> setups, TSetup setup)
		{
			var existing = setups
				.Where(s => typeof(TReplace).IsAssignableFrom(s.GetType()))
				.Select((s, i) => new { s, i }).Single();
			setups.Remove(existing.s);
			setups.Insert(existing.i, setup);
		}

		public IEnumerable<object> Setups { get { return _userSetups.Cast<object>().Union(_userDataSetups); } }

		public void Persist()
		{
			CultureInfo cultureInfo = null;

			var person = PersonFactory.CreatePerson(_name);
			person.Name = _name;

			_unitOfWorkAction(uow =>
				{
					_cultureSetup.Apply(uow, person, null);
					cultureInfo = person.PermissionInformation.Culture();
					_timeZoneSetup.Apply(uow, person, cultureInfo);
					_userSetups.ForEach(s => s.Apply(uow, person, cultureInfo));
					new PersonRepository(uow).Add(person);
				});

			_unitOfWorkAction(uow => _userDataSetups.ForEach(s => s.Apply(uow, person, cultureInfo)));

			Culture = cultureInfo;
			Person = person;
			if (Person.ApplicationAuthenticationInfo != null)
				LogOnName = Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
		}

		public string LogOnName { get; private set; }
		public IPerson Person { get; private set; }
		public CultureInfo Culture { get; private set; }
	}
}