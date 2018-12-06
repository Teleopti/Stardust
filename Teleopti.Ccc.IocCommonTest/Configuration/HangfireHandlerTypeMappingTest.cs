using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	[DomainTest]
	public class HangfireHandlerTypeMappingTest
	{
		public HandlerTypeMapper Mapper;

		[Test]
		public void AllNewHandlersShouldBeMapped()
		{
			var hangfireHandlers =
				from assembly in EventHandlerLocations.Assemblies()
				from type in assembly.GetTypes()
				where type.IsEventHandler()
				where type.RunsOnHangfire()
				select type;

			var failing = hangfireHandlers
				.Select(x =>
				{
					try
					{
						Mapper.NameForPersistence(x);
						return null;
					}
					catch (ArgumentException)
					{
						return $"{x.FullName}, {x.Assembly.GetName().Name}";
					}
				})
				.Where(x => x != null)
				.ToArray();

			if (failing.Any())
				Console.WriteLine("Found a new type that is not mapped! Here's a template for ya'!");
			failing.ForEach(x =>
			{
				var persistedName = new Regex(@"\.([a-zA-Z]+)\,").Match(x).Groups[1];
				Console.WriteLine($@"yield return new mapping
					{{
						CurrentPersistedName = ""{persistedName}"",
						//LegacyPersistedNames = new[] {{""{x}""}},
						CurrentTypeName = ""{x}""
					}};");
			});

			failing.Should().Be.Empty();
		}

		[Test, Ignore("WIP")]
		public void AllRemovedHandlersShouldBeRemovedFromMappings()
		{
		}

		[Test]
		public void AllLegacyTypesShouldWorkToo()
		{
			var type = Mapper.TypeForPersistedName("Teleopti.Ccc.Domain.MessageBroker.Server.MessageBrokerMailboxPurger, Teleopti.Ccc.Domain");
			type.Should().Not.Be.Null();
		}
	}
}