using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.UndoRedo
{
	[TestFixture]
	public class MementoTest
	{
		private IMemento target;
		private IOriginator<IPerson> orig;
		private MockRepository mocks;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			orig = mocks.StrictMock<IOriginator<IPerson>>();
		}

		[Test]
		public void VerifyRestoreWorks()
		{
			var org = new Person();
			var retAgent = mocks.StrictMock<IMemento>();
			using (mocks.Record())
			{
				Expect.On(orig)
					.Call(orig.CreateMemento())
					.Return(retAgent);
				orig.Restore(org);
			}
			using (mocks.Playback())
			{
				target = new Memento<IPerson>(orig, org);

				target.Restore().Should()
					.Be.SameInstanceAs(retAgent);
			}
		}

		[Test]
		public void VerifyNotNullAsOriginator()
		{
			new Action(() => new Memento<IPerson>(null, new Person()))
				.Should()
				.Throw<ArgumentNullException>();
		}

		[Test]
		public void VerifyNotNullAsOldState()
		{
			new Action(() => new Memento<IPerson>(orig, null))
				 .Should()
				 .Throw<ArgumentNullException>();
		}
	}
}