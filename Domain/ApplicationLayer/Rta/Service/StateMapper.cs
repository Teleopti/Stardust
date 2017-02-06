using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class MappedState
	{
		public Guid StateGroupId { get; set; }
		public string StateGroupName { get; set; }
	}

	public class MappedRule
	{
		public Guid RuleId { get; set; }
		public string RuleName { get; set; }
		public Adherence? Adherence { get; set; }
		public double? StaffingEffect { get; set; }
		public int DisplayColor { get; set; }

		public bool IsAlarm { get; set; }
		public int ThresholdTime { get; set; }
		public int AlarmColor { get; set; }
	}

	public class StateMapper
	{
		private readonly IStateCodeAdder _stateCodeAdder;
		private readonly IMappingReader _reader;
		private string _version;
		private IDictionary<ruleMappingKey, MappedRule> _ruleMappings = new Dictionary<ruleMappingKey, MappedRule>();
		private IDictionary<stateCodeMappingKey, MappedState> _stateCodeMappings = new Dictionary<stateCodeMappingKey, MappedState>();
		private IDictionary<Guid, MappedState> _stateMappings = new Dictionary<Guid, MappedState>();
		private IEnumerable<Guid> _loggedOutStateGroupIds;

		public StateMapper(IStateCodeAdder stateCodeAdder, IMappingReader reader)
		{
			_stateCodeAdder = stateCodeAdder;
			_reader = reader;
		}

		public MappedState StateFor(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			if (stateCode == null)
				return null;
			MappedState match;
			_stateCodeMappings.TryGetValue(new stateCodeMappingKey
			{
				businessUnitId = businessUnitId,
				platformTypeId = platformTypeId,
				stateCode = stateCode
			}, out match);
			if (match != null) return match;
			return _stateCodeAdder.AddUnknownStateCode(businessUnitId, platformTypeId, stateCode, stateDescription);
		}
		
		public MappedState StateFor(Guid? stateGroupId)
		{
			if (stateGroupId == null)
				return null;
			MappedState match;
			_stateMappings.TryGetValue(stateGroupId.Value, out match);
			if (match != null) return match;
			// state group has been removed, but the agent is still in that state...
			return new MappedState
			{
				StateGroupId = stateGroupId.Value,
				StateGroupName = null
			};
		}
		
		public MappedRule RuleFor(Guid businessUnitId, Guid? stateGroupId, Guid? activityId)
		{
			stateGroupId = stateGroupId ?? Guid.Empty;
			var match = queryRule(businessUnitId, stateGroupId, activityId);
			if (activityId != null && match == null)
				match = queryRule(businessUnitId, stateGroupId, null);
			return match;
		}

		private MappedRule queryRule(Guid businessUnitId, Guid? stateGroupId, Guid? activityId)
		{
			MappedRule match;
			_ruleMappings.TryGetValue(new ruleMappingKey
			{
				businessUnitId = businessUnitId,
				stateGroupId = stateGroupId,
				activityId = activityId
			}, out match);
			return match;
		}

		public IEnumerable<Guid> LoggedOutStateGroupIds()
		{
			return _loggedOutStateGroupIds;
		}

		public void RefreshMappingCache(string latestVersion)
		{
			var refresh = latestVersion != _version || _version == null;
			if (!refresh)
				return;

			var mappings = _reader.Read();
			_ruleMappings = mappings
				.Where(m =>
				{
					var illegal = m.StateCode == null && m.StateGroupId != Guid.Empty;
					return !illegal;
				})
				.ToDictionary(x => new ruleMappingKey
				{
					businessUnitId = x.BusinessUnitId,
					stateGroupId = x.StateGroupId,
					activityId = x.ActivityId
				}, m => m.RuleId.HasValue ? new MappedRule
				{
					RuleId = m.RuleId.Value,
					RuleName = m.RuleName,
					Adherence = m.Adherence,
					StaffingEffect = m.StaffingEffect,
					DisplayColor = m.DisplayColor,
					IsAlarm = m.IsAlarm,
					ThresholdTime = m.ThresholdTime,
					AlarmColor = m.AlarmColor
				} : null);
			_stateCodeMappings = mappings
				.GroupBy(x => new stateCodeMappingKey
				{
					businessUnitId = x.BusinessUnitId,
					platformTypeId = x.PlatformTypeId,
					stateCode = x.StateCode
				})
				.ToDictionary(x => x.Key, m =>
				{
					var mapping = m.First();
					return new MappedState
					{
						StateGroupId = mapping.StateGroupId.Value,
						StateGroupName = mapping.StateGroupName
					};
				});
			_stateMappings = mappings
				.Where(x => x.StateGroupId.HasValue)
				.GroupBy(x => x.StateGroupId.Value)
				.ToDictionary(x => x.Key, m =>
				{
					var mapping = m.First();
					return new MappedState
					{
						StateGroupId = m.Key,
						StateGroupName = mapping.StateGroupName
					};
				});
			_loggedOutStateGroupIds = mappings
				.Where(x => x.IsLoggedOut && x.StateGroupId.HasValue)
				.Select(x => x.StateGroupId.Value)
				.Distinct()
				.ToArray();
			_version = latestVersion;
		}

		private class ruleMappingKey
		{
			public Guid businessUnitId;
			public Guid? stateGroupId;
			public Guid? activityId;

			#region

			protected bool Equals(ruleMappingKey other)
			{
				return businessUnitId.Equals(other.businessUnitId)
					&& stateGroupId.Equals(other.stateGroupId)
					&& activityId.Equals(other.activityId);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((ruleMappingKey)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = businessUnitId.GetHashCode();
					hashCode = (hashCode * 397) ^ stateGroupId.GetHashCode();
					hashCode = (hashCode * 397) ^ activityId.GetHashCode();
					return hashCode;
				}
			}

			#endregion
		}

		private class stateCodeMappingKey
		{
			public Guid businessUnitId;
			public Guid? platformTypeId;
			public string stateCode;

			#region 

			protected bool Equals(stateCodeMappingKey other)
			{
				return businessUnitId.Equals(other.businessUnitId)
					&& platformTypeId.Equals(other.platformTypeId)
					&& string.Equals(stateCode, other.stateCode);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((stateCodeMappingKey)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = businessUnitId.GetHashCode();
					hashCode = (hashCode * 397) ^ platformTypeId.GetHashCode();
					hashCode = (hashCode * 397) ^ (stateCode?.GetHashCode() ?? 0);
					return hashCode;
				}
			}

			#endregion
		}
	}
}