using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class TransparentMeetingControlModelTest
	{
		private TransparentMeetingControlModel _transparentMeetingControlModel;
		private Object _parent;
		private bool _eventFired;

		[SetUp]
		public void Setup()
		{
			_parent = new Object();
			_transparentMeetingControlModel = new TransparentMeetingControlModel(0, 0, _parent, Color.Blue, 5);
			_eventFired = false;
		}

		[Test]
		public void ShouldSetProperties()
		{
			_transparentMeetingControlModel.Top = 1;
			_transparentMeetingControlModel.Height = 2;
			_transparentMeetingControlModel.Width = 3;
			_transparentMeetingControlModel.Left = 4;
			_transparentMeetingControlModel.BorderWidth = 6;
			_transparentMeetingControlModel.LeftBorderColor = Color.Green;
			_transparentMeetingControlModel.RightBorderColor = Color.Yellow;
			_transparentMeetingControlModel.ModelChanged = true;
			
			Assert.AreEqual(1,_transparentMeetingControlModel.Top);
			Assert.AreEqual(2,_transparentMeetingControlModel.Height);
			Assert.AreEqual(3,_transparentMeetingControlModel.Width);
			Assert.AreEqual(4,_transparentMeetingControlModel.Left);
			Assert.AreEqual(5, _transparentMeetingControlModel.Transparency);
			Assert.AreEqual(6,_transparentMeetingControlModel.BorderWidth);
			Assert.AreEqual(Color.Blue,_transparentMeetingControlModel.BackColor);
			Assert.AreEqual(Color.Green, _transparentMeetingControlModel.LeftBorderColor);
			Assert.AreEqual(Color.Yellow, _transparentMeetingControlModel.RightBorderColor);
			Assert.AreSame(_parent,_transparentMeetingControlModel.Parent);
			Assert.IsTrue(_transparentMeetingControlModel.ModelChanged);
		}

		[Test]
		public void ShouldRaiseChangedEvent()
		{
			_transparentMeetingControlModel.TransparentControlModelChanged += TransparentMeetingControlModelTransparentMeetingControlModelChanged;
			_transparentMeetingControlModel.ModelChanged = true;
			_transparentMeetingControlModel.RaisePossibleChangeEvent();

			Assert.IsTrue(_eventFired);

			_transparentMeetingControlModel.TransparentControlModelChanged -= TransparentMeetingControlModelTransparentMeetingControlModelChanged;
		}

		private void TransparentMeetingControlModelTransparentMeetingControlModelChanged(object sender, EventArgs e)
		{
			_eventFired = true;
		}

	}
}
