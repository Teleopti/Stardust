using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Editor
{
	
	[TestFixture, Apartment(ApartmentState.STA)]
	public class Bug26842
	{
		private EventAggregator _eventAggregator;
		private EditableShiftMapper _editableShiftMapper;
		private ShiftEditorViewModel _target;

		[SetUp]
		public void Setup()
		{
			_eventAggregator = new EventAggregator();
			_editableShiftMapper = new EditableShiftMapper();
			_target = new ShiftEditorViewModel(_eventAggregator, new CreateLayerViewModelService(), true, _editableShiftMapper);
		}

		[Test]
		public void ShouldBeAbleToMoveLayerMoreThanOnceWhenScheduleIsReloaded()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();

			//Emulates the reloading of a scheduleday as it works in the schedulescreen
			_eventAggregator.GetEvent<GenericEvent<TriggerShiftEditorUpdate>>().Subscribe(e =>
				                                                                              {
																								  var newScheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();
																								  _target.LoadSchedulePart(newScheduleDay);
				                                                                              });

			_target.LoadSchedulePart(scheduleDay);

			moveLayer(_target.Layers.First(), 37);
			moveLayer(_target.Layers.First(), 37);
		}

		private void moveLayer(ILayerViewModel layer, double pixels)
		{
			var panel = new DateTimePeriodPanel();
			FieldInfo fieldSize = typeof(UIElement).GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			fieldSize.SetValue(panel, new System.Windows.Size(10, 2));
			DateTimePeriodPanel.SetClipPeriod(panel, false);
			DateTimePeriodPanel.SetDateTimePeriod(panel, layer.Period);
			layer.EndTimeChanged(panel,pixels);
			layer.UpdatePeriod();
		}

	}
}