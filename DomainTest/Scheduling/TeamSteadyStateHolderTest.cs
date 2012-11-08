using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class TeamSteadyStateHolderTest
	{
		private TeamSteadyStateHolder _teamSteadyStateHolder;
		private IDictionary<Guid, bool> _teamSteadyStates;
		private Guid _guid;
		private MockRepository _mocks;
		private IGroupPerson _groupPerson;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupPerson = _mocks.StrictMock<IGroupPerson>();
			_guid= Guid.NewGuid();
	

			_teamSteadyStates = new Dictionary<Guid, bool> {{_guid, true}};
			_teamSteadyStateHolder = new TeamSteadyStateHolder(_teamSteadyStates);
		}

		[Test]
		public void ShouldReturnTrue()
		{
			using(_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(_guid).Repeat.AtLeastOnce();
			}

			using(_mocks.Playback())
			{
				var res = _teamSteadyStateHolder.IsSteadyState(_groupPerson);
				Assert.IsTrue(res);	
			}			
		}

		[Test]
		public void ShouldReturnFalseWhenNoIdOnGroupPerson()
		{
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(null);
			}

			using (_mocks.Playback())
			{
				var res = _teamSteadyStateHolder.IsSteadyState(_groupPerson);
				Assert.IsFalse(res);
			}				
		}

		[Test]
		public void ShouldReturnFalseWhenGroupPersonNotInDictionary()
		{
			_teamSteadyStates.Clear();

			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(_guid).Repeat.Twice();
			}

			using (_mocks.Playback())
			{
				var res = _teamSteadyStateHolder.IsSteadyState(_groupPerson);
				Assert.IsFalse(res);
			}			
		}

		[Test]
		public void ShouldReturnFalseWhenValueFalseInDictionary()
		{
			_teamSteadyStates = new Dictionary<Guid, bool> { { _guid, false } };
			_teamSteadyStateHolder = new TeamSteadyStateHolder(_teamSteadyStates);

			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(_guid).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var res = _teamSteadyStateHolder.IsSteadyState(_groupPerson);
				Assert.IsFalse(res);
			}			
		}

		[Test]
		public void ShouldSetState()
		{
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(_guid).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_teamSteadyStateHolder.SetSteadyState(_groupPerson, false);
				var res = _teamSteadyStateHolder.IsSteadyState(_groupPerson);
				Assert.IsFalse(res);
			}			
		}

		[Test]
		public void ShouldSkipSettingStateWhenNoIdOnGroupPerson()
		{
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(null);
			}

			using (_mocks.Playback())
			{
				_teamSteadyStateHolder.SetSteadyState(_groupPerson, false);
			}		
		}

		[Test]
		public void ShouldSkipSettingStateWhenGroupPersonNotInDictionary()
		{
			_teamSteadyStates.Clear();

			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(_guid).Repeat.Twice();
			}

			using (_mocks.Playback())
			{
				_teamSteadyStateHolder.SetSteadyState(_groupPerson, false);
			}	
		}
	}
}
