using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Portal
{
	public class PortalViewModel
	{
		public IEnumerable<NavigationItem> NavigationItems { get; set; }
		public IEnumerable<ReportNavigationItem> ReportNavigationItems { get; set; }
		public string CustomerName { get; set; }
		public bool ShowChangePassword { get; set; }
		public bool ShowWFMAppGuide { get; set; }
		public bool AsmEnabled { get; set; }
		public bool ShowMeridian { get; set; }
		public bool UseJalaaliCalendar { get; set; }
		public string DateFormat { get; set; }
		public string TimeFormat { get; set; }
		public DateTimeDefaultValues DateTimeDefaultValues { get; set; }

		public IEnumerable<BadgeViewModel> Badges { get; set; }
		public string CurrentLogonAgentName { get; set; }

		/// <summary>
		/// Indicate if badge should be shown
		/// </summary>
		public bool ShowBadge { get; set; }
		public GamificationRollingPeriodSet BadgeRollingPeriodSet { get; set; }

		public string AMDesignator { get; set; }
		public string PMDesignator { get; set; }
		public string DateFormatLocale { get; set; }

		public bool GrantEnabled { get; set; }
		public Guid? CurrentLogonAgentId { get; set; }
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
		public bool IsWebReport { get; set; }
		public bool IsDivider { get; set; }
		public Guid Id { get; set; }
	}
	
	public class DateTimeDefaultValues
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string FullDayStartTime { get; set; }
		public string FullDayEndTime { get; set; }
		public int TodayYear { get; set; }
		public int TodayMonth { get; set; }
		public int TodayDay { get; set; }
	}

	public class PreferenceOption : Option
	{
		public bool Extended { get; set; }
	}

	public class PreferenceOptionGroup
	{
		public PreferenceOptionGroup(string text, IEnumerable<PreferenceOption> preferenceOptions)
		{
			Text = text;
			Options = preferenceOptions;
		}

		public string Text { get; }
		public IEnumerable<PreferenceOption> Options { get; }
	}
	
	public class Option
	{
		public string Value { get; set; }
		public string Text { get; set; }
		public string Color { get; set; }
	}

	public class SelectBase
	{
		public string text { get; set; }
	}
	
	public class SelectGroup : SelectBase
	{
		public SelectOptionItem[] children { get; set; }
		public Guid PageId { get; set; }
	}
	
	public class SelectOptionItem : SelectBase
	{
		public string id { get; set; }
	}

	public class BadgeViewModel
	{
		public int BadgeType { get; set; }
		public bool IsExternal { get; set; }
		public string Name { get; set; }
		public int BronzeBadge { get; set; }
		public int SilverBadge { get; set; }
		public int GoldBadge { get; set; }
	}
}