using System.Text;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Environment = System.Environment;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class PerformanceTest
	{
		[Test] 
		public void ShouldMapEntityIdToProperty()
		{
			var nhCfg = new Configuration()
				.SetProperty(NHibernate.Cfg.Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName)
				.AddAssembly(typeof(Person).Assembly);

			var errOutput = new StringBuilder();
			foreach (var classMapping in nhCfg.ClassMappings)
			{
				var access = classMapping.IdentifierProperty.PropertyAccessorName;
				if (access.Contains("field"))
				{
					errOutput.AppendLine(classMapping.ClassName + " (access:" + access + ")");
				}
			}
			if (errOutput.Length > 0)
			{
				Assert.Fail("For performance reason, avoid field access on Id getters - choose access='nosetter.camelcase-underscore'. This is not the case on the following entities:"
					+ Environment.NewLine + Environment.NewLine + errOutput);				
			}
		}
	}
}