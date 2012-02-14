using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.Restriction
{
    /// <summary>
    /// Interaction logic for RestrictionEditor.xaml
    /// </summary>
    public partial class RestrictionEditor : UserControl
    {
        private readonly RestrictionEditorViewModel _model;

        public static readonly RoutedEvent RestrictionChangedEvent = EventManager.RegisterRoutedEvent(
            "RestrictionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RestrictionEditor));

        // Provide CLR accessors for the event
        public event RoutedEventHandler RestrictionChanged
        {
            add { AddHandler(RestrictionChangedEvent, value); }
            remove { RemoveHandler(RestrictionChangedEvent, value); }
        }
        
        public IScheduleDay SchedulePart
        {
            get { return _model.SchedulePart; }
        }

        public RestrictionEditor()
            : this(new RestrictionEditorViewModel(null, new RepositoryFactory(),UnitOfWorkFactory.Current))
        {
        }

        public RestrictionEditor(RestrictionEditorViewModel model)
        {
            InitializeComponent();
            _model = model;
            _model.RestrictionChanged += model_RestrictionChanged;
            DataContext = _model;
        }

        public void LoadRestriction(IScheduleDay schedulePart)
        {
            _model.Load(schedulePart);
            if (schedulePart!=null) ChangeTimeZoneToCurrent(schedulePart.TimeZone);
        }

        private void ChangeTimeZoneToCurrent(ICccTimeZoneInfo timeZone)
        {
            TimeZoneInfo info = timeZone.TimeZoneInfoObject as TimeZoneInfo;
            if (info != null)
            {
                TimeZoneInfo current = VisualTreeTimeZoneInfo.GetTimeZoneInfo(this);
                if (current != info)
                {
                    VisualTreeTimeZoneInfo.SetTimeZoneInfo(this, info);
                }
            }
        }

        private void model_RestrictionChanged(object sender, EventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RestrictionChangedEvent));
        }

        public bool RestrictionIsAltered
        {
            get { return _model.RestrictionIsAltered; }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
