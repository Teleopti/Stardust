

using System;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    /// <summary>
    /// Customer gui class for person object
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-02-22
    /// </remarks>
    public class PersonViewer
    {
        private readonly string _name;
        private readonly String _id;
        private readonly int _imageIndex;



        /// <summary>
        /// Initializes a new instance of the <see cref="PersonViewer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="id">The id.</param>
        /// <param name="imageIndex">Index of the image.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-22
        /// </remarks>
        public PersonViewer(string name, string id, int imageIndex)
        {
            _name = name;
            _id = id;
            _imageIndex = imageIndex;
        }


        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-22
        /// </remarks>
        public string Name
        {
            get { return _name; }   
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-22
        /// </remarks>
        public  string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the index of the image.
        /// </summary>
        /// <value>The index of the image.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-22
        /// </remarks>
        public int ImageIndex
        {
            get { return _imageIndex; }
        }

    }
}