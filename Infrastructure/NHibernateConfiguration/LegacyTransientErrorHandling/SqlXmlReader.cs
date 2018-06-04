using System;
using System.Data;
using System.Xml;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	internal class SqlXmlReader : XmlReader
	{
		private readonly IDbConnection connection;
		private readonly XmlReader innerReader;

		public SqlXmlReader(IDbConnection connection, XmlReader innerReader)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			if (innerReader == null)
				throw new ArgumentNullException(nameof(innerReader));
			this.connection = connection;
			this.innerReader = innerReader;
		}

		public override int AttributeCount
		{
			get
			{
				return this.innerReader.AttributeCount;
			}
		}

		public override string BaseURI
		{
			get
			{
				return this.innerReader.BaseURI;
			}
		}

		public override int Depth
		{
			get
			{
				return this.innerReader.Depth;
			}
		}

		public override bool EOF
		{
			get
			{
				return this.innerReader.EOF;
			}
		}

		public override bool HasValue
		{
			get
			{
				return this.innerReader.HasValue;
			}
		}

		public override bool IsEmptyElement
		{
			get
			{
				return this.innerReader.IsEmptyElement;
			}
		}

		public override string LocalName
		{
			get
			{
				return this.innerReader.LocalName;
			}
		}

		public override XmlNameTable NameTable
		{
			get
			{
				return this.innerReader.NameTable;
			}
		}

		public override string NamespaceURI
		{
			get
			{
				return this.innerReader.NamespaceURI;
			}
		}

		public override XmlNodeType NodeType
		{
			get
			{
				return this.innerReader.NodeType;
			}
		}

		public override string Prefix
		{
			get
			{
				return this.innerReader.Prefix;
			}
		}

		public override ReadState ReadState
		{
			get
			{
				return this.innerReader.ReadState;
			}
		}

		public override string Value
		{
			get
			{
				return this.innerReader.Value;
			}
		}

		public override string GetAttribute(string name)
		{
			return this.innerReader.GetAttribute(name);
		}

		public override string GetAttribute(int i)
		{
			return this.innerReader.GetAttribute(i);
		}

		public override string GetAttribute(string name, string namespaceUri)
		{
			return this.innerReader.GetAttribute(name, namespaceUri);
		}

		public override void Close()
		{
			if (this.innerReader != null)
				this.innerReader.Close();
			if (this.connection == null)
				return;
			this.connection.Close();
		}

		public override string LookupNamespace(string prefix)
		{
			return this.innerReader.LookupNamespace(prefix);
		}

		public override bool MoveToAttribute(string name, string ns)
		{
			return this.innerReader.MoveToAttribute(name, ns);
		}

		public override bool MoveToAttribute(string name)
		{
			return this.innerReader.MoveToAttribute(name);
		}

		public override bool MoveToElement()
		{
			return this.innerReader.MoveToElement();
		}

		public override bool MoveToFirstAttribute()
		{
			return this.innerReader.MoveToFirstAttribute();
		}

		public override bool MoveToNextAttribute()
		{
			return this.innerReader.MoveToNextAttribute();
		}

		public override bool Read()
		{
			return this.innerReader.Read();
		}

		public override bool ReadAttributeValue()
		{
			return this.innerReader.ReadAttributeValue();
		}

		public override void ResolveEntity()
		{
			this.innerReader.ResolveEntity();
		}
	}
}