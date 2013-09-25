using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
		[IsNotDeadCode("Used in NH mapping files.")]
    public class XmlType : IUserType
    {
        public new bool Equals(object x, object y)
        {
            if (x == null || y == null)
                return false;

            var xdoc_x = (IXPathNavigable)x;
            var xdoc_y = (IXPathNavigable)y;
            return xdoc_y.CreateNavigator().OuterXml == xdoc_x.CreateNavigator().OuterXml;
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            if (names.Length != 1)
                throw new InvalidOperationException("names array has more than one element. can't handle this!");

            var val = rs[names[0]] as string;
            if (val != null)
            {
                var document = new XmlDocument();
                document.LoadXml(val);
                return document;
            }

            return null;
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            string setValue = null;
            if (value != null)
                setValue = ((IXPathNavigable) value).CreateNavigator().OuterXml;

            NHibernateUtil.String.NullSafeSet(cmd, setValue, index);
        }

        public object DeepCopy(object value)
        {

            var toCopy = value as IXPathNavigable;

            if (toCopy == null)
                return null;

            var copy = new XmlDocument();
            copy.LoadXml(toCopy.CreateNavigator().OuterXml);
            return copy;
        }

        public object Replace(object original, object target, object owner)
        {
            throw new NotImplementedException();
        }

        public object Assemble(object cached, object owner)
        {
            var str = cached as string;
            if (str != null)
            {
                var doc = new XmlDocument();
                doc.LoadXml(str);
                return doc;
            }
            
            return null;
        }

        public object Disassemble(object value)
        {
            var val = value as IXPathNavigable;
            if (val != null)
            {
                return val.CreateNavigator().OuterXml;
            }
            
            return null;
        }

        public SqlType[] SqlTypes
        {
            get
            {
                return new SqlType[] { new StringSqlType(1073741823) };
            }
        }

        public Type ReturnedType
        {
            get { return typeof(IXPathNavigable); }
        }

        public bool IsMutable
        {
            get { return true; }
        }
    }
}