using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.DomainTest.Collection
{
	public class ReadOnlyDictionary_DoNotUseTest
	{
		private IDictionary<int, string> target;
		private IDictionary<int, string> org;
		private MockRepository mocks;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			org = mocks.StrictMock<IDictionary<int, string>>();
			target = new ReadOnlyDictionary_DoNotUse<int, string>(org);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void VerifyNotNullToConstructor()
		{
			new ReadOnlyDictionary<double, int>(null);
		}

		#region NotSupportedExceptionTests
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void VerifyAddThrowsException()
		{
			target.Add(3, "3");
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void VerifyAdd2ThrowsException()
		{
			target.Add(new KeyValuePair<int, string>(3, "3"));
		}
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void VerifyRemoveThrowsException()
		{
			target.Remove(1);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void VerifyRemove2ThrowsException()
		{
			target.Remove(new KeyValuePair<int, string>(33, "33"));
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void VerifyCannotSetItem()
		{
			target[44] = "44";
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void VerifyCannotClear()
		{
			target.Clear();
		}

		#endregion


		[Test]
		public void VerifyItem()
		{
			using (mocks.Record())
			{
				Expect.On(org)
						.Call(org[1])
						.Return("1");
			}
			using (mocks.Playback())
			{
				Assert.AreEqual("1", target[1]);
			}
		}

		[Test]
		public void VerifyContainsKey()
		{

			using (mocks.Record())
			{
				Expect.On(org)
						.Call(org.ContainsKey(1))
						.Return(true);
			}
			using (mocks.Playback())
			{
				Assert.IsTrue(target.ContainsKey(1));
			}
		}

		[Test]
		public void VerifyTryGetValue()
		{
			string value;
			using (mocks.Record())
			{
				Expect.On(org)
						.Call(org.TryGetValue(1, out value))
						.Return(true);
			}
			using (mocks.Playback())
			{
				Assert.IsTrue(target.TryGetValue(1, out value));
			}
		}

		[Test]
		public void VerifyKeys()
		{
			ICollection<int> keys = new List<int>();
			using (mocks.Record())
			{
				Expect.Call(org.Keys).Return(keys);
			}
			using (mocks.Playback())
			{
				Assert.AreSame(keys, target.Keys);
			}
		}


		[Test]
		public void VerifyValues()
		{
			ICollection<string> values = new List<string>();
			using (mocks.Record())
			{
				Expect.Call(org.Values).Return(values);
			}
			using (mocks.Playback())
			{
				Assert.AreSame(values, target.Values);
			}
		}

		[Test]
		public void VerifyContains()
		{
			using (mocks.Record())
			{
				Expect.Call(org.Contains(new KeyValuePair<int, string>(1, "1"))).Return(true);
			}
			using (mocks.Playback())
			{
				Assert.IsTrue(target.Contains(new KeyValuePair<int, string>(1, "1")));
			}
		}

		[Test]
		public void VerifyCopyTo()
		{
			KeyValuePair<int, string>[] array = new KeyValuePair<int, string>[0];
			using (mocks.Record())
			{
				org.CopyTo(array, 17);
			}
			using (mocks.Playback())
			{
				target.CopyTo(array, 17);
			}
		}

		[Test]
		public void VerifyCount()
		{
			using (mocks.Record())
			{
				Expect.Call(org.Count).Return(77);
			}
			using (mocks.Playback())
			{
				Assert.AreEqual(77, target.Count);
			}
		}

		[Test]
		public void VerifyEnumerator()
		{
			var enumerator =
					mocks.StrictMock<IEnumerator<KeyValuePair<int, string>>>();
			using (mocks.Record())
			{
				Expect.Call(org.GetEnumerator()).Return(enumerator).Repeat.Twice();
			}
			using (mocks.Playback())
			{
				Assert.AreSame(enumerator, ((ReadOnlyDictionary_DoNotUse<int, string>)target).GetEnumerator());
				Assert.AreSame(enumerator, target.GetEnumerator());
			}
		}

		[Test]
		public void VerifyReadOnly()
		{
			Assert.IsTrue(target.IsReadOnly);
		}
	}
}
