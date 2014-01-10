using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.UndoRedo
{
	[TestFixture]
	public class UndoRedoContainerTest
	{
		private IUndoRedoContainer target;
		private bool changedEventFired;

		[SetUp]
		public void Setup()
		{
			target = new UndoRedoContainer(10);
			changedEventFired = false;
			target.ChangedHandler+=OnChanged;
		}

		[TearDown]
		public void Cleanup()
		{
			target.ChangedHandler -= OnChanged;
		}

		[Test]
		public void VerifyContainerSize()
		{
				target = new UndoRedoContainer(1);
		    var mem = new dummy("1");
		    target.SaveState(mem);
		    mem.state = "2";
            target.SaveState(mem);
		    mem.state = "3";

		    target.Undo();
		    mem.state.Should().Be.EqualTo("2");
            target.CanUndo().Should().Be.False();
		}

		[Test]
		public void VerifyExceededContainerSizeWorks()
		{
			//Probably unnecessarly test because this is already tested in FixedCapacityTest
			target = new UndoRedoContainer(2);
			var mem = new dummy("1");
			target.SaveState(mem);
			mem.state = "2";
			target.SaveState(mem);
			mem.state = "3";
			target.SaveState(mem);
			mem.state = "4";

			target.Undo().Should().Be.True();
			mem.state.Should()
				.Be.EqualTo("3");
			target.Undo().Should().Be.True();
			mem.state.Should()
				.Be.EqualTo("2");
			target.Undo().Should().Be.False();
			mem.state.Should()
				.Be.EqualTo("2");
		}


		[Test]
		public void VerifyCanUndoWorks()
		{
			var mem = new dummy("dummy");
			target.SaveState(mem);
			mem.state = "arne";

			target.CanUndo().Should().Be.True();
			target.Undo();
			target.CanUndo().Should().Be.False();
		}

		[Test]
		public void VerifyUndoFiresChangeEvent()
		{
			target.Undo();
			changedEventFired.Should().Be.False();
			var mem = new dummy("dummy");
			target.SaveState(mem);
			target.Undo();
			changedEventFired.Should().Be.True();
		}


		[Test]
		public void VerifyUndoOrderWorks()
		{
			var mem1 = new dummy("nytt");
			var mem2 = new dummy("nytt");
			var mem3 = new dummy("nytt");
			target.SaveState(mem1);
			target.SaveState(mem2);
			target.SaveState(mem3);

			mem1.state = "ändrat";
			mem2.state = "ändrat";
			mem3.state = "ändrat";

			target.Undo();
			mem1.state.Should().Be.EqualTo("ändrat");
			mem2.state.Should().Be.EqualTo("ändrat");
			mem3.state.Should().Be.EqualTo("nytt");

			target.Undo();
			mem1.state.Should().Be.EqualTo("ändrat");
			mem2.state.Should().Be.EqualTo("nytt");
			mem3.state.Should().Be.EqualTo("nytt");

			mem1.state = "ändrat2";
			target.SaveState(mem1);

			mem1.state = "ändrat3";

			target.Undo();
			mem1.state.Should().Be.EqualTo("ändrat2");
			mem2.state.Should().Be.EqualTo("nytt");
			mem3.state.Should().Be.EqualTo("nytt");

			target.Undo();
			mem1.state.Should().Be.EqualTo("nytt");
			mem2.state.Should().Be.EqualTo("nytt");
			mem3.state.Should().Be.EqualTo("nytt");
		}


		[Test]
		public void VerifyUndoReturnsCorrectBoolAndEventIsFired()
		{
			var mem = new dummy("dummy");

			target.SaveState(mem);

			mem.state = "arne";

			target.Undo().Should().Be.True();
			target.Undo().Should().Be.False();
			target.Undo().Should().Be.False();
		}

		[Test]
		public void VerifyCanRedoWorks()
		{
			var mem = new dummy("dummy");

			target.SaveState(mem);

			mem.state = "arne";
			target.Undo();

			target.CanRedo().Should().Be.True();
			target.Redo();
			target.CanRedo().Should().Be.False();

			mem.state = "kurt";
			target.SaveState(mem);
			target.Undo();
			mem.state = "pelle";
			target.SaveState(mem);
			target.CanRedo().Should().Be.False();
		}

		[Test]
		public void VerifyRedoFiresChangeEvent()
		{
			var mem = new dummy("dummy");
			target.SaveState(mem);
			target.Undo();
			changedEventFired = false;
			target.Redo();
			changedEventFired.Should().Be.True();
		}

		[Test]
		public void VerifyRedoWorks()
		{
			var obj = new dummy("1");
			target.SaveState(obj);
			obj.state = "2";
			target.SaveState(obj);
			obj.state = "3";

			target.Undo();
			target.Undo();

			obj.state.Should().Be.EqualTo("1");
			target.Redo();
			obj.state.Should().Be.EqualTo("2");

			target.Undo();
			obj.state.Should().Be.EqualTo("1");
			target.Redo();
			obj.state.Should().Be.EqualTo("2");
			target.Redo();
			obj.state.Should().Be.EqualTo("3");
			target.Redo().Should().Be.False();
		}


		[Test]
		public void VerifyRedoReturnsCorrectBool()
		{
			var mem = new dummy("dummy");

			target.SaveState(mem);
			mem.state = "arne";

			target.SaveState(mem);
			mem.state = "bänkt";

			target.Undo();
			target.Undo();
			target.Redo().Should().Be.True();
			target.Redo().Should().Be.True();
			target.Redo().Should().Be.False();
			target.Redo().Should().Be.False();
		}

		[Test]
		public void VerifyClear()
		{
			var mem = new dummy("dummy");
			target.SaveState(mem);
			target.SaveState(mem);
			target.Undo();

			target.CanUndo().Should().Be.True();
			target.CanUndo().Should().Be.True();
			changedEventFired = false;
			target.Clear();
			changedEventFired.Should().Be.True();
			target.CanUndo().Should().Be.False();
			target.CanUndo().Should().Be.False();
		}

		[Test]
		public void CannotSaveNullState()
		{
			const dummy foo = null;
			new Action(() => target.SaveState(foo))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void VerifySaveStateClearsRedoStack()
		{
			var mem = new dummy("dummy");

			changedEventFired.Should().Be.False();
			target.SaveState(mem);
			changedEventFired.Should().Be.True();
			mem.state = "changed";

			target.Undo();

			target.SaveState(mem);
			mem.state = "changed again";

			target.Redo().Should().Be.False();
		}

		[Test]
		public void VerifyRollback()
		{
			var mem = new dummy("sdf");

			target.CreateBatch("a");
			target.SaveState(mem);
			target.RollbackBatch();
			target.CanUndo().Should().Be.False();
			changedEventFired.Should().Be.False();
		}


		[Test]
		public void VerifyBatchDescriptionUsedForErrorMessage()
		{
			target.CreateBatch("gnaget");
			try
			{
				target.Undo();
			}
			catch (Exception ex)
			{
				ex.Message.Should().Contain("gnaget");
				return;
			}
			Assert.Fail("Should throw");
		}

		[Test]
		public void VerifyEmptyBatchDoesNothing()
		{
			target.CreateBatch("a");
			target.CommitBatch();
			target.CanUndo().Should().Be.False();
			changedEventFired.Should().Be.False();
		}

		[Test]
		public void VerifyUndoAll()
		{
			dummy mem = new dummy("start");

			target.SaveState(mem);
			mem.state = "newer";

			target.SaveState(mem);
			mem.state = "even newer";

			target.UndoAll();
			mem.state.Should().Be.EqualTo("start");
		}

		[Test]
		public void VerifyUndoAllDoesNotCrashIfEmpty()
		{
			dummy mem = new dummy("start");

			target.UndoAll();
			mem.state.Should().Be.EqualTo("start");
		}

		[Test]
		public void VerifyInRedoUndoWhenUndoing()
		{
			target.InUndoRedo.Should().Be.False();

			var mem = new dummyThatCreatesMementoWhileRestoring(target);
			target.SaveState(mem);

			new Action(() => target.Undo())
				.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void CannotUndoInGroup()
		{
			target.CreateBatch("a");
			new Action(() => target.Undo())
				.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void CannotRedoInGroup()
		{
			target.CreateBatch("a");
			new Action(() => target.Redo())
				.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void CannotStartBatchInsideBatch()
		{
			target.CreateBatch("a");
			new Action(() => target.CreateBatch("b"))
				.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void CannotEndBatchTwice()
		{
			target.CreateBatch("a");
			target.CommitBatch();
			new Action(() => target.CommitBatch())
				.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void CannotEndNotStartedBatch()
		{
			new Action(() => target.CommitBatch())
				.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void CannotRollbackNotStartedBatch()
		{
			new Action(() => target.RollbackBatch())
				.Should().Throw<InvalidOperationException>();
		}

		private void OnChanged(object sender, EventArgs e)
		{
			changedEventFired = true;
		}

		private class dummyThatCreatesMementoWhileRestoring : IOriginator<dummyThatCreatesMementoWhileRestoring>
		{
			private readonly IUndoRedoContainer _container;

			public dummyThatCreatesMementoWhileRestoring(IUndoRedoContainer container)
			{
				_container = container;
			}

			public void Restore(dummyThatCreatesMementoWhileRestoring previousState)
			{
				Assert.IsTrue(_container.InUndoRedo);
				_container.SaveState(this);
			}

			public IMemento CreateMemento()
			{
				return new Memento<dummyThatCreatesMementoWhileRestoring>(this, this);
			}
		}

		private class dummy : IOriginator<dummy>
		{
			internal string state;

			internal dummy(string currentState)
			{
				state = currentState;
			}

			public void Restore(dummy previousState)
			{
				state = previousState.state;
			}

			public IMemento CreateMemento()
			{
				return new Memento<dummy>(this, new dummy(state));
			}
		}
	}
}