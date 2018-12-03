using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	public class WorkTimeStartEndExtractorTest
	{
		private MockRepository _mocks;
		private IVisualLayer _layer1;
		private IVisualLayer _layer2;
		private List<IVisualLayer> _layers;
		private WorkTimeStartEndExtractor _target;
		private IVisualLayer _layer3;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_layer1 = _mocks.StrictMock<IVisualLayer>();
			_layer2 = _mocks.StrictMock<IVisualLayer>();
			_layer3 = _mocks.StrictMock<IVisualLayer>();
			_layers = new List<IVisualLayer> {_layer1, _layer2, _layer3};
			_target = new WorkTimeStartEndExtractor();
		}

		[Test]
		public void WorkTimeStartShouldReturnNullWhenNullCollection()
		{
			Assert.That(_target.WorkTimeStart(null), Is.Null);
		}

		[Test]
		public void WorkTimeEndShouldReturnNullWhenNullCollection()
		{
			Assert.That(_target.WorkTimeEnd(null), Is.Null);
		}

		[Test]
		public void WorkTimeStartShouldGetStartTimeOnFirstLayerWithWorkTime()
		{
			var start = new DateTime(2011, 9, 7, 10, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, start.AddHours(1));
			Expect.Call(_layer1.WorkTime()).Return(TimeSpan.Zero);
			Expect.Call(_layer2.WorkTime()).Return(TimeSpan.FromHours(1));
			Expect.Call(_layer2.Period).Return(period);
			_mocks.ReplayAll();
			Assert.That(_target.WorkTimeStart(_layers), Is.EqualTo(start));
 			_mocks.VerifyAll();
		}

		[Test]
		public void WorkTimeEndShouldGetEndTimeOnLastLayerWithWorkTime()
		{
			var start = new DateTime(2011, 9, 7, 10, 0, 0, DateTimeKind.Utc);
			var period2 = new DateTimePeriod(start.AddHours(5), start.AddHours(6));

			Expect.Call(_layer1.WorkTime()).Return(TimeSpan.FromHours(1));
			Expect.Call(_layer2.WorkTime()).Return(TimeSpan.Zero);
			Expect.Call(_layer3.WorkTime()).Return(TimeSpan.FromHours(1));
			Expect.Call(_layer3.Period).Return(period2);

			_mocks.ReplayAll();
			Assert.That(_target.WorkTimeEnd(_layers), Is.EqualTo(start.AddHours(6)));
			_mocks.VerifyAll();
		}
	}
}