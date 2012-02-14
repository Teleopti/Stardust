namespace Teleopti.Ccc.DatabaseConverter
{
    /// <summary>
    /// Two objects connected to each other.
    /// </summary>
    /// <typeparam name="TObject1"></typeparam>
    /// <typeparam name="TObject2"></typeparam>
    public class ObjectPair<TObject1, TObject2>
    {
        private TObject1 _obj1;
        private TObject2 _obj2;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPair&lt;Tobj1, Tobj2&gt;"/> class.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        public ObjectPair(TObject1 obj1, TObject2 obj2)
        {
            _obj1 = obj1;
            _obj2 = obj2;
        }

        /// <summary>
        /// Gets the obj1.
        /// </summary>
        /// <value>The obj1.</value>
        public TObject1 Obj1
        {
            get { return _obj1; }
        }

        /// <summary>
        /// Gets the obj2.
        /// </summary>
        /// <value>The obj2.</value>
        public TObject2 Obj2
        {
            get { return _obj2; }
        }
    }
}