using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class AlarmMapperTest
	{
		private IAlarmMapper _target;
		private IDatabaseReader _databaseReader;
		private IMbCacheFactory _cacheFactory;

		private Guid _platFormTypeId, _activityId, _stateId, _businessUnitId, _stateGroupId, _personId;
		private string _stateCode;

		private ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>> _stateGroupDictionary;
		private ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> _alarmDictionary;
		private RtaAlarmLight _rtaAlarm;
		private RtaStateGroupLight _rtaStateGroup;
	    private IDatabaseWriter _databaseWriter;

	    [SetUp]
		public void Setup()
		{
			_databaseReader = MockRepository.GenerateStrictMock<IDatabaseReader>();
			_databaseWriter = MockRepository.GenerateStrictMock<IDatabaseWriter>();
			_cacheFactory = MockRepository.GenerateStrictMock<IMbCacheFactory>();

			_target = new AlarmMapper(_databaseReader, _databaseWriter, _cacheFactory);

			_platFormTypeId = Guid.NewGuid();
			_activityId = Guid.NewGuid();
			_stateId = Guid.NewGuid();
			_businessUnitId = Guid.NewGuid();
			_stateGroupId = Guid.NewGuid();
			_personId = Guid.NewGuid();
			_stateCode = "AUX3";

			_rtaStateGroup = new RtaStateGroupLight
				{
					BusinessUnitId = _businessUnitId,
					PlatformTypeId = _platFormTypeId,
					StateCode = _stateCode,
					StateGroupId = _stateGroupId,
					StateGroupName = "StateGroupName",
					StateId = _stateId,
					StateName = "Ready"
				};
			_stateGroupDictionary = new ConcurrentDictionary<Tuple<string,Guid,Guid>, List<RtaStateGroupLight>>();
			_stateGroupDictionary.TryAdd(new Tuple<string, Guid, Guid>(_stateCode,_platFormTypeId,_businessUnitId), new List<RtaStateGroupLight>{_rtaStateGroup});

			_rtaAlarm = new RtaAlarmLight
			{
				ActivityId = _activityId,
				StateGroupId = _stateGroupId,
				StateGroupName = "StateGroupName",
				AlarmTypeId = Guid.NewGuid(),
				BusinessUnit = _businessUnitId
			};
			_alarmDictionary = new ConcurrentDictionary<Tuple<Guid,Guid,Guid>, List<RtaAlarmLight>>();
			_alarmDictionary.TryAdd(new Tuple<Guid,Guid,Guid>(_activityId,_stateGroupId,_businessUnitId), new List<RtaAlarmLight> {_rtaAlarm});
		}

		[Test]
		public void GetAlarm_ReturnValidAlarm()
		{
			_databaseReader.Expect(d => d.ActivityAlarms()).Return(_alarmDictionary);
			var result = _target.GetAlarm(_activityId, _stateGroupId, _businessUnitId);
			result.Should().Be.EqualTo(_rtaAlarm);
		}

		[Test]
		public void GetAlarm_NoMatchingActivity_ReturnAlarmForNoActivity()
		{
			var rtaAlarmForNoActivity = new RtaAlarmLight
			{
				ActivityId = Guid.Empty,
				StateGroupId = _stateGroupId,
				BusinessUnit = _businessUnitId
			};
			_alarmDictionary.TryAdd(new Tuple<Guid, Guid, Guid>(Guid.Empty,_stateGroupId,_businessUnitId), new List<RtaAlarmLight>{rtaAlarmForNoActivity});

			_databaseReader.Expect(d => d.ActivityAlarms()).Return(_alarmDictionary);

			var result = _target.GetAlarm(Guid.NewGuid(), _stateGroupId, _businessUnitId);
			result.Should().Be.EqualTo(rtaAlarmForNoActivity);
		}

		[Test]
		public void GetAlarm_NoMatchingStateGroup_ReturnAlarmForNoStateGroup()
		{
			var alarmForNoStateGroup = new List<RtaAlarmLight>
				{
					new RtaAlarmLight
						{
							StateGroupId = Guid.Empty,
							ActivityId = _activityId,
							BusinessUnit = _businessUnitId
						}
				};
			_alarmDictionary.AddOrUpdate(new Tuple<Guid, Guid, Guid>(_activityId,Guid.Empty,_businessUnitId), alarmForNoStateGroup, (guid, list) => alarmForNoStateGroup);

			_databaseReader.Expect(d => d.ActivityAlarms()).Return(_alarmDictionary);
			var result = _target.GetAlarm(_activityId, Guid.Empty, _businessUnitId);
			result.Should().Be.EqualTo(alarmForNoStateGroup.First());
		}

		[Test]
		public void GetAlarm_NoMatchingStateGroup_NoMatchingEmptyStateGroupAlarm_ReturnNull()
		{
			_databaseReader.Expect(d => d.ActivityAlarms()).Return(_alarmDictionary);
			var result = _target.GetAlarm(_activityId, Guid.NewGuid(), _businessUnitId);
			result.Should().Be.Null();
		}

		[Test]
		public void GetStateGroup_ReturnValidStateGroup()
		{
			_databaseReader.Expect(d => d.StateGroups()).Return(_stateGroupDictionary);
			var result = _target.GetStateGroup(_stateCode, _platFormTypeId, _businessUnitId);
			result.Should().Be.EqualTo(_rtaStateGroup);
		}

		[Test]
		public void GetStateGroup_NoMatchForStateCodeInBusinessUnit_ReturnTheNewState()
        {
            var fictionarlDefaultStateGroup = new RtaStateGroupLight
            {
                BusinessUnitId = Guid.Empty,
                PlatformTypeId = _platFormTypeId,
                StateCode = _stateCode,
                StateGroupId = Guid.NewGuid(),
                StateGroupName = "a_new_state_group_name",
                StateName = _stateCode,
                StateId = Guid.NewGuid()
            };
            var stateDictionary = new ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>();
            stateDictionary.TryAdd(new Tuple<string, Guid, Guid>(_stateCode, _platFormTypeId, Guid.Empty), new List<RtaStateGroupLight> { fictionarlDefaultStateGroup });

			_databaseReader.Expect(d => d.StateGroups()).Return(_stateGroupDictionary);
            _databaseWriter.Expect(d => d.AddAndGetNewRtaState(_stateCode, _platFormTypeId, Guid.Empty)).Return(fictionarlDefaultStateGroup);
            _cacheFactory.Expect(c => c.Invalidate(_databaseReader, d => d.StateGroups(), false)).IgnoreArguments();
            _databaseReader.Expect(d => d.StateGroups()).Return(stateDictionary);

			var result = _target.GetStateGroup(_stateCode, _platFormTypeId, Guid.Empty);
			result.BusinessUnitId.Should().Be.EqualTo(Guid.Empty);
			result.StateGroupId.Should().Be.EqualTo(fictionarlDefaultStateGroup.StateGroupId);
		}

		[Test]
		public void GetStateGroup_StateCodeNotFound_AddStateGroupToDb_ReturnTheNewState()
		{
			const string newStateCode = "some_new_state_code";
			var fictionarlDefaultStateGroup = new RtaStateGroupLight
				{
					BusinessUnitId = _businessUnitId,
					PlatformTypeId = _platFormTypeId,
					StateCode = newStateCode,
					StateGroupId = Guid.NewGuid(),
					StateGroupName = "a_new_state_group_name",
					StateName = newStateCode,
					StateId = Guid.NewGuid()
				};
			var stateDictionary = new ConcurrentDictionary<Tuple<string,Guid,Guid>, List<RtaStateGroupLight>>();
			stateDictionary.TryAdd(new Tuple<string, Guid, Guid>(newStateCode,_platFormTypeId,_businessUnitId), new List<RtaStateGroupLight> {fictionarlDefaultStateGroup});

			_databaseReader.Expect(d => d.StateGroups()).Return(new ConcurrentDictionary<Tuple<string,Guid,Guid>, List<RtaStateGroupLight>>());
			_databaseWriter.Expect(d => d.AddAndGetNewRtaState(newStateCode, _platFormTypeId,_businessUnitId)).Return(fictionarlDefaultStateGroup);
			_cacheFactory.Expect(c => c.Invalidate(_databaseReader, d => d.StateGroups(), false)).IgnoreArguments();
			_databaseReader.Expect(d => d.StateGroups()).Return(stateDictionary);

			var result = _target.GetStateGroup(newStateCode, _platFormTypeId, _businessUnitId);
			result.BusinessUnitId.Should().Be.EqualTo(fictionarlDefaultStateGroup.BusinessUnitId);
			result.PlatformTypeId.Should().Be.EqualTo(fictionarlDefaultStateGroup.PlatformTypeId);
			result.StateCode.Should().Be.EqualTo(fictionarlDefaultStateGroup.StateCode);
			result.StateName.Should().Be.EqualTo(fictionarlDefaultStateGroup.StateName);
			result.StateGroupId.Should().Be.EqualTo(fictionarlDefaultStateGroup.StateGroupId);
			result.StateGroupName.Should().Be.EqualTo(fictionarlDefaultStateGroup.StateGroupName);
			result.StateId.Should().Be.EqualTo(fictionarlDefaultStateGroup.StateId);
			
			_databaseWriter.AssertWasCalled(d => d.AddAndGetNewRtaState(newStateCode, _platFormTypeId,_businessUnitId));
			_cacheFactory.AssertWasCalled(c => c.Invalidate(_databaseReader, d => d.StateGroups(), false), a => a.IgnoreArguments());
		}

		[Test]
		public void IsAgentLoggedOut_Yes_ReturnTrue()
		{
			var stateGroup = new RtaStateGroupLight
				{
					BusinessUnitId = _businessUnitId,
					PlatformTypeId = _platFormTypeId,
					StateGroupId = _stateGroupId,
					IsLogOutState = true
				};
			var stateDictionary = new ConcurrentDictionary<Tuple<string,Guid,Guid>, List<RtaStateGroupLight>>();
			stateDictionary.TryAdd(new Tuple<string, Guid, Guid>("stateCode",_platFormTypeId,_businessUnitId), new List<RtaStateGroupLight> { stateGroup });

			_databaseReader.Expect(d => d.StateGroups()).Return(stateDictionary);
			
			var result = _target.IsAgentLoggedOut(_personId, "stateCode", _platFormTypeId, _businessUnitId);
			result.Should().Be.True();
		}

		[Test]
		public void IsAgentLoggedOut_NoStateGroupFound_ReturnFalse()
		{
			var stateGroup = new RtaStateGroupLight();
            var stateDictionary = new ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>();
            stateDictionary.TryAdd(new Tuple<string, Guid, Guid>("stateCode", _platFormTypeId, _businessUnitId), new List<RtaStateGroupLight> { stateGroup });

			_databaseReader.Expect(d => d.StateGroups()).Return(stateDictionary);

			var result = _target.IsAgentLoggedOut(_personId, "", _platFormTypeId, _businessUnitId);
			result.Should().Be.False();
		}

		[Test]
		public void IsAgentLoggedOut_NoAlarm_ShouldNotCrashRta()
		{
			var stateGroup = new RtaStateGroupLight
			{
				BusinessUnitId = _businessUnitId,
				PlatformTypeId = _platFormTypeId,
				StateGroupId = _stateGroupId
			};
            var stateDictionary = new ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>();
			stateDictionary.TryAdd(new Tuple<string, Guid, Guid>("stateCode",_platFormTypeId,_businessUnitId), new List<RtaStateGroupLight> { stateGroup });

			_databaseReader.Expect(d => d.StateGroups()).Return(stateDictionary);
			_databaseReader.Expect(d => d.ActivityAlarms()).Return(new ConcurrentDictionary<Tuple<Guid,Guid,Guid>, List<RtaAlarmLight>>());

			var result = _target.IsAgentLoggedOut(_personId, "stateCode", _platFormTypeId, _businessUnitId);
			result.Should().Be.False();
		}
	}
}
