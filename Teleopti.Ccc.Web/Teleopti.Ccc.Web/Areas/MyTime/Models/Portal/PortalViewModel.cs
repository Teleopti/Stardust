using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Portal
{
	public class PortalViewModel
	{
		public IEnumerable<NavigationItem> NavigationItems { get; set; }
		public IEnumerable<ReportNavigationItem> ReportNavigationItems { get; set; }
		public string CustomerName { get; set; }
		public bool ShowChangePassword { get; set; }
		public bool HasAsmPermission { get; set; }
		public bool HasSignInAsAnotherUser { get; set; }
		public bool ShowMeridian { get; set; }
		public IEnumerable<BadgeViewModel> Badges { get; set; }
		public bool IsBadgesToggleEnabled { get; set; }
	}

	public class NavigationItem
	{
		public string Title { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }

		public bool PayAttention { get; set; }
		public string TitleCount { get; set; }

	public class TeamScheduleNavigationItem : NavigationItem
	{
	}



		public int UnreadMessageCount { get; set; }
	}

	public class ReportNavigationItem : NavigationItem
	{
		public string Url { get; set; }
		public bool IsMyReport { get; set; }
		public bool IsDivider { get; set; }
	}
	public class PreferenceOption : Option, IPreferenceOption
	{
		public bool Extended { get; set; }
	}

	public class PreferenceOptionGroup
	{
		public PreferenceOptionGroup(string text, IEnumerable<IPreferenceOption> preferenceOptions)
		{
			Text = text;
			Options = preferenceOptions;
		}

		public string Text { get; private set; }
		public IEnumerable<IPreferenceOption> Options { get; private set; }
	}

	public interface IPreferenceOption : IOption
	{
		bool Extended { get; }
	}


	public class Option : IOption
	{
		public string Value { get; set; }
		public string Text { get; set; }
		public string Color { get; set; }
	}
	
	public interface IOption
	{
		string Value { get; }
		string Text { get; }
		string Color { get; }
	}

	public interface ISelectOption
	{
		string text { get; }
	}

	public interface ISelectGroup : ISelectOption
	{
		ISelectOptionItem[] children { get; }
	}

	public class SelectGroup : ISelectGroup
	{
		public string text { get; set; }
		public ISelectOptionItem[] children { get; set; }
		public Guid PageId { get; set; }
	}

	public interface ISelectOptionItem : ISelectOption
	{
		string id { get; }
	}

	public class SelectOptionItem : ISelectOptionItem
	{
		public string text { get; set; }
		public string id { get; set; }
	}

	public class BadgeViewModel
	{
		public BadgeType BadgeType { get; set; }
		public int BronzeBadge { get; set; }
		public int SilverBadge { get; set; }
		public int GoldBadge { get; set; }
	}
}