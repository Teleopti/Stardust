using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    /// <summary>
    /// Settings for ShiftEditor
    /// 
    /// </summary>
    public class ShiftEditorSettings:DependencyObject
    {
        //refactor
        private bool _isMovedInternaly;
        private ObservableCollection<IShiftCategory> _shiftCategories;
        public IShiftEditorSettingsTarget Target{ get; private set;}

        public ObservableCollection<IShiftCategory> ShiftCategories { 
            get
            {
                return _shiftCategories;
            } 
            private set{ _shiftCategories = value;} }

        public ShiftEditorSettings(IShiftEditorSettingsTarget target):this(target, new List<IShiftCategory>()){}

        public ShiftEditorSettings(IShiftEditorSettingsTarget target,IList<IShiftCategory> categories)
        {
            ShiftCategories = new ObservableCollection<IShiftCategory>(categories);
            CollectionViewSource.GetDefaultView(ShiftCategories).CurrentChanged += ShiftEditorSettingsCurrentShiftCategoryChanged;
            Target = target;
            _isMovedInternaly = false;
        }

        void ShiftEditorSettingsCurrentShiftCategoryChanged(object sender, EventArgs e)
        {
           if(!_isMovedInternaly) Target.SettingsAltered(this);
        }

        public TimeSpan Interval
        {
            get { return (TimeSpan) GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        #region settings

        public bool ClipAbsence
        {
            get { return (bool)GetValue(ClipAbsenceProperty); }
            set { SetValue(ClipAbsenceProperty, value); }
        }

        public bool Expanded
        {
            get { return (bool)GetValue(ExpandedProperty); }
            set { SetValue(ExpandedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the width of the details.
        /// (Field not belonging to the actual Layerview, settings, layerinfo etc)
        /// </summary>
        /// <value>The width of the details.</value>
        /// <remarks>
        /// Put this here instead of the control so we can store the value for the user easier.
        /// </remarks>
        public double DetailsWidth
        {
            get { return (double) GetValue(DetailsWidthProperty); }
            set { SetValue(DetailsWidthProperty, value); }
        }

        public static readonly DependencyProperty DetailsWidthProperty =
            DependencyProperty.Register("DetailsWidth", 
            typeof (double), 
            typeof (ShiftEditorSettings), 
            new UIPropertyMetadata(100d,SettingsChanged));

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", 
            typeof (TimeSpan), 
            typeof (ShiftEditorSettings), 
            new UIPropertyMetadata(TimeSpan.FromMinutes(15),SettingsChanged,CoerceInterval));

        private static object CoerceInterval(DependencyObject d, object basevalue)
        {
            TimeSpan t = (TimeSpan) basevalue;
            return t < TimeSpan.FromMinutes(1) ? TimeSpan.FromMinutes(1) : basevalue;
        }

        public static readonly DependencyProperty ExpandedProperty =
            DependencyProperty.Register("Expanded", 
            typeof (bool), 
            typeof (ShiftEditorSettings),
            new UIPropertyMetadata(false, SettingsChanged));

        public static readonly DependencyProperty ClipAbsenceProperty =
            DependencyProperty.Register("ClipAbsence",
            typeof (bool), 
            typeof (ShiftEditorSettings),
            new UIPropertyMetadata(true,SettingsChanged));
        #endregion

        private static void SettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShiftEditorSettings settings = (ShiftEditorSettings) d;
            settings.Target.SettingsAltered(settings);
        }
    }
}
