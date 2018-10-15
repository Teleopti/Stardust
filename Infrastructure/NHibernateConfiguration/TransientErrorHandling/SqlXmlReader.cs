using System;
using System.Data;
using System.Xml;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
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

		public override int AttributeCount => this.innerReader.AttributeCount;

		public override string BaseURI => innerReader.BaseURI;

		public override int Depth => this.innerReader.Depth;

		public override bool EOF => this.innerReader.EOF;

		public override bool HasValue => this.innerReader.HasValue;

		public override bool IsEmptyElement => this.innerReader.IsEmptyElement;

		public override string LocalName => this.innerReader.LocalName;

		public override XmlNameTable NameTable => this.innerReader.NameTable;

		public override string NamespaceURI => this.innerReader.NamespaceURI;

		public override XmlNodeType NodeType => this.innerReader.NodeType;

		public override string Prefix => this.innerReader.Prefix;

		public override ReadState ReadState => this.innerReader.ReadState;

		public override string Value => this.innerReader.Value;

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
			innerReader?.Close();
			connection?.Close();
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