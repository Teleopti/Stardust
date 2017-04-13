using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
	static public class EventHelper
	{
		static Dictionary<Type, HashSet<FieldInfo>> dicEventFieldInfos = new Dictionary<Type, HashSet<FieldInfo>>();

		static BindingFlags AllBindings
		{
			get { return BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static; }
		}

		//private void releaseEvents(Control control)
		//{
		//	return;
		//	//Debug.WriteLine(control.ToString());
		//	EventHelper.RemoveAllEventHandlers(control);

		//	foreach (var subControl in control.Controls)
		//	{

		//		var ctrl = subControl as Control;
		//		if (ctrl != null)
		//		{
		//			Debug.WriteLine("(control.Controls) " + ctrl.Name);
		//			if (ctrl.Name == "ribbonControlAdv1")
		//				releaseEvents(ctrl);
		//		}

		//		var toolStripEx = subControl as ToolStripEx;
		//		if (toolStripEx != null)
		//		{
		//			foreach (var item in toolStripEx.Items)
		//			{
		//				var toolStripPanelItem = item as ToolStripPanelItem;
		//				if (toolStripPanelItem != null)
		//				{
		//					foreach (var item1 in toolStripPanelItem.Items)
		//					{
		//						var comp = item1 as ToolStripItem;
		//						if (comp != null)
		//						{
		//							Debug.WriteLine("(ToolStripItem)" + comp.Name);
		//							EventHelper.RemoveAllEventHandlers(comp);
		//						}
		//					}
		//				}
		//				var itemCtrlHost = item as ToolStripControlHost;
		//				if (itemCtrlHost != null)
		//				{
		//					Debug.WriteLine("(ToolStripControlHost)" + itemCtrlHost.Control);
		//					releaseEvents(itemCtrlHost.Control);
		//				}
		//				else
		//				{
		//					var ctrlItem = item as Control;
		//					if (ctrlItem != null)
		//					{
		//						Debug.WriteLine("(Control)" + ctrlItem);
		//						releaseEvents(ctrlItem);
		//					}
		//					else
		//					{
		//						var component = item as Component;
		//						if (component != null)
		//						{
		//							Debug.WriteLine("(Component)" + component);
		//							EventHelper.RemoveAllEventHandlers(component);
		//						}
		//					}
		//				}
		//			}
		//		}
		//	}
		//}

		//--------------------------------------------------------------------------------
		static HashSet<FieldInfo> GetTypeEventFields(Type t)
		{
			if (dicEventFieldInfos.ContainsKey(t))
				return dicEventFieldInfos[t];

			HashSet<FieldInfo> lst = new HashSet<FieldInfo>();
			BuildEventFields(t, lst);
			dicEventFieldInfos.Add(t, lst);
			return lst;
		}

		//--------------------------------------------------------------------------------
		private static void BuildEventFields(Type t, HashSet<FieldInfo> lst)
		{
			// Type.GetEvent(s) gets all Events for the type AND it's ancestors
			// Type.GetField(s) gets only Fields for the exact type.
			//  (BindingFlags.FlattenHierarchy only works on PROTECTED & PUBLIC
			//   doesn't work because Fieds are PRIVATE)

			// NEW version of this routine uses .GetEvents and then uses .DeclaringType
			// to get the correct ancestor type so that we can get the FieldInfo.
			//foreach (EventInfo ei in t.GetEvents(AllBindings))
			//{
			//	Type dt = ei.DeclaringType;
			//	FieldInfo fi = dt.GetField(ei.Name, AllBindings);
			//	if (fi != null)
			//		lst.Add(fi);
			//}




			// OLD version of the code - called itself recursively to get all fields
			// for 't' and ancestors and then tested each one to see if it's an EVENT
			// Much less efficient than the new code
			foreach (FieldInfo fi in t.GetFields(AllBindings))
			{
				EventInfo ei = null;
				try
				{
					ei = t.GetEvent(fi.Name, AllBindings);
				}
				catch (Exception e)
				{
					Debug.Print(fi.Name + " " + e.Message);
				}
				if (ei != null)
				{
					lst.Add(fi);
					Console.WriteLine(ei.Name);
				}
			}
			if (t.BaseType != null)
				BuildEventFields(t.BaseType, lst);
		}

		//--------------------------------------------------------------------------------
		static EventHandlerList GetStaticEventHandlerList(Type t, object obj)
		{
			MethodInfo mi = t.GetMethod("get_Events", AllBindings);
			return (EventHandlerList)mi.Invoke(obj, new object[] { });
		}

		//--------------------------------------------------------------------------------
		public static void RemoveAllEventHandlers(object obj)
		{
			RemoveEventHandler(obj, "");
		}

		//--------------------------------------------------------------------------------
		public static void RemoveEventHandler(object obj, string EventName)
		{
			if (obj == null)
				return;

			//Component component = obj as Component;
			//if (component != null)
			//{
			//	component.
			//}


			Type t = obj.GetType();
			HashSet<FieldInfo> event_fields = GetTypeEventFields(t);
			EventHandlerList static_event_handlers = null;

			foreach (FieldInfo fi in event_fields)
			{
				Debug.WriteLine("fi.Name " + fi.Name);
				if (EventName != "" && string.Compare(EventName, fi.Name, true) != 0)
					continue;

				// After hours and hours of research and trial and error, it turns out that
				// STATIC Events have to be treated differently from INSTANCE Events...
				if (fi.IsStatic)
				{
					// STATIC EVENT
					if (static_event_handlers == null)
						static_event_handlers = GetStaticEventHandlerList(t, obj);

					object idx = fi.GetValue(obj);
					Delegate eh = static_event_handlers[idx];
					if (eh == null)
						continue;

					Delegate[] dels = eh.GetInvocationList();
					if (dels == null)
						continue;

					//EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					EventInfo ei = null;
					try
					{
						ei = t.GetEvent(fi.Name, AllBindings);
					}
					catch (Exception e)
					{
						Debug.Print(fi.Name + " " + e.Message);
					}
					if (ei != null)
					{
						foreach (Delegate del in dels)
							ei.RemoveEventHandler(obj, del);
					}
				}
				else
				{
					// INSTANCE EVENT
					EventInfo ei = null;
					try
					{
						ei = t.GetEvent(fi.Name, AllBindings);
					}
					catch (Exception e)
					{
						Debug.Print(fi.Name + " " + e.Message);
					}
					if (ei != null)
					{
						object val = fi.GetValue(obj);
						Delegate mdel = (val as Delegate);
						if (mdel != null)
						{
							foreach (Delegate del in mdel.GetInvocationList())
							{
								try
								{
									ei.RemoveEventHandler(obj, del);
								}
								catch (Exception e)
								{
									Debug.Print(fi.Name + " " + e.Message);
								}
							}
						}
					}
				}
			}
		}

	}
}