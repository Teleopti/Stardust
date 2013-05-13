using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class PortalPage : Page
	{
		[FindBy(Id = "tabs")]
		public Element Menu;

		public Link ScheduleLink
		{
			get
			{
				return Document.Link(Find.By("href", s => s.EndsWith("#ScheduleTab")));
			}
		}

		public Link TeamScheduleLink
		{
			get
			{
				return Document.Link(Find.By("href", s => s.EndsWith("#TeamScheduleTab")));
			}
		}

		public Link StudentAvailabilityLink
		{
			get
			{
				return Document.Link(Find.By("href", s => s.EndsWith("#StudentAvailabilityTab")));
			}
		}

		public Link PreferencesLink
		{
			get { return Document.Link(Find.By("href", s => s.EndsWith("#PreferenceTab"))); }
		}

		public Link RequestsLink
		{
			get { return Document.Link(Find.By("href", s => s.EndsWith("#RequestsTab"))); }
		}

	    public Link MessageLink
        {
            get { return Document.Link(Find.By("href", s => s.EndsWith("#MessageTab"))); }
	    }

		[FindBy(Id = "asm-link")]
		public Link AsmButton { get; set; }

	}
}