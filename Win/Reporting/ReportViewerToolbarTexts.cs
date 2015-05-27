using Microsoft.Reporting.WinForms;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Win.Reporting
{
	class ReportViewerToolbarTexts :IReportViewerMessages
	{
		public string DocumentMapButtonToolTip { get; set; }
		public string ParameterAreaButtonToolTip { get; set; }
		public string FirstPageButtonToolTip { get { return Resources.RepViewerFirstPage; } }
		public string PreviousPageButtonToolTip { get { return Resources.RepViewerPreviousPage; } }
		public string CurrentPageTextBoxToolTip { get { return Resources.RepViewerCurrentPage; } }
		public string PageOf { get { return Resources.RepViewerOf; } }
		public string NextPageButtonToolTip { get { return Resources.RepViewerNextPage; } }
		public string LastPageButtonToolTip { get { return Resources.RepViewerLastPage; } }
		public string BackButtonToolTip { get; set; }
		public string RefreshButtonToolTip { get { return Resources.RepViewerRefresh; } }
		public string PrintButtonToolTip { get { return Resources.RepViewerPrint; } }
		public string ExportButtonToolTip { get { return Resources.RepViewerExport; } }
		public string ZoomControlToolTip { get { return Resources.RepViewerZoom; } }
		public string SearchTextBoxToolTip { get { return Resources.RepViewerFindText; } }
		public string FindButtonToolTip { get { return Resources.RepViewerFind; } }
		public string FindNextButtonToolTip { get { return Resources.RepViewerFindNext; } }
		public string ZoomToPageWidth { get { return Resources.RepViewerPageWidth; } }
		public string ZoomToWholePage { get { return Resources.RepViewerWholePage; } }
		public string FindButtonText { get { return Resources.RepViewerFind; } }
		public string FindNextButtonText { get { return Resources.RepViewerNext; } }
		public string ViewReportButtonText { get; set; }
		public string ProgressText { get; set; }
		public string TextNotFound { get { return Resources.RepViewerTextNotFound; } }
		public string NoMoreMatches { get { return Resources.RepViewerNoMoreMatches; } }
		public string ChangeCredentialsText { get; set;  }
		public string NullCheckBoxText { get; set;  }
		public string NullValueText { get; set;  }
		public string TrueValueText { get; set;  }
		public string FalseValueText { get; set;  }
		public string SelectAValue { get; set;  }
		public string UserNamePrompt { get; set;  }
		public string PasswordPrompt { get; set;  }
		public string SelectAll { get; set;  }
		public string PrintLayoutButtonToolTip { get { return Resources.RepViewerPrinLayout; } }
		public string PageSetupButtonToolTip { get { return Resources.RepViewerPageSetup; } }
		public string NullCheckBoxToolTip { get; set;  }
		public string TotalPagesToolTip { get { return Resources.RepViewerTotalPages; } }
		public string StopButtonToolTip { get { return Resources.RepViewerStop; } }
		public string DocumentMapMenuItemText { get; set;  }
		public string BackMenuItemText { get; set;  }
		public string RefreshMenuItemText { get; set;  }
		public string PrintMenuItemText { get { return Resources.RepViewerPrint; } }
		public string PrintLayoutMenuItemText { get { return Resources.RepViewerPrinLayout; } }
		public string PageSetupMenuItemText { get {return Resources.RepViewerPageSetup;}  }
		public string ExportMenuItemText { get { return Resources.RepViewerExport; } }
		public string StopMenuItemText { get { return Resources.RepViewerStop; } }
		public string ZoomMenuItemText { get { return Resources.RepViewerZoom; } }
		public string ViewReportButtonToolTip { get; set;  }
	}
}
