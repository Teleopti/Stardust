using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.WpfControls.Controls.ScheduleViewer.Models
{
    public class ScheduleViewerScheduleDayViewModel : DependencyObject
    {
        private IScheduleDay _part;
        
        public LayerViewModelCollection Layers { get; private set; }

        public ScheduleViewerScheduleDayViewModel(IScheduleDay part,IEventAggregator eventAggregator)
        {
			Layers = new LayerViewModelCollection(eventAggregator, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null);
            Layers.AddFromSchedulePart(part);
            _part = part;
            DisplayColor = Color.LightBlue;
            Description = new Description("-");
            if (_part.PersonAssignmentCollection().Count > 0)
            {
                HasMainShift = true;
                Description = _part.PersonAssignmentCollection().First().ShiftCategory.Description;
                DisplayColor = _part.PersonAssignmentCollection().First().ShiftCategory.DisplayColor;

            }

        }

        public Color DisplayColor
        {
            get { return (Color)GetValue(DisplayColorProperty); }
            set { SetValue(DisplayColorProperty, value); }
        }

      
        public static readonly DependencyProperty DisplayColorProperty =
            DependencyProperty.Register("DisplayColor", typeof(Color), typeof(ScheduleViewerScheduleDayViewModel), new UIPropertyMetadata(Color.LightBlue));



        public Description Description
        {
            get { return (Description)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

    
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(Description), typeof(ScheduleViewerScheduleDayViewModel), new UIPropertyMetadata(new Description("DEMO", "DE")));



        public DateTime Start
        {
            get { return _part.DateOnlyAsPeriod.Period().LocalStartDateTime; }
        }

        public DateTime End
        {
            get { return _part.DateOnlyAsPeriod.Period().LocalEndDateTime; }
        }

        public DateTimePeriod Period
        {
            get { return _part.Period; }
        }

        public bool HasMainShift
        {
            get { return (bool)GetValue(HasMainShiftProperty); }
            set { SetValue(HasMainShiftProperty, value); }
        }

        public static readonly DependencyProperty HasMainShiftProperty =
            DependencyProperty.Register("HasMainShift", typeof(bool), typeof(ScheduleViewerScheduleDayViewModel), new UIPropertyMetadata(false));

    }
}
