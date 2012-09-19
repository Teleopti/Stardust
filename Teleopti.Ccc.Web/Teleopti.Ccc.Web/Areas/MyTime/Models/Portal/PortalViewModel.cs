﻿using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Portal
{
	public class PortalViewModel
	{
		public IEnumerable<SectionNavigationItem> NavigationItems { get; set; }
		public string CustomerName { get; set; }
		public bool ShowChangePassword { get; set; }
		public bool ShowAsm { get; set; }
	}

	public class NavigationItem
	{
		public string Title { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }
	}

	public class SectionNavigationItem : NavigationItem
	{
		public IEnumerable<NavigationItem> NavigationItems { get; set; }
		public IEnumerable<ToolBarItemBase> ToolBarItems { get; set; }
	}

	public class PreferenceNavigationItem : SectionNavigationItem
	{
		public IEnumerable<IPreferenceOption> PreferenceOptions { get; set; }
		public IEnumerable<IOption> ActivityOptions { get; set; }
	}

	public abstract class ToolBarItemBase
	{
		public string Title { get; set; }
	}

	public class ToolBarSeparatorItem : ToolBarItemBase
	{
	}

	public class ToolBarButtonItem : ToolBarItemBase
	{
		public string ButtonType { get; set; }
		public string Icon { get; set; }
		public string Text { get; set; }
	}

	public class ToolBarSelectBox : ToolBarItemBase
	{
		public string Type { get; set; }
		public IEnumerable<IOption> Options { get; set; }
	}

	public class ToolBarSplitButton : ToolBarItemBase
	{
		public IEnumerable<IOption> Options { get; set; }
	}

	public class ToolBarDatePicker : ToolBarItemBase
	{
		public string NextTitle { get; set; }
		public string PrevTitle { get; set; }
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

}