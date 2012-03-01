using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Portal
{
	public class PortalViewModel
	{
		public IEnumerable<SectionNavigationItem> NavigationItems { get; set; }
		public string CustomerName { get; set; }
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
	}

	public class ToolBarSelectBox : ToolBarItemBase
	{
		public string Type { get; set; }
		public IEnumerable<SelectBoxOption> Options { get; set; }
	}

	public class SelectBoxOption
	{
		public string Value { get; set; }
		public string Text { get; set; }
	}

	public class ToolBarSplitButton : ToolBarItemBase
	{
		public IEnumerable<ISplitButtonOption> Options { get; set; }
	}

	public interface ISplitButtonOption
	{
		string Value { get; }
		string Text { get; }
		StyleClassViewModel Style { get; }
	}

	public class SplitButtonSplitter : ISplitButtonOption
	{
		public string Value { get { return "-"; } }
		public string Text { get { return "-"; } }
		public StyleClassViewModel Style { get; set; }
	}

	public class SplitButtonOption : ISplitButtonOption
	{
		public string Value { get; set; }
		public string Text { get; set; }
		public StyleClassViewModel Style { get; set; }
	}

	public class ToolBarDatePicker : ToolBarItemBase
	{
		public string NextTitle { get; set; }
		public string PrevTitle { get; set; }
	}
}