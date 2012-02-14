using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Teleopti.Ccc.PayrollFormatter
{
    public class DocumentFormat
    {
        private readonly string _element;
        private readonly DocumentFormatType _documentFormatType;
        private string _itemSeparator = ",";
        private string _rowSeparator = Environment.NewLine;
        private Encoding _encoding = Encoding.Unicode;
        private readonly IList<ItemFormat> _items = new List<ItemFormat>();

        public DocumentFormat(string element, DocumentFormatType documentFormatType)
        {
            _element = element;
            _documentFormatType = documentFormatType;

            HeaderEnabled = false;
            FooterEnabled = false;
            HeaderElement = string.Empty;
            FooterElement = string.Empty;
            ItemsElement = string.Empty;
        }

        public DocumentFormatType DocumentFormatType
        {
            get { return _documentFormatType; }
        }

        public string Element
        {
            get { return _element; }
        }

        public string ItemSeparator
        {
            get { return _itemSeparator; }
            set { _itemSeparator = value; }
        }

        public string RowSeparator
        {
            get { return _rowSeparator; }
            set { _rowSeparator = value; }
        }

        public Encoding Encoding
        {
            get { return _encoding; }
        }

        public bool HeaderEnabled { get; set; }

        public bool FooterEnabled { get; set; }

        public string HeaderElement { get; set; }

        public string FooterElement { get; set; }

        public ReadOnlyCollection<ItemFormat> Items
        {
            get { return new ReadOnlyCollection<ItemFormat>(_items); }
        }

        public string ItemsElement { get; set; }

        public void SetEncoding(string encodingName)
        {
            _encoding = Encoding.GetEncoding(encodingName);
        }

        public void AddItem(ItemFormat itemFormat)
        {
            _items.Add(itemFormat);
        }

        public static DocumentFormat LoadFromXml(IXPathNavigable navigable)
        {
            var navigator = navigable.CreateNavigator();

            XPathNavigator formatNavigator = navigator.SelectSingleNode("//*[local-name() = 'Format']");
            if (formatNavigator == null) return new DocumentFormat("", DocumentFormatType.Xml); //The original format will be used
            if (formatNavigator.IsEmptyElement) throw new ArgumentException("The Format element in supplied XML file is invalid");

            var documentNavigator = formatNavigator.SelectSingleNode("*[local-name() = 'Document']");
            DocumentFormat format = GetDocumentFormat(documentNavigator);

            var itemsNavigator = documentNavigator.SelectSingleNode("*[local-name() = 'Items']");
            if (itemsNavigator!=null)
                ParseItemsFormats(itemsNavigator, format);

            formatNavigator.DeleteSelf();

            return format;
        }

        private static void ParseItemsFormats(XPathNavigator itemsNavigator, DocumentFormat documentFormat)
        {
            string attributeValue = itemsNavigator.GetAttribute("Element", string.Empty);
            if (!string.IsNullOrEmpty(attributeValue))
                documentFormat.ItemsElement = attributeValue;

            var allItems = itemsNavigator.SelectChildren("Item", "urn:payroll-format-1.0");
            while (allItems.MoveNext())
            {
                string type = allItems.Current.GetAttribute("Type", string.Empty);
                string element = allItems.Current.GetAttribute("Element", string.Empty);
                ItemFormat itemFormat = new ItemFormat(element,type);

                attributeValue = allItems.Current.GetAttribute("Format", string.Empty);
                if (!string.IsNullOrEmpty(attributeValue))
                    itemFormat.Format = attributeValue;

                attributeValue = allItems.Current.GetAttribute("Fill", string.Empty);
                if (!string.IsNullOrEmpty(attributeValue))
                    itemFormat.Fill = attributeValue[0];

                attributeValue = allItems.Current.GetAttribute("Quote", string.Empty);
                if (!string.IsNullOrEmpty(attributeValue))
                    itemFormat.Quote = XmlConvert.ToBoolean(attributeValue);

                attributeValue = allItems.Current.GetAttribute("Length", string.Empty);
                if (!string.IsNullOrEmpty(attributeValue))
                    itemFormat.Length = XmlConvert.ToInt16(attributeValue);

                attributeValue = allItems.Current.GetAttribute("Align", string.Empty);
                if (!string.IsNullOrEmpty(attributeValue))
                    itemFormat.Align = (Align) Enum.Parse(typeof(Align), attributeValue,true);

                documentFormat.AddItem(itemFormat);
            }
        }

        private static DocumentFormat GetDocumentFormat(XPathNavigator documentNavigator)
        {
            string attributeValue = documentNavigator.GetAttribute("Type", string.Empty);
            if (!Enum.IsDefined(typeof(DocumentFormatType), attributeValue))
                throw new ArgumentException("The supplied document format type is invalid");

            var documentFormatType = (DocumentFormatType)Enum.Parse(typeof(DocumentFormatType), attributeValue,true);
            attributeValue = documentNavigator.GetAttribute("DocumentElement", string.Empty);

            DocumentFormat format = new DocumentFormat(attributeValue,
                                                       documentFormatType);

            attributeValue = documentNavigator.GetAttribute("Separator", string.Empty);
            if (!string.IsNullOrEmpty(attributeValue))
                format.ItemSeparator = attributeValue;

            attributeValue = documentNavigator.GetAttribute("Line-separator", string.Empty);
            if (!string.IsNullOrEmpty(attributeValue))
                format.RowSeparator = attributeValue;

            attributeValue = documentNavigator.GetAttribute("Encoding", string.Empty);
            if (!string.IsNullOrEmpty(attributeValue))
                format.SetEncoding(attributeValue);

            var headerNavigator = documentNavigator.SelectSingleNode("*[local-name() = 'Header']");
            if (headerNavigator!=null)
            {
                if (XmlConvert.ToBoolean(headerNavigator.GetAttribute("Enabled", string.Empty)))
                {
                    format.HeaderEnabled = true;
                    format.HeaderElement = headerNavigator.GetAttribute("Element", string.Empty);
                }
            }

            var footerNavigator = documentNavigator.SelectSingleNode("*[local-name() = 'Footer']");
            if (footerNavigator != null)
            {
                if (XmlConvert.ToBoolean(footerNavigator.GetAttribute("Enabled", string.Empty)))
                {
                    format.FooterEnabled = true;
                    format.FooterElement = footerNavigator.GetAttribute("Element", string.Empty);
                }
            }
            return format;
        }
    }
}