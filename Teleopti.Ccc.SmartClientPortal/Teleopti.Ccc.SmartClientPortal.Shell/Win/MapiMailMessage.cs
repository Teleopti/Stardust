using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Win
{
    /// <summary>
    /// Represents an email message to be sent through MAPI.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class MapiMailMessage : IMapiMailMessage
    {
        #region Private MapiFileDescriptor Class

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class MapiFileDescriptor
        {
            public int Reserved = 0;
            public int Flags = 0;
            public int Position = 0;
            public string Path = null;
            public string Name = null;
            public IntPtr Type = IntPtr.Zero;
        }

        /// <summary>
        /// Creates the message and adds a range of attachments.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="filePaths">The file paths.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-09-24
        /// </remarks>
        public void CreateMessage(string address, ArrayList filePaths)
        {
            ((RecipientCollection)Recipients).Add(address);
            Files.AddRange(filePaths);
            ShowDialog();
        }
        #endregion Private MapiFileDescriptor Class

        #region Enums

        /// <summary>
        /// Specifies the valid RecipientTypes for a Recipient.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
        public enum RecipientType : int
        {
            /// <summary>
            /// Recipient will be in the TO list.
            /// </summary>
            To = 1,
            /// <summary>
            /// Recipient will be in the CC list.
            /// </summary>
            CC = 2,
            /// <summary>
            /// Recipient will be in the BCC list.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "BCC")]
            BCC = 3
        };

        #endregion Enums

        #region Member Variables

        private string _subject;
        private string _body;
        private RecipientCollection _recipientCollection;
        private ArrayList _files;
        private ManualResetEventSlim _manualResetEvent;

        #endregion Member Variables

        #region Constructors

        /// <summary>
        /// Creates a blank mail message.
        /// </summary>
        public MapiMailMessage()
        {
            _files = new ArrayList();
            _recipientCollection = new RecipientCollection();
            _manualResetEvent = new ManualResetEventSlim(false);
        }

        /// <summary>
        /// Creates a new mail message with the specified Subject and body.
        /// </summary>
        public MapiMailMessage(string subject, string body)
            : this()
        {
            _subject = subject;
            _body = body;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the Subject of this mail message.
        /// </summary>
        public string Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        /// <summary>
        /// Gets or sets the body of this mail message.
        /// </summary>
        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }

        /// <summary>
        /// Gets the recipient list for this mail message.
        /// </summary>
        public IList Recipients
        {
            get { return _recipientCollection; }
        }

        /// <summary>
        /// Gets the file list for this mail message.
        /// </summary>
        public ArrayList Files
        {
            get { return _files; }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Displays the mail message dialog asynchronously.
        /// </summary>
        public void ShowDialog()
        {
            // Create the mail message in an STA thread
            Thread t = new Thread(ShowMail);
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            // only return when the new thread has built it's interop representation
            _manualResetEvent.Wait();
            _manualResetEvent.Reset();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Sends the mail message.
        /// </summary>
        private void ShowMail()
        {
            MAPIHelperInterop.MapiMessage message = new MAPIHelperInterop.MapiMessage();

            using (RecipientCollection.InteropRecipientCollection interopRecipients
                = _recipientCollection.GetInteropRepresentation())
            {
                message.Subject = _subject;
                message.NoteText = _body;

                message.Recipients = interopRecipients.Handle;
                message.RecipientCount = _recipientCollection.Count;

                // Check if we need to add attachments
                if (_files.Count > 0)
                {
                    // Add attachments
                    message.Files = AllocAttachments(out message.FileCount);
                }
                // Signal the creating thread (make the remaining code async)
                _manualResetEvent.Set();

                const int mapiDialog = 0x8;
                //const int MAPI_LOGON_UI = 0x1;
                const int successSuccess = 0;
                int error = MAPIHelperInterop.MAPISendMail(IntPtr.Zero, IntPtr.Zero, message, mapiDialog, 0);

                if (_files.Count > 0)
                {
                    // Deallocate the Files
                    _DeallocFiles(message);
                }

                // Check for error
                if (error != successSuccess)
                {
                    LogErrorMapi(error);
                }
            }
        }

        /// <summary>
        /// Deallocates the Files in a message.
        /// </summary>
        /// <param Name="message">The message to deallocate the Files from.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void _DeallocFiles(MAPIHelperInterop.MapiMessage message)
        {
            if (message.Files != IntPtr.Zero)
            {
                Type fileDescType = typeof(MapiFileDescriptor);
                int fsize = Marshal.SizeOf(fileDescType);

                // Get the ptr to the Files
                int runptr = (int)message.Files;
                // Release each file
                for (int i = 0; i < message.FileCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr)runptr, fileDescType);
                    runptr += fsize;
                }
                // Release the file
                Marshal.FreeHGlobal(message.Files);
            }
        }

        /// <summary>
        /// Allocates the file attachments
        /// </summary>
        /// <param Name="fileCount"></param>
        /// <returns></returns>
        private IntPtr AllocAttachments(out int fileCount)
        {
            fileCount = 0;
            if (_files == null)
            {
                return IntPtr.Zero;
            }
            if ((_files.Count <= 0) || (_files.Count > 100))
            {
                return IntPtr.Zero;
            }

            Type atype = typeof(MapiFileDescriptor);
            int asize = Marshal.SizeOf(atype);
            IntPtr ptra = Marshal.AllocHGlobal(_files.Count * asize);

            MapiFileDescriptor mfd = new MapiFileDescriptor();
            mfd.Position = -1;
            int runptr = (int)ptra;
            for (int i = 0; i < _files.Count; i++)
            {
                string path = _files[i] as string;
                mfd.Name = Path.GetFileName(path);
                mfd.Path = path;
                Marshal.StructureToPtr(mfd, (IntPtr)runptr, false);
                runptr += asize;
            }

            fileCount = _files.Count;
            return ptra;
        }

        /// <summary>
        /// Logs any Mapi errors.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void LogErrorMapi(int errorCode)
        {
            const int mapiUserAbort = 1;
            const int mapiEFailure = 2;
            const int mapiELoginFailure = 3;
            const int mapiEDiskFull = 4;
            const int mapiEInsufficientMemory = 5;
            const int mapiEBlkTooSmall = 6;
            const int mapiETooManySessions = 8;
            const int mapiETooManyFiles = 9;
            const int mapiETooManyRecipients = 10;
            const int mapiEAttachmentNotFound = 11;
            const int mapiEAttachmentOpenFailure = 12;
            const int mapiEAttachmentWriteFailure = 13;
            const int mapiEUnknownRecipient = 14;
            const int mapiEBadReciptype = 15;
            const int mapiENoMessages = 16;
            const int mapiEInvalidMessage = 17;
            const int mapiETextTooLarge = 18;
            const int mapiEInvalidSession = 19;
            const int mapiETypeNotSupported = 20;
            const int mapiEAmbiguousRecipient = 21;
            const int mapiEMessageInUse = 22;
            const int mapiENetworkFailure = 23;
            const int mapiEInvalidEditfields = 24;
            const int mapiEInvalidRecips = 25;
            const int mapiENotSupported = 26;
            const int mapiENoLibrary = 999;
            const int mapiEInvalidParameter = 998;

            string error = string.Empty;
            switch (errorCode)
            {
                case mapiUserAbort:
                    error = "User Aborted.";
                    break;
                case mapiEFailure:
                    error = "MAPI Failure.";
                    break;
                case mapiELoginFailure:
                    error = "Login Failure.";
                    break;
                case mapiEDiskFull:
                    error = "MAPI Disk full.";
                    break;
                case mapiEInsufficientMemory:
                    error = "MAPI Insufficient memory.";
                    break;
                case mapiEBlkTooSmall:
                    error = "MAPI Block too small.";
                    break;
                case mapiETooManySessions:
                    error = "MAPI Too many sessions.";
                    break;
                case mapiETooManyFiles:
                    error = "MAPI too many Files.";
                    break;
                case mapiETooManyRecipients:
                    error = "MAPI too many recipients.";
                    break;
                case mapiEAttachmentNotFound:
                    error = "MAPI Attachment not found.";
                    break;
                case mapiEAttachmentOpenFailure:
                    error = "MAPI Attachment open failure.";
                    break;
                case mapiEAttachmentWriteFailure:
                    error = "MAPI Attachment Write Failure.";
                    break;
                case mapiEUnknownRecipient:
                    error = "MAPI Unknown recipient.";
                    break;
                case mapiEBadReciptype:
                    error = "MAPI Bad recipient Type.";
                    break;
                case mapiENoMessages:
                    error = "MAPI No messages.";
                    break;
                case mapiEInvalidMessage:
                    error = "MAPI Invalid message.";
                    break;
                case mapiETextTooLarge:
                    error = "MAPI Text too large.";
                    break;
                case mapiEInvalidSession:
                    error = "MAPI Invalid session.";
                    break;
                case mapiETypeNotSupported:
                    error = "MAPI Type not supported.";
                    break;
                case mapiEAmbiguousRecipient:
                    error = "MAPI Ambiguous recipient.";
                    break;
                case mapiEMessageInUse:
                    error = "MAPI Message in use.";
                    break;
                case mapiENetworkFailure:
                    error = "MAPI Network failure.";
                    break;
                case mapiEInvalidEditfields:
                    error = "MAPI Invalid edit fields.";
                    break;
                case mapiEInvalidRecips:
                    error = "MAPI Invalid Recipients.";
                    break;
                case mapiENotSupported:
                    error = "MAPI Not supported.";
                    break;
                case mapiENoLibrary:
                    error = "MAPI No Library.";
                    break;
                case mapiEInvalidParameter:
                    error = "MAPI Invalid parameter.";
                    break;
            }

            Debug.WriteLine("Error sending MAPI Email. Error: " + error + " (code = " + errorCode + ").");
        }
        #endregion Private Methods

        #region Private MAPIHelperInterop Class

        /// <summary>
        /// Internal class for calling MAPI APIs
        /// </summary>
        internal class MAPIHelperInterop
        {
            #region Constructors

            /// <summary>
            /// Private constructor.
            /// </summary>
            private MAPIHelperInterop()
            {
                // Intenationally blank
            }

            #endregion Constructors

            #region Constants
            public const int MAPI_LOGON_UI = 0x1;
            #endregion Constants

            #region APIs

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
            public static extern int MAPILogon(IntPtr hwnd, string prf, string pw, int flg, int rsv, ref IntPtr sess);

            #endregion APIs

            #region Structs

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public class MapiMessage
            {
                public int Reserved;
                public string Subject;
                public string NoteText;
                public string MessageType;
                public string DateReceived;
                public string ConversationID;
                public int Flags;
                public IntPtr Originator = IntPtr.Zero;
                public int RecipientCount;
                public IntPtr Recipients = IntPtr.Zero;
                public int FileCount;
                public IntPtr Files = IntPtr.Zero;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public class MapiRecipDesc
            {
                public int Reserved;
                public int RecipientClass;
                public string Name;
                public string Address;
                public int AiDSize;
                public IntPtr EntryID = IntPtr.Zero;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "MapiMessage.DateReceived"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "MapiMessage.Subject"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "MapiMessage.NoteText"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "MapiMessage.MessageType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "MapiMessage.ConversationID"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("MAPI32.DLL")]
            public static extern int MAPISendMail(IntPtr session, IntPtr hwnd, MapiMessage message, int flg, int rsv);

            #endregion Structs
        }

        #endregion Private MAPIHelperInterop Class
    }

    /// <summary>
    /// Represents a Recipient for a MapiMailMessage.
    /// </summary>
    public class Recipient
    {
        #region Public Properties

        /// <summary>
        /// The email address of this recipient.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string Address;

        /// <summary>
        /// The display Name of this recipient.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string DisplayName;

        /// <summary>
        /// How the recipient will receive this message (To, CC, BCC).
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public MapiMailMessage.RecipientType RecipientType = MapiMailMessage.RecipientType.To;

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new recipient with the specified address.
        /// </summary>
        public Recipient(string address)
        {
            Address = address;
        }

        /// <summary>
        /// Creates a new recipient with the specified address and display Name.
        /// </summary>
        public Recipient(string address, string displayName)
        {
            Address = address;
            DisplayName = displayName;
        }

        /// <summary>
        /// Creates a new recipient with the specified address and recipient Type.
        /// </summary>
        public Recipient(string address, MapiMailMessage.RecipientType recipientType)
        {
            Address = address;
            RecipientType = recipientType;
        }

        /// <summary>
        /// Creates a new recipient with the specified address, display Name and recipient Type.
        /// </summary>
        public Recipient(string address, string displayName, MapiMailMessage.RecipientType recipientType)
        {
            Address = address;
            DisplayName = displayName;
            RecipientType = recipientType;
        }

        #endregion Constructors

        #region Internal Methods

        /// <summary>
        /// Returns an interop representation of a recepient.
        /// </summary>
        /// <returns></returns>
        internal MapiMailMessage.MAPIHelperInterop.MapiRecipDesc GetInteropRepresentation()
        {
            MapiMailMessage.MAPIHelperInterop.MapiRecipDesc interop = new MapiMailMessage.MAPIHelperInterop.MapiRecipDesc();

            if (DisplayName == null)
            {
                interop.Name = Address;
            }
            else
            {
                interop.Name = DisplayName;
                interop.Address = Address;
            }

            interop.RecipientClass = (int)RecipientType;

            return interop;
        }

        #endregion Internal Methods
    }

    /// <summary>
    /// Represents a colleciton of recipients for a mail message.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1058:TypesShouldNotExtendCertainBaseTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class RecipientCollection : CollectionBase
    {
        /// <summary>
        /// Adds the specified recipient to this collection.
        /// </summary>
        private void Add(Recipient value)
        {
            List.Add(value);
        }

        /// <summary>
        /// Adds a new recipient with the specified address to this collection.
        /// </summary>
        public void Add(string address)
        {
            Add(new Recipient(address));
        }

        internal InteropRecipientCollection GetInteropRepresentation()
        {
            return new InteropRecipientCollection(this);
        }

        /// <summary>
        /// Struct which contains an interop representation of a colleciton of recipients.
        /// </summary>
        internal struct InteropRecipientCollection : IDisposable
        {
            #region Member Variables

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            private IntPtr _handle;
            private int _count;

            #endregion Member Variables

            #region Constructors

            /// <summary>
            /// Default constructor for creating InteropRecipientCollection.
            /// </summary>
            /// <param Name="outer"></param>
            public InteropRecipientCollection(RecipientCollection outer)
            {
                _count = outer.Count;

                if (_count == 0)
                {
                    _handle = IntPtr.Zero;
                    return;
                }

                // allocate enough memory to hold all recipients
                int size = Marshal.SizeOf(typeof(MapiMailMessage.MAPIHelperInterop.MapiRecipDesc));
                _handle = Marshal.AllocHGlobal(_count * size);

                // place all interop recipients into the memory just allocated
                int ptr = (int)_handle;
                foreach (Recipient native in outer)
                {
                    MapiMailMessage.MAPIHelperInterop.MapiRecipDesc interop = native.GetInteropRepresentation();

                    // stick it in the memory block
                    Marshal.StructureToPtr(interop, (IntPtr)ptr, false);
                    ptr += size;
                }
            }

            #endregion Costructors

            #region Public Properties

            public IntPtr Handle
            {
                get { return _handle; }
            }

            #endregion Public Properties

            #region Public Methods

            /// <summary>
            /// Disposes of resources.
            /// </summary>
            public void Dispose()
            {
                if (_handle != IntPtr.Zero)
                {
                    Type type = typeof(MapiMailMessage.MAPIHelperInterop.MapiRecipDesc);
                    int size = Marshal.SizeOf(type);

                    // destroy all the structures in the memory area
                    int ptr = (int)_handle;
                    for (int i = 0; i < _count; i++)
                    {
                        Marshal.DestroyStructure((IntPtr)ptr, type);
                        ptr += size;
                    }

                    // free the memory
                    Marshal.FreeHGlobal(_handle);

                    _handle = IntPtr.Zero;
                    _count = 0;
                }
            }

            #endregion Public Methods
        }

        
    }
}