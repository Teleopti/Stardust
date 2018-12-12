using System;
using System.Data;
using System.Data.Common;
using System.Drawing;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Teleopti.Wfm.Adherence.Configuration
{
    public class ColorNumber : IUserType
    {
        public new bool Equals(object x, object y)
        {
            return x.Equals(y); 
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            object dbValue = NHibernateUtil.Int32.NullSafeGet(rs, names, session);
            if(dbValue==null) return null;
            return Color.FromArgb((int)dbValue);
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            NHibernateUtil.Int32.NullSafeSet(cmd, ((Color)value).ToArgb(), index, session);
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public SqlType[] SqlTypes
        {
            get
            {
                SqlType[] types = new SqlType[1];
                types[0] = new SqlType(DbType.Int32);
                return types; 
            }
        }

        public Type ReturnedType
        {
            get { return typeof(Color); }
        }

        public bool IsMutable
        {
            get { return false; }
        }
    }
}