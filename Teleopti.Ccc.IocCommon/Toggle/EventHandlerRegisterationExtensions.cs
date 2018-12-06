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

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public static class EventHandlerRegisterationExtensions
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

			var runOnHangfire = typeof(IRunOnHangfire).IsAssignableFrom(t);
			var runOnStardust = typeof(IRunOnStardust).IsAssignableFrom(t);
			var runInSync = typeof(IRunInSync).IsAssignableFrom(t);
			var runInSyncInFatClientProcess = typeof(IRunInSyncInFatClientProcess).IsAssignableFrom(t);
			if (!(runOnHangfire ^ runOnStardust ^ runInSync ^ runInSyncInFatClientProcess))
				throw new Exception($"All event handlers need to implement an IRunOn* interface. {t.Name} does not.");

			return true;
		}

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
						Method = t.GetMethod("Handle", new[] { eventType })
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

		public static bool EnabledByToggle(this Type type, IocConfiguration config) => type.EnabledByToggle(config.Toggle);

		public static bool EnabledByToggle(this Type type, Func<Toggles, bool> toggles)
		{
			var attributes = type.GetCustomAttributes(false);
			return innerEnabledByToggle(toggles, attributes);
		}

		public static bool EnabledByToggle(this MethodInfo method, Func<Toggles, bool> toggles)
		{
			var attributes = method.GetCustomAttributes(false);
			return innerEnabledByToggle(toggles, attributes);
		}

		private static bool innerEnabledByToggle(Func<Toggles, bool> toggles, object[] attributes)
		{
			var attributesOn = attributes.OfType<EnabledBy>().FirstOrDefault();
			var attributesOff = attributes.OfType<DisabledBy>().FirstOrDefault();

			if (attributesOn == null && attributesOff == null) return true;

			var resultOn = true;
			var resultOff = true;

			if (attributesOn != null)
			{
				var togglesOn = attributesOn.Toggles;
				resultOn = togglesOn.All(toggles);
			}

			if (attributesOff != null)
			{
				var togglesOff = attributesOff.Toggles;
				resultOff = !togglesOff.Any(toggles);
			}

			return resultOn && resultOff;
		}
	}
}