using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public struct GroupPageLight : IEquatable<GroupPageLight>
	{
		private string _displayName;
		private string _key;

		public GroupPageType Type { get; private set; }

		public GroupPageLight(string displayName, GroupPageType groupPageType)
			: this()
		{
			DisplayName = displayName;
			Type = groupPageType;
			Key = Type.ToString();
		}

		public GroupPageLight(string displayName, GroupPageType groupPageType, string key) : this()
		{
			DisplayName = displayName;
			Type = groupPageType;
			Key = key;
		}

		public string DisplayName
		{
			get { return _displayName ?? "SingelAgent"; }
			private set { _displayName = value; }
		}

		public string Key
		{
			get { return _key ?? "SingelAgent"; }
			private set { _key = value; }
		}

		public static GroupPageLight SingleAgentGroup(string resourceKey)
		{
			return new GroupPageLight(resourceKey, GroupPageType.SingleAgent);
		}

		public bool Equals(GroupPageLight other)
		{
			return Key.Equals(other.Key);
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode();
		}
	}

	public enum GroupPageType
	{
		SingleAgent,
		Hierarchy,
		Contract,
		ContractSchedule,
		PartTimePercentage,
		Note,
		RuleSetBag,
		Skill,
		UserDefined
	}
}