using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.Win.WpfControls.Controls.Notes
{
    /// <summary>
    /// Interaction logic for NotesEditor.xaml
    /// </summary>
    public partial class NotesEditor : UserControl
    {
		public bool Enabled
		{
			get { return (bool)GetValue(EnabledProperty); }
			set { SetValue(EnabledProperty, value); }
		}

		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.Register("Enabled", typeof(bool), typeof(NotesEditor), new PropertyMetadata(false));

        readonly NotesEditorViewModel _model;

        public static readonly RoutedEvent NotesChangedEvent = EventManager.RegisterRoutedEvent(
            "NotesChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotesEditor));

        public static readonly RoutedEvent PublicNotesChangedEvent = EventManager.RegisterRoutedEvent(
            "PublicNotesChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotesEditor));

        // Provide CLR accessors for the event
        public event RoutedEventHandler NotesChanged
        {
            add { AddHandler(NotesChangedEvent, value); }
            remove { RemoveHandler(NotesChangedEvent, value); }
        }

        public event RoutedEventHandler PublicNotesChanged
        {
            add { AddHandler(PublicNotesChangedEvent, value); }
            remove { RemoveHandler(PublicNotesChangedEvent, value); }
        }
 
         public NotesEditor(bool isEnabled)
            : this(new NotesEditorViewModel(null),isEnabled)
        {
        }

        public NotesEditor(NotesEditorViewModel model, bool isEnabled)
        {
            InitializeComponent();
            _model = model;
            _model.NotesChanged += model_NotesChanged;
            _model.PublicNotesChanged += _model_PublicNotesChanged;
            _model.IsEnabled = isEnabled;
            DataContext = _model;
        }

        public NotesEditor()
        {
        }

        public IScheduleDay SchedulePart
        {
            get { return _model.SchedulePart; }
        }

        public void LoadNote(IScheduleDay schedulePart)
        {
            _model.Load(schedulePart);
			Enabled = true;
        }

        private void model_NotesChanged(object sender, EventArgs e)
        {
			Enabled = false;
            RaiseEvent(new RoutedEventArgs(NotesChangedEvent));

        }

        private void _model_PublicNotesChanged(object sender, EventArgs e)
        {
	        Enabled = false;
            RaiseEvent(new RoutedEventArgs(PublicNotesChangedEvent));
        }


        public bool NotesIsAltered
        {
            get { return _model.NotesIsAltered; }
        }

        public bool PublicNotesIsAltered
        {
            get { return _model.PublicNotesIsAltered; }
        }

        public static void RemoveFocus()
        {
            var elementWithFocus = Keyboard.FocusedElement as UIElement;
            if (elementWithFocus != null)
            {
                elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            }
        }
    }
}
