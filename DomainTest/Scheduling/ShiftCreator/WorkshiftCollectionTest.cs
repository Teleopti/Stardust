using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
	[TestFixture]
	public class WorkShiftCollectionTest
	{
		private MockRepository _mocks;
		private IWorkShiftAddCallback _callback;
		private WorkShiftCollection _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_callback = _mocks.DynamicMock<IWorkShiftAddCallback>();
			_target = new WorkShiftCollection(_callback);
		}

		[Test]
		public void ShouldCallCallbackOnAdd()
		{
			var item = _mocks.DynamicMock<IWorkShift>();
			Expect.Call(() =>_callback.BeforeAdd(item));
			_mocks.ReplayAll();
			_target.Add(item);
			_mocks.VerifyAll();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallCallbackOnRemove()
		{
			var item = _mocks.DynamicMock<IWorkShift>();
			Expect.Call(() => _callback.BeforeAdd(item));
			Expect.Call(() => _callback.BeforeRemove());
			_mocks.ReplayAll();
			_target.Add(item);
			_target.Remove(item);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallCallbackOnRemoveAt()
		{
			var item = _mocks.DynamicMock<IWorkShift>();
			
			Expect.Call(() => _callback.BeforeAdd(item));
			Expect.Call(() => _callback.BeforeRemove());
			_mocks.ReplayAll();
			_target.Add(item);
			_target.RemoveAt(0);
			_mocks.VerifyAll();
		}
	}

	
}