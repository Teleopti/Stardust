using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Aop
{
	[DomainTest]
	public class OrderedAspectsTest : IIsolateSystem, IExtendSystem
	{
		public AspectedType Target;
		private static IList<string> startResult;
		private static IList<string> endResult;
		
		[Test]
		public void ShouldRunBeforeMethodBasedOnAttributeOrder()
		{
			Target.DoIt();

			startResult.Should().Have.SameSequenceAs("start1", "start2", "start3");
		}

		[Test]
		public void ShouldRunAfterMethodBasedOnReversedAttributeOrder()
		{
			Target.DoIt();

			endResult.Should().Have.SameSequenceAs("end3", "end2", "end1");
		}

		public class AspectedType
		{
			[Attribute2]
			[Attribute3]
			[Attribute1]
			public virtual void DoIt()
			{
				
			}
		}

		private class Attribute1 : AspectAttribute
		{
			public Attribute1() : base(typeof(Aspect1))
			{
			}

			public override int Order { get { return 1; } }
		}
		public class Aspect1 : IAspect
		{
			public void OnBeforeInvocation(IInvocationInfo invocation)
			{
				startResult.Add("start1");
			}

			public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
				endResult.Add("end1");
			}
		}
		private class Attribute2 : AspectAttribute
		{
			public Attribute2() : base(typeof(Aspect2))
			{
			}
			public override int Order { get { return 2; } }
		}
		public class Aspect2 : IAspect
		{
			public void OnBeforeInvocation(IInvocationInfo invocation)
			{
				startResult.Add("start2");
			}

			public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
				endResult.Add("end2");
			}
		}
		private class Attribute3 : AspectAttribute
		{
			public Attribute3() : base(typeof(Aspect3))
			{
			}
			public override int Order { get { return 3; } }
		}
		public class Aspect3 : IAspect
		{
			public void OnBeforeInvocation(IInvocationInfo invocation)
			{
				startResult.Add("start3");
			}

			public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
				endResult.Add("end3");
			}
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AspectedType>();
			extend.AddService<Aspect1>();
			extend.AddService<Aspect2>();
			extend.AddService<Aspect3>();
		}

		public void Isolate(IIsolate isolate)
		{
			startResult = new List<string>();
			endResult = new List<string>();
		}
	}
}