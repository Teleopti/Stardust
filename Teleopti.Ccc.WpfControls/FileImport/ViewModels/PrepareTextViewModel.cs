using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.FileImport;
using Teleopti.Ccc.WpfControls.FileImport.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.FileImport.ViewModels
{
    public class PrepareTextViewModel : INotifyPropertyChanged, IModel
    {

        private StringBuilder _stringBuilder;
        private string _streamPath;
        private Encoding _encoding;
        Collection<string> _separatorList = new Collection<string> { ";", "," };
       
        public ICollectionView TimeZoneCollection { private set; get; }
        public ICollectionView EncodingCollection { private set; get; }
        public string Separator { set; get; }


        public PrepareTextViewModel(string streamPath, ICccTimeZoneInfo defaultTimeZone)
        {
            
            TimeZoneCollection = prepareTimeZones(defaultTimeZone);
            TimeZoneCollection.MoveCurrentTo(defaultTimeZone);

            _streamPath = streamPath;
            _encoding = preViewFile(null, out _stringBuilder);

            EncodingCollection = prepareEncodingInfos(_encoding);
            EncodingCollection.MoveCurrentTo(_encoding);
        }



        /// <summary>
        /// Gets or sets the preview text.
        /// </summary>
        /// <value>The preview text.</value>
        /// <remarks>
        /// The first lines for displaying in the gui
        /// </remarks>
        public string PreviewText
        {
            get
            {
                return _stringBuilder.ToString();
            }
           
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Collection<string> SeparatorCollection
        {
            get
            {
                return _separatorList;
            }
        }

        public ICccTimeZoneInfo TimeZone
        {
            get
            {
                return TimeZoneCollection.CurrentItem as ICccTimeZoneInfo;
            }
        }

        public void UpdateEncoding(object selectedItem)
        {
            Encoding info = (Encoding) selectedItem;
            preViewFile(info, out _stringBuilder);
            FirePropertyChanged("PreviewText");
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public IModel NextStep()
        {
            TimeZoneInfo timeZoneInfo = TimeZone.TimeZoneInfoObject as TimeZoneInfo;
            return new PreviewQueueGridViewModel(_streamPath, Separator, timeZoneInfo, _encoding);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void FirePropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
        }

        private Encoding preViewFile(Encoding encoding, out StringBuilder text)
        {
            StreamReader reader;
            Encoding ret;
            text = new StringBuilder();

            if (encoding == null)
                using(reader = new StreamReader(_streamPath, true))
                {
                    readFirstLines(reader, text);
                    ret = reader.CurrentEncoding;
                    //reader.Close();
                }
            else
            {
                using (reader = new StreamReader(_streamPath, encoding, false))
                {
                    readFirstLines(reader, text);
                    ret = encoding;
                    //reader.Close();
                }
                
            }

            return ret;
            
        }

        private static void readFirstLines(StreamReader reader, StringBuilder text)
        {
            for (int i = 0; i < 100; i++)
            {
                if (!reader.EndOfStream)
                    text.AppendLine(reader.ReadLine());
            }
            
        }

        private static ICollectionView prepareTimeZones(ICccTimeZoneInfo defaultTimeZone)
        {
            List<TimeZoneInfo> timeZoneCollection = TimeZoneInfo.GetSystemTimeZones().ToList();
            IList<ICccTimeZoneInfo> selectableTimeZones = new List<ICccTimeZoneInfo>();
            timeZoneCollection.ForEach(t => selectableTimeZones.Add(new CccTimeZoneInfo(t)));
            
            if (!selectableTimeZones.Contains(defaultTimeZone)) 
                selectableTimeZones.Add(defaultTimeZone);
            
            CollectionViewSource viewSource = new CollectionViewSource() { Source = selectableTimeZones };
            return viewSource.View;
            
        }

        private static ICollectionView prepareEncodingInfos(Encoding defaultEncoding)
        {
            EncodingInfo[] encodingInfos = Encoding.GetEncodings();
            List<EncodingInfo> selectableInfos = encodingInfos.OrderBy(e => e.DisplayName).ToList();
            IList<Encoding> selectableEncodings = new List<Encoding>();
            selectableInfos.ForEach(t => selectableEncodings.Add(t.GetEncoding()));

            if (!selectableEncodings.Contains(defaultEncoding))
                selectableEncodings.Add(defaultEncoding);

            CollectionViewSource viewSource = new CollectionViewSource() { Source = selectableEncodings };
            return viewSource.View;
        }
    }
}