﻿using System;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
	// Used for translation auto search, DO NOT REMOVE!
	// UserTexts.Resources.RowsPerPageTen
	// UserTexts.Resources.RowsPerPageTwenty
	// UserTexts.Resources.RowsPerPageThirty
	// UserTexts.Resources.RowsPerPageForty
	// UserTexts.Resources.RowsPerPageFifty

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), Serializable]
    public enum RowsPerPage
    {
        Ten = 10,
        Twenty = 20,
        Thirty = 30,
        Forty = 40,
        Fifty = 50
    }

    [Serializable]
    public class PersonFinderSettings : SettingValue
    {
        public PersonFinderSettings()
        {
            TerminalDate = DateTime.Today.AddMonths(-1);
            NumberOfRowsPerPage = RowsPerPage.Ten;
        }

        public DateTime TerminalDate { get; set; }
        public RowsPerPage NumberOfRowsPerPage { get; set; }
    }
}
