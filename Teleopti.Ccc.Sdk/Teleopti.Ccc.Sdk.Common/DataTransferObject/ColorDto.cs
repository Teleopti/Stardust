using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a ColorDto object.
    /// </summary>
    /// <remarks>Use the Color.FromArgb(Alpha, Red, Green, Blue) to recreate the color.</remarks>
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ColorDto : Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDto"/> class.
        /// </summary>
        /// <param name="color">The color.</param>
        public ColorDto(Color color)
        {
            Alpha = color.A;
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

        private byte _alpha;
        private byte _red;
        private byte _green;
        private byte _blue;

        /// <summary>
        /// Gets or sets the alpha.
        /// </summary>
        /// <value>The alpha.</value>
        [DataMember]
        public byte Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

        /// <summary>
        /// Gets or sets the red.
        /// </summary>
        /// <value>The red.</value>
        [DataMember]
        public byte Red
        {
            get { return _red; }
            set { _red = value; }
        }

        /// <summary>
        /// Gets or sets the green.
        /// </summary>
        /// <value>The green.</value>
        [DataMember]
        public byte Green
        {
            get { return _green; }
            set { _green = value; }
        }

        /// <summary>
        /// Gets or sets the blue.
        /// </summary>
        /// <value>The blue.</value>
        [DataMember]
        public byte Blue
        {
            get { return _blue; }
            set { _blue = value; }
        }

        /// <summary>
        /// Internal method to convert Dto to color.
        /// </summary>
        /// <returns></returns>
        public Color ToColor()
        {
            return Color.FromArgb(Alpha, Red, Green, Blue);
        }
    }
}