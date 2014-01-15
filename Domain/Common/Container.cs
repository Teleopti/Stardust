using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Base class for containing a class
    /// </summary>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    public class Container<TContent>
    {
        private TContent _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Container{TContent}"/> class.
        /// </summary>
        protected Container()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Container{TContent}"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public Container(TContent content)
        {
            InParameter.NotNull("content", content);
            _content = content;
        }

        /// <summary>
        /// Gets / sets the content.
        /// </summary>
        /// <value>The content.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        protected virtual TContent Content
        {
            get { return _content; }
            set { _content = value; }
        }
    }
}