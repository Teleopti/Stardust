using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers
{
    /// <summary>
    /// People Admin Helper 
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-07-14
    /// </remarks>
    public static class PeopleAdminHelper
    {
        private const string Cangray = "CanGray";
        private const string Isaverageworktimeperdayoverride = "IsAverageWorkTimePerDayOverride";
        private const string Isdayoffoverride = "IsDaysOffOverride";

        #region Methods


        /// <summary>
        /// Gets the first day of week.
        /// </summary>
        /// <param name="selectedDateTime">The selected date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-05
        /// </remarks>
        public static DateTime GetFirstDayOfWeek(DateTime selectedDateTime)
        {
            // One can only start a rotation on the first day in the week
            DayOfWeek firstDayInWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            while (selectedDateTime.DayOfWeek != firstDayInWeek)
            {
                selectedDateTime = selectedDateTime.AddDays(-1);
            }
            return selectedDateTime;
        }

        /// <summary>
        /// Determines whether [has person period duplicates2] [the specified person periods].
        /// </summary>
        /// <param name="personPeriods">The person periods.</param>
        /// <returns>
        /// 	<c>true</c> if [has person period duplicates2] [the specified person periods]; otherwise, 
        /// <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-07-04
        /// </remarks>
        public static bool HasPersonPeriodDuplicates2(IEnumerable<IPersonPeriod> personPeriods)
        {
            var x = new PersonPeriodComparer();

            IEnumerable<IPersonPeriod> periods = personPeriods.Distinct(x);

            IList<IPersonPeriod> distinctPersonPeriods = periods.ToList();

            return (distinctPersonPeriods.Count != personPeriods.Count());
        }

        /// <summary>
        /// Validates the period is not duplicate.
        /// </summary>
        /// <param name="filteredPeopleHolder">The filtered people holder.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-07-14
        /// </remarks>
        public static string ValidatePeriodIsNotDuplicate(FilteredPeopleHolder filteredPeopleHolder)
        {
            foreach (PersonPeriodModel adapter in filteredPeopleHolder.PersonPeriodGridViewCollection)
            {
				if (!adapter.AdapterOrChildCanBold()) continue;

				if (HasPersonPeriodDuplicates2(adapter.Parent.PersonPeriodCollection))
                {
                    //TODO: Need to change this according to empty name
                    //if (String.IsNullOrEmpty(adapter.FullName))
                    //    return "[ ]";

                    return adapter.FullName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines whether [has schedule period duplicates] [the specified schedule periods].
        /// </summary>
        /// <param name="schedulePeriods">The schedule periods.</param>
        /// <returns>
        /// 	<c>true</c> if [has schedule period duplicates] [the specified schedule periods]; otherwise, 
        /// <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-07-21
        /// </remarks>
        public static bool HasSchedulePeriodDuplicates(IList<ISchedulePeriod> schedulePeriods)
        {
            Dictionary<DateOnly, ISchedulePeriod> y = new Dictionary<DateOnly, ISchedulePeriod>();

            foreach (ISchedulePeriod test in schedulePeriods)
            {
                if (y.ContainsKey(test.DateFrom))
                {
                    return true;
                }
                y.Add(test.DateFrom, test);
            }

            return false;
        }

        /// <summary>
        /// Validates the schedule period is not duplicate.
        /// </summary>
        /// <param name="filteredPeopleHolder">The filtered people holder.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-07-21
        /// </remarks>
        public static string ValidateSchedulePeriodIsNotDuplicate(FilteredPeopleHolder filteredPeopleHolder)
        {
            foreach (SchedulePeriodModel adapter in filteredPeopleHolder.SchedulePeriodGridViewCollection)
            {
				if(!adapter.AdapterOrChildCanBold()) continue;

                if (HasSchedulePeriodDuplicates(adapter.Parent.PersonSchedulePeriodCollection))
                {
                    return adapter.FullName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Validates the person rotation is not duplicate.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:     Shiran Ginige
        /// Created date:   2008-07-21
        /// </remarks>
        public static string ValidatePersonRotationIsNotDuplicate(FilteredPeopleHolder filteredPeopleHolder)
        {	
		    for (int i = 0; i < filteredPeopleHolder.PersonRotationParentAdapterCollection.Count; i++)
		    {
			    if(!filteredPeopleHolder.PersonRotationParentAdapterCollection[i].AdapterOrChildCanBold()) continue;
			    
			    PeopleWorksheet.StateHolder.GetChildPersonRotations(i, filteredPeopleHolder);

			    Dictionary<DateOnly, IPersonRotation> y = new Dictionary<DateOnly, IPersonRotation>();

			    foreach (IPersonRotation personRotation in PeopleWorksheet.StateHolder.
				    ChildPersonRotationCollection)
			    {
				    if (y.ContainsKey(personRotation.StartDate))
				    {
					    return personRotation.Person.Name.ToString();
				    }
				    y.Add(personRotation.StartDate, personRotation);
			    }
		    }
	        
	        return string.Empty;
        }


        public static string ValidatePersonAvailabilityIsNotDuplicate(FilteredPeopleHolder filteredPeopleHolder)
        {

            for (int i = 0; i < filteredPeopleHolder.PersonAvailabilityParentAdapterCollection.Count; i++)
            {
				if (!filteredPeopleHolder.PersonAvailabilityParentAdapterCollection[i].AdapterOrChildCanBold()) continue;

				PeopleWorksheet.StateHolder.GetChildPersonAvailabilities(i, filteredPeopleHolder);

                Dictionary<DateOnly, IPersonAvailability> y = new Dictionary<DateOnly, IPersonAvailability>();

                foreach (IPersonAvailability personAvailability in PeopleWorksheet.StateHolder.
                    ChildPersonAvailabilityCollection)
                {
                    if (y.ContainsKey(personAvailability.StartDate))
                    {
                        return personAvailability.Person.Name.ToString();
                    }
                    y.Add(personAvailability.StartDate, personAvailability);
                }
            }
            return string.Empty;

        }


        /// <summary>
        /// Determines whether this instance can gray the specified property reflector.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyReflector">The property reflector.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> 
        /// instance containing the event data.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-07-23
        /// </remarks>
        public static void GrayColumn<T>(PropertyReflector propertyReflector, T dataItem,
            GridQueryCellInfoEventArgs e)
        {
            if ((bool)propertyReflector.GetValue(dataItem, Cangray))
            {
                e.Style.BackColor = Color.Silver;
                e.Style.CellType = "Test";
                e.Style.CellValue = string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 07-05-2008
        /// </remarks>
        public static ClipHandler<string> ConvertClipboardToClipHandler()
        {
            ClipHandler<string> clipHandler = new ClipHandler<string>();

			
            //check if there is any data in clipboard
            if (Clipboard.ContainsText())
            {
                //get text in clipboard
                string clipboardText = Clipboard.GetText();
                //remove "\n"
                clipboardText = clipboardText.Replace("\n", "");
                //remove empty string at end
                clipboardText = clipboardText.TrimEnd();
                //split on rows
                string[] clipBoardRows = clipboardText.Split('\r');

                int row = 0;
                //loop each row
                foreach (string rowString in clipBoardRows)
                {
                    //split on columns
                    string[] clipBoardCols = rowString.Split('\t');

                    int col = 0;
                    //loop each column
                    foreach (string columnString in clipBoardCols)
                    {
                        //add to cliphandler
                        clipHandler.AddClip(row, col, columnString);

                        col++;
                    }

                    row++;
                }
            }


            return clipHandler;
        }

        /// <summary>
        /// Creates the col width for child grid.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridRowColSizeEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="isValidColumn">if set to <c>true</c> [is valid column].</param>
        /// <param name="lastColumnIndex">Last index of the column.</param>
        /// <param name="renderingValue">The rendering value.</param>
        /// <param name="colWidth">Width of the col.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-22
        /// </remarks>
        public static void CreateColWidthForChildGrid(GridRowColSizeEventArgs e, bool isValidColumn,
            int lastColumnIndex, int renderingValue, int colWidth)
        {
            //Hack: Fix for Child Navigation.Child column header value is 0.01 
            if (e.Index == 0)
                e.Size = 1 / 100;

            else if (isValidColumn)
            {
                // This is solution for child resizing and navaigtion rendering issues.Add value 2 to match parent 
                //and child columns
                e.Size = colWidth;

                if (e.Index + 2 == lastColumnIndex)
                {
                    // In Last column need to substract 2 to recover rendering navigation issues.
                    e.Size = colWidth - renderingValue;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Creates the row height for child grid.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridRowColSizeEventArgs"/> 
        /// instance containing the event data.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-22
        /// </remarks>
        public static void CreateRowHeightForChildGrid(GridRowColSizeEventArgs e)
        {
            //Hack: Fix for Child Navigation
            if (e.Index == 0)
                e.Size = 1 / 100;
            else
                e.Size = 20;
            e.Handled = true;
        }

        /// <summary>
        /// Creates the child grid column count.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="columnCount">The column count.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-22
        /// </remarks>
        public static void CreateColumnCountForChildGrid(GridRowColCountEventArgs e, int columnCount)
        {
            e.Count = columnCount;
            e.Handled = true;
        }

        /// <summary>
        /// Creates the row count for child grid.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="collection">The collection.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-22
        /// </remarks>
        public static void CreateRowCountForChildGrid<T>(GridRowColCountEventArgs e, IList<T> collection)
        {
            if (collection != null)
            {
                e.Count = collection.Count;
                e.Handled = true;
            }
        }


        /// <summary>
        /// Determines whether [is new value] [the specified property reflector].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyReflector">The property reflector.</param>
        /// <param name="dataItem">The data item.</param>
        /// <returns>
        /// 	<c>true</c> if [is new value] [the specified property reflector]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-20
        /// </remarks>
        public static bool IsAverageWorkTimeOverridable<T>(PropertyReflector propertyReflector, T dataItem)
        {
            return (bool)propertyReflector.GetValue(dataItem, Isaverageworktimeperdayoverride);
        }

        public static bool IsDayOffOverridable<T>(PropertyReflector propertyReflector, T dataItem)
        {
            return (bool)propertyReflector.GetValue(dataItem, Isdayoffoverride);
        }


        /// <summary>
        /// Resets the day off.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem">The data item.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-23
        /// </remarks>
        public static void ResetDayOff<T>(T dataItem)
        {
            ISchedulePeriodModel schedulePeriodAdapter = dataItem as ISchedulePeriodModel;

            if ((schedulePeriodAdapter != null))
            {
                SchedulePeriod schedulePeriod = schedulePeriodAdapter.SchedulePeriod as SchedulePeriod;

                if (schedulePeriod != null)
                {
                    schedulePeriod.ResetDaysOff();
                }
            }
        }

        /// <summary>
        /// Resets the day off.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem">The data item.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-23
        /// </remarks>
        public static void ResetAverageWorkTimePerDay<T>(T dataItem)
        {
            ISchedulePeriodModel schedulePeriodAdapter = dataItem as ISchedulePeriodModel;

            if ((schedulePeriodAdapter != null))
            {
                SchedulePeriod schedulePeriod = schedulePeriodAdapter.SchedulePeriod as SchedulePeriod;

                if (schedulePeriod != null)
                {
                    schedulePeriod.ResetAverageWorkTimePerDay();
                }
            }
        }

		/// <summary>
		/// Resets the period time.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataItem">The data item.</param>
		public static void ResetPeriodTime<T>(T dataItem)
		{
			ISchedulePeriodModel schedulePeriodAdapter = dataItem as ISchedulePeriodModel;

			if ((schedulePeriodAdapter != null))
			{
				SchedulePeriod schedulePeriod = schedulePeriodAdapter.SchedulePeriod as SchedulePeriod;

				if (schedulePeriod != null)
				{
					schedulePeriod.ResetPeriodTime();
				}
			}
		}

        /// <summary>
        /// Determines whether [is can bold] [the specified period].
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="personPeriodChildGridViewCollection">The person period child grid view collection.</param>
        /// <returns>
        /// 	<c>true</c> if [is can bold] [the specified period]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-14
        /// </remarks>
        public static bool IsCanBold(IPersonPeriod period,
            ReadOnlyCollection<PersonPeriodChildModel> personPeriodChildGridViewCollection)
        {
            bool canBold = false;

            if (personPeriodChildGridViewCollection != null && period != null)
            {
                PersonPeriodChildModel model =
                    personPeriodChildGridViewCollection.FirstOrDefault(p => p.ContainedEntity.Id == period.Id);
                if (model != null)
                {
                    canBold = model.CanBold;
                }
            }

            return canBold;
        }

        /// <summary>
        /// Determines whether [is can bold] [the specified period].
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="schedulePeriodChildGridViewCollection">The schedule period child grid view collection.</param>
        /// <returns>
        /// 	<c>true</c> if [is can bold] [the specified period]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-14
        /// </remarks>
        public static bool IsCanBold(ISchedulePeriod period,
            ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildGridViewCollection)
        {
            bool canBold = false;

            if (schedulePeriodChildGridViewCollection != null && period != null)
            {
                try
                {
                    SchedulePeriodChildModel model = schedulePeriodChildGridViewCollection.
                        Where(p => p.ContainedEntity.Id == period.Id).Single();
                    canBold = model.CanBold;
                }
                catch (InvalidOperationException)
                {
                    //marias 2011-06-09:
                    //Can bold is still false because no elements matched single()
                }
            }

            return canBold;
        }


        /// <summary>
        /// Invalidates the grid range.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="columnCount">The column count.</param>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-14
        /// </remarks>
        public static void InvalidateGridRange(int row, int columnCount, GridControlBase grid)
        {
            if (grid != null)
            {
                grid.InvalidateRange(GridRangeInfo.Cells(row, 1, row, columnCount));
            }
        }


        /// <summary>
        /// Determines whether [is can bold] [the specified person rotation].
        /// </summary>
        /// <param name="personRotation">The person rotation.</param>
        /// <param name="personRotationChildGridViewCollection">The person rotation child grid view collection.</param>
        /// <returns>
        /// 	<c>true</c> if [is can bold] [the specified person rotation]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-17
        /// </remarks>
        public static bool IsCanBold(IPersonRotation personRotation,
           ReadOnlyCollection<PersonRotationModelChild> personRotationChildGridViewCollection)
        {
            bool canBold = false;

            if (personRotationChildGridViewCollection != null && personRotation != null)
            {
                IList<PersonRotationModelChild> adaptercollection = personRotationChildGridViewCollection.
                                                               Where(p => p.PersonRotation.Id == personRotation.Id).ToList();

                if (adaptercollection.Count > 0)
                {
                    canBold = adaptercollection[0].CanBold;
                }
            }

            return canBold;
        }

	    
	    /// <remarks>
	    /// Created by: Dinesh Ranasinghe
	    /// Created date: 2008-11-17
	    /// </remarks>
	    public static bool IsCanBold(IAccount account,ReadOnlyCollection<IPersonAccountChildModel> personAccountChildGridViewCollection)
        {
            bool canBold = false;

            if (personAccountChildGridViewCollection != null && account != null)
            {
                IList<IPersonAccountChildModel> adaptercollection = personAccountChildGridViewCollection.
                                                               Where(p => p.ContainedEntity.Id == account.Id).ToList();

                if (adaptercollection.Count > 0)
                {
                    canBold = adaptercollection[0].CanBold;
                }
            }

            return canBold;
        }

        public static bool IsCanBold(IPersonAvailability personAvailability,
           ReadOnlyCollection<PersonAvailabilityModelChild> personAvailabilityChildGridViewCollection)
        {
            bool canBold = false;

            if (personAvailabilityChildGridViewCollection != null && personAvailability != null)
            {
                IList<PersonAvailabilityModelChild> adaptercollection = personAvailabilityChildGridViewCollection.
                                                               Where(p => p.PersonRotation.Id == personAvailability.Id).ToList();

                if (adaptercollection.Count > 0)
                {
                    canBold = adaptercollection[0].CanBold;
                }
            }

            return canBold;
        }

        #endregion
    }

}
