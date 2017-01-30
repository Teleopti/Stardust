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
		private IDictionary<ruleMappingKey, MappedRule> _ruleMappingDictionary = new Dictionary<ruleMappingKey, MappedRule>();
		private IDictionary<stateMappingKey, MappedState> _stateMappingDictionary = new Dictionary<stateMappingKey, MappedState>();

		public StateMapper(IStateCodeAdder stateCodeAdder, IMappingReader reader)
		{
			_stateCodeAdder = stateCodeAdder;
			_reader = reader;
		}

		public MappedRule RuleFor(Guid businessUnitId, Guid platformTypeId, string stateCode, Guid? activityId)
		{
			var match = queryRule(businessUnitId, platformTypeId, stateCode, activityId);
			if (activityId != null && match == null)
				match = queryRule(businessUnitId, platformTypeId, stateCode, null);
			return match;
		}

		private MappedRule queryRule(Guid businessUnitId, Guid platformTypeId, string stateCode, Guid? activityId)
		{
			var key = new ruleMappingKey
			{
				businessUnitId = businessUnitId,
				platformTypeId = platformTypeId,
				stateCode = stateCode,
				activityId = activityId
			};
			MappedRule result;
			_ruleMappingDictionary.TryGetValue(key, out result);
			return result;
		}

		public MappedState StateFor(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			if (stateCode == null)
				return null;
			var match = queryState(businessUnitId, platformTypeId, stateCode);
			if (match != null) return match;
			return _stateCodeAdder.AddUnknownStateCode(businessUnitId, platformTypeId, stateCode, stateDescription);
		}

		private MappedState queryState(Guid businessUnitId, Guid platformTypeId, string stateCode)
		{
			var key = new stateMappingKey
			{
				businessUnitId = businessUnitId,
				platformTypeId = platformTypeId,
				stateCode = stateCode
			};
			MappedState result;
			_stateMappingDictionary.TryGetValue(key, out result);
			return result;
		}

		public void RefreshMappingCache(string latestVersion)
		{
			var refresh = latestVersion != _version || _version == null;
			if (!refresh)
				return;

			var mappings = _reader.Read();
			_ruleMappingDictionary = mappings
				.Where(m =>
				{
					var illegal = m.StateCode == null && m.StateGroupId != Guid.Empty;
					return !illegal;
				})
				.ToDictionary(x => new ruleMappingKey
				{
					businessUnitId = x.BusinessUnitId,
					platformTypeId = x.PlatformTypeId,
					stateCode = x.StateCode,
					activityId = x.ActivityId
				}, m => new MappedRule
				{
					RuleId = m.RuleId,
					RuleName = m.RuleName,
					Adherence = m.Adherence,
					StaffingEffect = m.StaffingEffect,
					DisplayColor = m.DisplayColor,
					IsAlarm = m.IsAlarm,
					ThresholdTime = m.ThresholdTime,
					AlarmColor = m.AlarmColor
				});
			_stateMappingDictionary = mappings
				.GroupBy(x => new stateMappingKey
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
						StateGroupId = mapping.StateGroupId,
						StateGroupName = mapping.StateGroupName
					};
				});
			_version = latestVersion;
		}

		private class ruleMappingKey
		{
			public Guid businessUnitId;
			public Guid platformTypeId;
			public string stateCode;
			public Guid? activityId;

			#region

			protected bool Equals(ruleMappingKey other)
			{
				return businessUnitId.Equals(other.businessUnitId) 
					&& platformTypeId.Equals(other.platformTypeId) 
					&& string.Equals(stateCode, other.stateCode) 
					&& activityId.Equals(other.activityId);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((ruleMappingKey) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = businessUnitId.GetHashCode();
					hashCode = (hashCode*397) ^ platformTypeId.GetHashCode();
					hashCode = (hashCode*397) ^ (stateCode?.GetHashCode() ?? 0);
					hashCode = (hashCode*397) ^ activityId.GetHashCode();
					return hashCode;
				}
			}

			#endregion
		}

		private class stateMappingKey
		{
			public Guid businessUnitId;
			public Guid platformTypeId;
			public string stateCode;

			#region 

			protected bool Equals(stateMappingKey other)
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
				return Equals((stateMappingKey) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = businessUnitId.GetHashCode();
					hashCode = (hashCode*397) ^ platformTypeId.GetHashCode();
					hashCode = (hashCode*397) ^ (stateCode?.GetHashCode() ?? 0);
					return hashCode;
				}
			}

			#endregion
		}
	}
}