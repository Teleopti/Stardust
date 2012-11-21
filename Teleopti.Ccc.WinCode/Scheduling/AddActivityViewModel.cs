﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class AddActivityViewModel :AddLayerViewModel<IActivity>, IAddActivityViewModel
    {
        private readonly ReadOnlyCollection<IShiftCategory> _categories;

        public ICollectionView ShiftCategories
        {
            get { return CollectionViewSource.GetDefaultView(_categories) as ListCollectionView; }
        }

        public AddActivityViewModel(IList<IActivity> activities, IList<IShiftCategory> categories, DateTimePeriod period,TimeSpan interval)
            : base(activities, period, UserTexts.Resources.AddActivity, interval)
        {

            PeriodViewModel.Min = period.StartDateTime.Date;
            _categories = new ReadOnlyCollection<IShiftCategory>(categories);
        }

        public IShiftCategory SelectedShiftCategory
        {
            get { return CollectionViewSource.GetDefaultView(_categories).CurrentItem as IShiftCategory; }
        }
    }
}



