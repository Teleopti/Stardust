using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views
{
	public class EmptyGridView : GridViewBase
	{
		public override IEnumerable<Tuple<IPerson, int>> Sort(bool isAscending)
		{
			return Enumerable.Empty<Tuple<IPerson, int>>();
		}

		public override void PerformSort(IEnumerable<Tuple<IPerson, int>> order)
		{
		}

		internal override ViewType Type => ViewType.EmptyView;

		public EmptyGridView(GridControl view, FilteredPeopleHolder filteredPeopleHolder, bool visible)
			: base(view, filteredPeopleHolder)
		{
			view.Visible = visible;
		}
	}
}
