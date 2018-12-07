using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public static class EventHandlerRegistrationExtensions
	{
		public static void RegisterEventHandlers(this ContainerBuilder builder, Func<Toggles, bool> toggles, params Assembly[] assemblies)
		{
			builder.RegisterAssemblyTypes(assemblies)
				.Where(t =>
					t.IsEventHandler() &&
					t.EnabledByToggle(toggles) &&
					t.RegisterAsSingleton()
				)
				.As(t =>
					t.HandleInterfaces()
						.Where(x => x.Method?.EnabledByToggle(toggles) ?? true)
						.Select(x => x.Type)
				)
				.AsSelf()
				.SingleInstance()
				.ApplyAspects();

			builder.RegisterAssemblyTypes(assemblies)
				.Where(t =>
					t.IsEventHandler() &&
					t.EnabledByToggle(toggles) &&
					t.RegisterAsLifetimeScope()
				)
				.As(t =>
					t.HandleInterfaces()
						.Where(x => x.Method?.EnabledByToggle(toggles) ?? true)
						.Select(x => x.Type)
				)
				.AsSelf()
				.InstancePerLifetimeScope()
				.ApplyAspects();
		}

		public static bool IsEventHandler(this Type t)
		{
			if (!t.HandleInterfaces().Any())
				return false;

			var runInSync = typeof(IRunInSync).IsAssignableFrom(t);
			var runInSyncInFatClientProcess = typeof(IRunInSyncInFatClientProcess).IsAssignableFrom(t);
			if (!(t.RunsOnHangfire() ^ t.RunsOnStardust() ^ runInSync ^ runInSyncInFatClientProcess))
				throw new Exception($"All event handlers need to implement an IRunOn* interface. {t.Name} does not.");

			return true;
		}

		public static bool RunsOnHangfire(this Type t) =>
			typeof(IRunOnHangfire).IsAssignableFrom(t);

		public static bool RunsOnStardust(this Type t) =>
			typeof(IRunOnStardust).IsAssignableFrom(t);

		public static IEnumerable<HandlerInfo> HandleInterfaces(this Type t)
		{
			foreach (var i in t.GetInterfaces())
			{
				if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>))
				{
					var eventType = i.GetMethods().Single().GetParameters().Single().ParameterType;
					yield return new HandlerInfo
					{
						Type = i,
						Method = t.GetMethod("Handle", new[] {eventType})
					};
				}

				if (i == typeof(IHandleEvents))
				{
					yield return new HandlerInfo
					{
						Type = i,
						Method = t.GetMethod("Handle", typeof(IEnumerable<IEvent>).AsArray())
					};
				}
			}
		}

		public class HandlerInfo
		{
			public Type Type { get; set; }
			public MethodInfo Method { get; set; }
		}

		public static bool RegisterAsSingleton(this Type type)
		{
			return !type.GetCustomAttributes(false)
				.OfType<InstancePerLifetimeScopeAttribute>()
				.Any();
		}

		public static bool RegisterAsLifetimeScope(this Type type)
		{
			return type.GetCustomAttributes(false)
				.OfType<InstancePerLifetimeScopeAttribute>()
				.Any();
		}

	}
}