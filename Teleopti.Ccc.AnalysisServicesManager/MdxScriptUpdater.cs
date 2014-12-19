using System;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using System.Runtime.Serialization;

namespace AnalysisServicesManager
{
	public class MdxScriptUpdater : IDisposable
	{
		public enum UpdateType
		{
			DeleteAndInsert,
			DeleteOnly,
			InsertOnly
		};

		private string _marker = "/* Custom member add by Teleopti ASManager */";
		private Server _serverConnection = null;
		private bool _ownedServer = false;
		private List<string> _mdxCommands;

		public string Marker
		{
			get { return this._marker; }
			set { this._marker = value; }
		}

		public Server ServerConnection
		{
			get { return this._serverConnection; }
			set
			{
				if (this._serverConnection != null && _ownedServer)
				{
					try
					{
						this.Dispose();
					}
					finally
					{
						_ownedServer = false;
					}
				}
				this.ServerConnection = value;
				_ownedServer = true;
			}
		}

		public List<string> MdxCommands
		{
			get { return this._mdxCommands; }
			set { this._mdxCommands = value; }
		}

		public MdxScriptUpdater()
		{
			this._mdxCommands = new List<string>();
		}

		public MdxScriptUpdater(string serverName) : this()
		{
			this._serverConnection = new Server();
			this.ServerConnection.Connect(serverName);
		}

		public void Update(string databaseName, string cubeName)
		{
			Update(databaseName, cubeName, UpdateType.InsertOnly);
		}

		public void Update(string databaseName, string cubeName, UpdateType updateType)
		{
			Database db = ServerConnection.Databases[databaseName];
			Cube cube = db.Cubes.FindByName(cubeName);
			Update(cube, updateType);
		}

		public void Update(Cube cube)
		{
			Update(cube, UpdateType.InsertOnly);
		}

		public void Update(Cube cube, UpdateType updateType)
		{
			foreach (MdxScript script in cube.MdxScripts)
			{
				if (script.DefaultScript)
				{
					Update(script, updateType);
					cube.Update();
					return;
				}
			}
			throw new MdxScriptUpdaterException("Default script not found");
		}

		public void Update(MdxScript script, UpdateType updateType)
		{
			if (updateType == UpdateType.DeleteAndInsert || updateType == UpdateType.DeleteOnly)
			{
				DeleteMarkedCommands(script);
			}
			if (updateType == UpdateType.DeleteAndInsert || updateType == UpdateType.InsertOnly)
			{
				InsertMarkedCommands(script);
			}
			script.Update();
		}

		public void DeleteMarkedCommands(MdxScript script)
		{
			DeleteMarkedCommands(script, Marker);
		}

		public static void DeleteMarkedCommands(MdxScript script, string marker)
		{
			for (int i = script.Commands.Count - 1; i >= 0; i--)
			{
				if (script.Commands[i].Text.Contains(marker))
				{
					script.Commands.RemoveAt(i);
				}
			}
		}

		public void InsertMarkedCommands(MdxScript script)
		{
			InsertMarkedCommands(script, this.MdxCommands, Marker);
		}

		public void InsertMarkedCommands(MdxScript script, IEnumerable<string> commands)
		{
			InsertMarkedCommands(script, commands, Marker);
		}

		public void InsertMarkedCommands(MdxScript script, string marker)
		{
			InsertMarkedCommands(script, this.MdxCommands, marker);
		}

		public static void InsertMarkedCommands(MdxScript script, IEnumerable<string> commands, string marker)
		{
			foreach (string mdxCommand in commands)
			{
				Command cmd = new Command();
				cmd.Text = marker + mdxCommand;
				script.Commands.Add(cmd);
			}
		}

		public void Dispose()
		{
			if (this.ServerConnection != null && _ownedServer)
			{
				this.ServerConnection.Disconnect();
				this.ServerConnection.Dispose();
			}
			this._serverConnection = null;
		}
	}

	public class MdxScriptUpdaterException : Exception
	{
		public MdxScriptUpdaterException() : base()
		{
		}

		public MdxScriptUpdaterException(string message) : base(message)
		{
		}

		public MdxScriptUpdaterException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected MdxScriptUpdaterException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}