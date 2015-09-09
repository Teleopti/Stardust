using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.WinCode.Common.ExceptionHandling
{
	public class ExceptionMessageBuilder
	{
		private readonly Exception _exception;
		private readonly ITogglesActive _toggles;
		public const string ToggleFeaturesUnknown = "ToggleFeatures unknown";

		public ExceptionMessageBuilder(Exception exception, ITogglesActive toggles)
		{
			_exception = exception;
			_toggles = toggles;
		}

		public string BuildSimpleExceptionMessage()
		{
			return simpleExceptionMessage(_exception);
		}

		public string BuildCompleteExceptionMessage()
		{
			return completeExceptionMessage(_exception, _toggles);
		}

		private static string simpleExceptionMessage(Exception exception)
		{
			var builder = new StringBuilder();

			builder.AppendLine("Unhandled application exception occured.");
			builder.AppendLine("Timestamp: (UTC)" + DateTime.UtcNow + " (local)" + DateTime.Now);
	
			//Note: SQL exceptions are different, we need to loop the Error collection
			//to get all the information.
			var sqlException = exception as SqlException;
			if (sqlException == null)
			{
				builder.Append(exception);
			}
			else
			{
				appendSqlException(builder, sqlException);
			}

			appendVersionInfo(builder);
			return builder.ToString();
		}


		private static string completeExceptionMessage(Exception exception, ITogglesActive toggles)
		{
			var builder = new StringBuilder();

			builder.AppendLine("Unhandled application exception occured.");
			builder.AppendLine("Timestamp: (UTC)" + DateTime.UtcNow + " (local)" + DateTime.Now);
			

			//Note: SQL exceptions are different, we need to loop the Error collection
			//to get all the information.
			var sqlException = exception as SqlException;
			if (sqlException == null)
			{
				builder.Append(exception);
			}
			else
			{
				appendSqlException(builder, sqlException);
			}

			appendAssemblyInfo(builder);
			appendVersionInfo(builder);
			appendFeatureInfo(builder, toggles);

			return builder.ToString();
		}

		private static void appendAssemblyInfo(StringBuilder builder)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				builder.AppendLine(assembly.ToString());
			}
		}

		private static void appendVersionInfo(StringBuilder text)
		{
			text.AppendLine();
			text.Append("Product Version: ");
			text.Append(Application.ProductVersion);
			text.AppendLine();
			text.AppendLine();
		}

		private static void appendFeatureInfo(StringBuilder text, ITogglesActive toggles)
		{
			if(toggles == null)
				return;
			text.AppendLine();
			text.AppendLine();
			try
			{

				foreach (var entry in toggles.AllActiveToggles())
				{
					text.AppendLine(string.Format("{0} = {1} ", entry.Key, entry.Value));
				}
			}
			catch (Exception)
			{
				text.AppendLine(ToggleFeaturesUnknown);
			}
		}

		private static void appendSqlException(StringBuilder text, SqlException sqlException)
		{
			foreach (SqlError error in sqlException.Errors)
			{
				text.AppendLine(error.ToString());
			}
		}
	}
}
