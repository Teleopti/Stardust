using System.Linq;
using System.Text;
using NHibernate.Cfg;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Environment = System.Environment;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class PerformanceTest
	{
		private const string correctAccess = "nosetter.camelcase-underscore";

		[Test] 
		public void ShouldMapEntityIdToProperty()
		{
			var nhCfg = new Configuration()
				.SetProperty(NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect")
				.AddAssembly(typeof(Person).Assembly);

			var errOutput = new StringBuilder();
			foreach (var classMapping in nhCfg.ClassMappings.Where(classMapping => !classMapping.IdentifierProperty.PropertyAccessorName.Equals(correctAccess)))
			{
				errOutput.AppendLine(classMapping.ClassName);
			}
			if(errOutput.Length>0)
				Assert.Fail("For performance reason, choose access='" + correctAccess + "' on Id mapping. This is not the case on the following entities." + Environment.NewLine + errOutput);
		}
	}
}