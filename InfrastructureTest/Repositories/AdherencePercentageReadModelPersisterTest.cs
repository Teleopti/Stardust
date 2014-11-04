﻿using System;
using System.Drawing;
using System.Threading;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class AdherencePercentageReadModelPersisterTest : IReadModelReadWriteTest<IAdherencePercentageReadModelPersister>
	{
		public IAdherencePercentageReadModelPersister Target { get; set; }

		private static AdherencePercentageReadModel createReadModel(DateOnly dateOnly, Guid personGuid, DateTime lastTimeStamp, TimeSpan timeInAdherence, TimeSpan timeOutOfAdherence, int minOutOfAdherence = 8, int minInAdherence = 22)
		{
			//Add tests for timezone
			//we are missing events of when the shift start and shift end
			return new AdherencePercentageReadModel
			{
				Date = dateOnly,
				IsLastTimeInAdherence = true,
				LastTimestamp = lastTimeStamp,
				TimeInAdherence = timeInAdherence,
				TimeOutOfAdherence = timeOutOfAdherence,
				PersonId = personGuid

			};
		}


		[Test]
		public void ShouldBeAbleToSaveReadModelForPerson()
		{
			var personGuid = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);
			var model = createReadModel(dateOnly, personGuid, new DateTime(2012, 8, 29, 8, 0, 0),timeInAdherence,timeOutOfAdherence);

			Target.Persist(model);

			var savedModel = Target.Get(dateOnly, personGuid);
			savedModel.BelongsToDate.Should().Be.EqualTo(model.BelongsToDate);
			savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model.IsLastTimeInAdherence);
			savedModel.LastTimestamp.Should().Be.EqualTo(model.LastTimestamp);
			savedModel.PersonId.Should().Be.EqualTo(model.PersonId);
			savedModel.TimeOutOfAdherence.Should().Be.EqualTo(model.TimeOutOfAdherence);
			savedModel.TimeInAdherence.Should().Be.EqualTo(model.TimeInAdherence);

		}

		[Test]
		public void ShouldBeAbleToSaveReadModelOnDifferentDaysForSamePerson()
		{
			var personGuid = Guid.NewGuid();
			var dateOnly1 = new DateOnly(2012, 8, 29);
			var dateOnly2 = new DateOnly(2012, 8, 30);

			var model1 = createReadModel(dateOnly1, personGuid, new DateTime(2012, 8, 29, 8, 0, 0), TimeSpan.FromMinutes(22), TimeSpan.FromMinutes(47));
			Target.Persist(model1);

			var model2 = createReadModel(dateOnly2, personGuid, new DateTime(2012, 8, 30, 8, 0, 0), TimeSpan.FromMinutes(135), TimeSpan.FromSeconds(55));
			Target.Persist(model2);

			var savedModel = Target.Get(dateOnly1, personGuid);
			savedModel.BelongsToDate.Should().Be.EqualTo(model1.BelongsToDate);
			savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model1.IsLastTimeInAdherence);
			savedModel.LastTimestamp.Should().Be.EqualTo(model1.LastTimestamp);
			savedModel.PersonId.Should().Be.EqualTo(model1.PersonId);
			savedModel.TimeOutOfAdherence.Should().Be.EqualTo(model1.TimeOutOfAdherence);
			savedModel.TimeInAdherence.Should().Be.EqualTo(model1.TimeInAdherence);

			savedModel = Target.Get(dateOnly2, personGuid);
			savedModel.BelongsToDate.Should().Be.EqualTo(model2.BelongsToDate);
			savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model2.IsLastTimeInAdherence);
			savedModel.LastTimestamp.Should().Be.EqualTo(model2.LastTimestamp);
			savedModel.PersonId.Should().Be.EqualTo(model2.PersonId);
			savedModel.TimeOutOfAdherence.Should().Be.EqualTo(model2.TimeOutOfAdherence);
			savedModel.TimeInAdherence.Should().Be.EqualTo(model2.TimeInAdherence);
		}

		[Test]
		public void ShouldBeAbleToUpdateExistingReadModel()
		{
			var personGuid = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);

			var modelOld = createReadModel(dateOnly, personGuid, new DateTime(2014, 8, 29, 8, 0, 0),TimeSpan.Zero,TimeSpan.FromMinutes(8),9, 21);
			Target.Persist(modelOld);

			var modelUpdated = createReadModel(dateOnly, personGuid, new DateTime(2014, 8, 29, 8, 15, 0),timeInAdherence,timeOutOfAdherence, 8, 20);
			Target.Persist(modelUpdated);

			var savedModel = Target.Get(dateOnly, personGuid);
			savedModel.BelongsToDate.Should().Be.EqualTo(modelUpdated.BelongsToDate);
			savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(modelUpdated.IsLastTimeInAdherence);
			savedModel.LastTimestamp.Should().Be.EqualTo(modelUpdated.LastTimestamp);
			savedModel.PersonId.Should().Be.EqualTo(modelUpdated.PersonId);
			savedModel.TimeInAdherence.Should().Be.EqualTo(modelUpdated.TimeInAdherence);
			savedModel.TimeOutOfAdherence.Should().Be.EqualTo(modelUpdated.TimeOutOfAdherence);
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
			builder.RegisterModule(new CommonModule(new IocArgs {DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForTest()}));

			builder.RegisterType<MutableFakeCurrentTeleoptiPrincipal>().AsSelf().As<ICurrentTeleoptiPrincipal>().SingleInstance();

			_container = builder.Build();
			
			var dataSource = _container.Resolve<IDataSourcesFactory>().Create("App", ConnectionStringHelper.ConnectionStringUsedInTests, null);
			_container.Resolve<MutableFakeCurrentTeleoptiPrincipal>()
				.SetPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("_", dataSource, null, null), null));
		}

		private void disposeContainer()
		{
			_container.Dispose();
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

