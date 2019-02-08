using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class PersistedTypeMapperForTest : PersistedTypeMapper
	{
		public bool DynamicMappingsForTestProjects = true;
		public bool StaticMappingsForTestProjects = true;

		private IEnumerable<(string persistedName, string typeName)> dynamicMaps = Enumerable.Empty<(string, string)>();

		public IEnumerable<string> AllCurrentTypeNames() =>
			Mappings().Select(x => x.CurrentTypeName);

		protected override string PersistedNameForTypeName(string typeName)
		{
			var persistedName = dynamicMapPersistedNameForTypeName(typeName);
			if (persistedName != null)
				return persistedName;
			return base.PersistedNameForTypeName(typeName);
		}

		protected override string TypeNameForPersistedName(string persistedName)
		{
			var typeName = dynamicMaps.FirstOrDefault(x => x.persistedName == persistedName).typeName;
			if (typeName != null)
				return typeName;
			return base.TypeNameForPersistedName(persistedName);
		}

		private string dynamicMapPersistedNameForTypeName(string typeName)
		{
			if (!DynamicMappingsForTestProjects)
				return null;
			var isTestAssembly = typeName.EndsWith(", Teleopti.Ccc.InfrastructureTest") ||
								 typeName.EndsWith(", Teleopti.Wfm.Administration.IntegrationTest");
			if (!isTestAssembly)
				return null;
			var persistedName = samplePersistedName(typeName);
			dynamicMaps = dynamicMaps.Append((persistedName: persistedName, typeName: typeName)).ToArray();
			return persistedName;
		}

		protected override string ExceptionInfoFor(string typeName) => 
			$@"
yield return new PersistedTypeMapping
{{
	CurrentPersistedName = ""{samplePersistedName(typeName)}"",
	//LegacyPersistedNames = new[] {{""{typeName}""}},
	CurrentTypeName = ""{typeName}""
}};";

		private static string samplePersistedName(string typeName)
		{
			var match = new Regex(@"[.+]([a-zA-Z0-9]+)(\[\])?[,]").Match(typeName);
			if (match.Groups.Count > 1)
				return match.Groups[1] + match.Groups[2].ToString();
			return match.Groups[1].ToString();
		}

		protected override IEnumerable<PersistedTypeMapping> Mappings()
		{
			return base.Mappings()
				.Concat(nonDynamicTestMappings());
		}

		private IEnumerable<PersistedTypeMapping> nonDynamicTestMappings()
		{
			if (!StaticMappingsForTestProjects)
				yield break;

			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventRealPublishingTest+TestHandler",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventRealPublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventRealPublishingTest+TestEvent",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventRealPublishingTest+TestEvent, Teleopti.Ccc.InfrastructureTest"
			};

			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventPublishingTest+TestHandler",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventPublishingTest+TestMultiHandler1",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestMultiHandler1, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventPublishingTest+TestMultiHandler2",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestMultiHandler2, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventPublishingTest+TestBothHangfireHandler",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPublishingTest+TestBothHangfireHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventPackagePublishingTest+TestHandler",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPackagePublishingTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventPackagePublishingTest+TestEvent",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPackagePublishingTest+TestEvent, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireEventPackagePublishingTest+AnotherTestEvent",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireEventPackagePublishingTest+AnotherTestEvent, Teleopti.Ccc.InfrastructureTest"
			};

			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireJobTypeNameChangesTest+MovedEvent",
				LegacyPersistedNames = new[] {"Teleopti.Original.Assembly.Namespace.MovedEventName, Teleopti.Original"},
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireJobTypeNameChangesTest+MovedEvent, Teleopti.Ccc.InfrastructureTest"
			};
			yield return new PersistedTypeMapping
			{
				CurrentPersistedName = "HangfireJobTypeNameChangesTest+TestHandler",
				CurrentTypeName = "Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire.HangfireJobTypeNameChangesTest+TestHandler, Teleopti.Ccc.InfrastructureTest"
			};
		}
	}
}