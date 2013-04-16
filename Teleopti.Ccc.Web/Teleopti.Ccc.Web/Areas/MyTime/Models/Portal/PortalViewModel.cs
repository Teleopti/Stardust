﻿using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Portal
{
	public class PortalViewModel
	{
		public IEnumerable<NavigationItem> NavigationItems { get; set; }
		public string CustomerName { get; set; }
		public bool ShowChangePassword { get; set; }
		public bool HasAsmPermission { get; set; }
		public bool ShowMeridian { get; set; }
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

	public class PreferenceOption : Option, IPreferenceOption
	{
		public bool Extended { get; set; }
	}

	public class PreferenceOptionSplit : OptionSplit, IPreferenceOption
	{
		public bool Extended { get { return false; } }
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

	public class OptionSplit : IOption
	{
		public string Value { get { return "-"; } }
		public string Text { get { return "-"; } }
		public string Color { get { return null; } }
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

	public class SelectSelectGroup : ISelectGroup
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
}