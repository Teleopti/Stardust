using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
	public class EmptyGridView : GridViewBase
	{
		internal override ViewType Type
		{
			get { return ViewType.EmptyView; }
		}

		public EmptyGridView(GridControl view, FilteredPeopleHolder filteredPeopleHolder, bool visible)
			: base(view, filteredPeopleHolder)
		{
			view.Visible = visible;
		}
	}
}
