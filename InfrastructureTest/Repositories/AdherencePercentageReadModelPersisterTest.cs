using System;
using System.Drawing;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class AdherencePercentageReadModelPersisterTest : IReadModelReadWriteTest<IAdherencePercentageReadModelPersister>
	{
		public IAdherencePercentageReadModelPersister Target { get; set; }

		private static AdherencePercentageReadModel createReadModel(DateOnly dateOnly, Guid personGuid, DateTime lastTimeStamp, int minOutOfAdherence = 8, int minInAdherence = 22)
		{
			//Add tests for timezone
			//we are missing events of when the shift start and shift end
			return new AdherencePercentageReadModel
			{
				Date = dateOnly,
				IsLastTimeInAdherence = true,
				LastTimestamp = lastTimeStamp,
				MinutesInAdherence = minInAdherence,
				MinutesOutOfAdherence = minOutOfAdherence,
				PersonId = personGuid
			};
		}


		[Test]
		public void ShouldBeAbleToSaveReadModelForPerson()
		{
			var personGuid = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);
			var model = createReadModel(dateOnly, personGuid, new DateTime(2012, 8, 29, 8, 0, 0));

			Target.Persist(model);

			var savedModel = Target.Get(dateOnly, personGuid);
			savedModel.BelongsToDate.Should().Be.EqualTo(model.BelongsToDate);
			savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model.IsLastTimeInAdherence);
			savedModel.LastTimestamp.Should().Be.EqualTo(model.LastTimestamp);
			savedModel.MinutesInAdherence.Should().Be.EqualTo(model.MinutesInAdherence);
			savedModel.MinutesOutOfAdherence.Should().Be.EqualTo(model.MinutesOutOfAdherence);
			savedModel.PersonId.Should().Be.EqualTo(model.PersonId);
		}

		[Test]
		public void ShouldBeAbleToSaveReadModelOnDifferentDaysForSamePerson()
		{
			var personGuid = Guid.NewGuid();
			var dateOnly1 = new DateOnly(2012, 8, 29);
			var dateOnly2 = new DateOnly(2012, 8, 30);

			var model1 = createReadModel(dateOnly1, personGuid, new DateTime(2012, 8, 29, 8, 0, 0));
			Target.Persist(model1);

			var model2 = createReadModel(dateOnly2, personGuid, new DateTime(2012, 8, 30, 8, 0, 0));
			Target.Persist(model2);

			var savedModel = Target.Get(dateOnly1, personGuid);
			savedModel.BelongsToDate.Should().Be.EqualTo(model1.BelongsToDate);
			savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model1.IsLastTimeInAdherence);
			savedModel.LastTimestamp.Should().Be.EqualTo(model1.LastTimestamp);
			savedModel.MinutesInAdherence.Should().Be.EqualTo(model1.MinutesInAdherence);
			savedModel.MinutesOutOfAdherence.Should().Be.EqualTo(model1.MinutesOutOfAdherence);
			savedModel.PersonId.Should().Be.EqualTo(model1.PersonId);

			savedModel = Target.Get(dateOnly2, personGuid);
			savedModel.BelongsToDate.Should().Be.EqualTo(model2.BelongsToDate);
			savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model2.IsLastTimeInAdherence);
			savedModel.LastTimestamp.Should().Be.EqualTo(model2.LastTimestamp);
			savedModel.MinutesInAdherence.Should().Be.EqualTo(model2.MinutesInAdherence);
			savedModel.MinutesOutOfAdherence.Should().Be.EqualTo(model2.MinutesOutOfAdherence);
			savedModel.PersonId.Should().Be.EqualTo(model2.PersonId);
		}

		[Test]
		public void ShouldBeAbleToUpdateExistingReadModel()
		{
			var personGuid = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 8, 29);

			var modelOld = createReadModel(dateOnly, personGuid, new DateTime(2014, 8, 29, 8, 0, 0), 9, 21);
			Target.Persist(modelOld);

			var modelUpdated = createReadModel(dateOnly, personGuid, new DateTime(2014, 8, 29, 8, 15, 0), 8, 20);
			Target.Persist(modelUpdated);

			var savedModel = Target.Get(dateOnly, personGuid);
			savedModel.BelongsToDate.Should().Be.EqualTo(modelUpdated.BelongsToDate);
			savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(modelUpdated.IsLastTimeInAdherence);
			savedModel.LastTimestamp.Should().Be.EqualTo(modelUpdated.LastTimestamp);
			savedModel.MinutesInAdherence.Should().Be.EqualTo(modelUpdated.MinutesInAdherence);
			savedModel.MinutesOutOfAdherence.Should().Be.EqualTo(modelUpdated.MinutesOutOfAdherence);
			savedModel.PersonId.Should().Be.EqualTo(modelUpdated.PersonId);
		}

	}

	[ReadModelReadWriteTest]
	public interface IReadModelReadWriteTest<T>
	{
		T Target { get; set; }
	}

	// several attributes would be nice for reuse but nunit cant guarantee exection order
	public class ReadModelReadWriteTestAttribute : Attribute, ITestAction
	{
		private IContainer _container;

		public void BeforeTest(TestDetails testDetails)
		{
			buildContainer();
			configureApplication();
			startReadModelUnitOfWork();
			resolveAndSetTarget(testDetails);
		}

		public void AfterTest(TestDetails testDetails)
		{
			endReadModelUnitOfWork();
			disposeContainer();
		}

		public ActionTargets Targets { get; private set; }


		private void buildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<CommonModule>();
			_container = builder.Build();
		}

		private void disposeContainer()
		{
			_container.Dispose();
		}

		private void configureApplication()
		{
			_container.Resolve<IReadModelUnitOfWorkConfiguration>()
				.Configure(ConnectionStringHelper.ConnectionStringUsedInTests);
		}

		private void startReadModelUnitOfWork()
		{
			var aspect = _container.Resolve<IReadModelUnitOfWorkAspect>();
			aspect.OnBeforeInvokation();
		}

		private void endReadModelUnitOfWork()
		{
			var aspect = _container.Resolve<IReadModelUnitOfWorkAspect>();
			aspect.OnAfterInvokation(null);
		}

		private void resolveAndSetTarget(TestDetails testDetails)
		{
			dynamic fixture = testDetails.Fixture;
			var targetType = testDetails.Fixture.GetType().GetProperty("Target").PropertyType;
			// "as dynamic" is actually required, even though resharper says differently
			fixture.Target = _container.Resolve(targetType) as dynamic;
		}

	}
}

