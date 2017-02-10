using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
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

		private readonly PerTenant<string> _version;
		private readonly PerTenant<IDictionary<ruleMappingKey, MappedRule>> _ruleMappings;
		private readonly PerTenant<IDictionary<stateCodeMappingKey, MappedState>> _stateCodeMappings;
		private readonly PerTenant<IDictionary<Guid, MappedState>> _stateMappings;
		private readonly PerTenant<IEnumerable<Guid>> _loggedOutStateGroupIds;

		public StateMapper(IStateCodeAdder stateCodeAdder, IMappingReader reader, ICurrentDataSource dataSource)
		{
			_stateCodeAdder = stateCodeAdder;
			_reader = reader;

			_version = new PerTenant<string>(dataSource);
			_ruleMappings = new PerTenant<IDictionary<ruleMappingKey, MappedRule>>(dataSource);
			_stateCodeMappings = new PerTenant<IDictionary<stateCodeMappingKey, MappedState>>(dataSource);
			_stateMappings = new PerTenant<IDictionary<Guid, MappedState>>(dataSource);
			_loggedOutStateGroupIds = new PerTenant<IEnumerable<Guid>>(dataSource);
		}

		public MappedState StateFor(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			if (stateCode == null)
				return null;
			MappedState match;

			_stateCodeMappings.Value
				.TryGetValue(new stateCodeMappingKey
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
			_stateMappings.Value.TryGetValue(stateGroupId.Value, out match);
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
			var match = queryRule(businessUnitId, stateGroupId, activityId);
			if (activityId != null && match == null)
				match = queryRule(businessUnitId, stateGroupId, null);
			return match;
		}

		private MappedRule queryRule(Guid businessUnitId, Guid? stateGroupId, Guid? activityId)
		{
			MappedRule match;
			_ruleMappings.Value.TryGetValue(new ruleMappingKey
			{
				businessUnitId = businessUnitId,
				stateGroupId = stateGroupId,
				activityId = activityId
			}, out match);
			return match;
		}

		public IEnumerable<Guid> LoggedOutStateGroupIds()
		{
			return _loggedOutStateGroupIds.Value;
		}

		public void RefreshMappingCache(string latestVersion)
		{
			var refresh = latestVersion != _version.Value || _version.Value == null;
			if (!refresh)
				return;

			var mappings = _reader.Read();
			_ruleMappings.Set(
				mappings
					.Where(m =>
					{
						var illegal = m.StateCode == null && m.StateGroupId.HasValue;
						return !illegal;
					})
					.GroupBy(x => new ruleMappingKey
					{
						businessUnitId = x.BusinessUnitId,
						stateGroupId = x.StateGroupId,
						activityId = x.ActivityId
					})
					.ToDictionary(x => x.Key, m =>
					{
						var mapping = m.First();
						if (!mapping.RuleId.HasValue)
							return null;
						return new MappedRule
						{
							RuleId = mapping.RuleId.Value,
							RuleName = mapping.RuleName,
							Adherence = mapping.Adherence,
							StaffingEffect = mapping.StaffingEffect,
							DisplayColor = mapping.DisplayColor,
							IsAlarm = mapping.IsAlarm,
							ThresholdTime = mapping.ThresholdTime,
							AlarmColor = mapping.AlarmColor
						};
					})
			);
			_stateCodeMappings.Set(
				mappings
					.Where(x => x.StateGroupId.HasValue)
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
					})
			);
			_stateMappings.Set(
				mappings
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
					})
			);
			_loggedOutStateGroupIds.Set(
				mappings
					.Where(x => x.IsLoggedOut && x.StateGroupId.HasValue)
					.Select(x => x.StateGroupId.Value)
					.Distinct()
					.ToArray()
			);
			_version.Set(latestVersion);
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