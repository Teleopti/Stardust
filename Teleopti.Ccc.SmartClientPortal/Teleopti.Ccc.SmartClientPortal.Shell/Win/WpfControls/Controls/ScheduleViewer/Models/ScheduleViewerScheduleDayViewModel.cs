using System;
using System.Drawing;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.ScheduleViewer.Models
{
    public class ScheduleViewerScheduleDayViewModel : DependencyObject
    {
        private readonly IScheduleDay _part;
        
        public LayerViewModelCollection Layers { get; }

        public ScheduleViewerScheduleDayViewModel(IScheduleDay part,IEventAggregator eventAggregator)
        {
			Layers = new LayerViewModelCollection(eventAggregator, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), new ReplaceLayerInSchedule(), PrincipalAuthorization.Current_DONTUSE());
            Layers.AddFromSchedulePart(part);
            _part = part;
            DisplayColor = Color.LightBlue;
            Description = new Description("-");
	        var ass = _part.PersonAssignment();
            if (ass!=null)
            {
                HasMainShift = true;
                Description = ass.ShiftCategory.Description;
                DisplayColor = ass.ShiftCategory.DisplayColor;
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
		
        public DateTime Start => _part.DateOnlyAsPeriod.DateOnly.Date;

	    public DateTime End => _part.DateOnlyAsPeriod.DateOnly.Date.AddDays(1);

	    public DateTimePeriod Period => _part.Period;

	    public bool HasMainShift
        {
            get { return (bool)GetValue(HasMainShiftProperty); }
            set { SetValue(HasMainShiftProperty, value); }
        }

        public static readonly DependencyProperty HasMainShiftProperty =
            DependencyProperty.Register("HasMainShift", typeof(bool), typeof(ScheduleViewerScheduleDayViewModel), new UIPropertyMetadata(false));
    }
}
