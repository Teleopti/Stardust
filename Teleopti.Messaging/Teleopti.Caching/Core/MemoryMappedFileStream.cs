using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Permissions;

namespace Teleopti.Caching.Core
{
    public class MemoryMappedFileStream : UnmanagedMemoryStream
    {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        private IntPtr _fileHandle;
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        private IntPtr _mappingHandle;
        private const string NeedNonnegativeNumber = "Value can not be negative.";
        private const string MustBeSmallerThanInt32Max = "Value must be less than or equal to Int32.MaxValue.";
        private const string MemoryMappedFileLengthZero = "Files used for memory mapping must be longer than 0 bytes.";

        public unsafe MemoryMappedFileStream(string fileName, FileAccess access, long offsetIntoFile, long length, string optionalMappingName)
        {
            _fileHandle = UnsafeNativeMethods.InvalidHandleValue;
            _mappingHandle = IntPtr.Zero;
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (offsetIntoFile < 0L)
            {
                throw new ArgumentOutOfRangeException("offsetIntoFile", NeedNonnegativeNumber);
            }
            if (length < 0L)
            {
                throw new ArgumentOutOfRangeException("length", NeedNonnegativeNumber);
            }
            if (length > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("length", length, MustBeSmallerThanInt32Max);
            }
            string fullPath = Path.GetFullPath(fileName);
            int dwDesiredAccess = 0;
            int dwProtect = 0;
            FileIOPermissionAccess noAccess = FileIOPermissionAccess.NoAccess;
            bool canRead = (access & FileAccess.Read) != 0;
            bool canWrite = (access & FileAccess.Write) != 0;
            if (canRead)
            {
                dwDesiredAccess = -2147483648;
                dwProtect = 2;
                noAccess = FileIOPermissionAccess.Read;
            }
            if (canWrite)
            {
                dwDesiredAccess |= 0x40000000;
                dwProtect = 4;
                noAccess |= FileIOPermissionAccess.Write;
            }
            new FileIOPermission(noAccess, fullPath).Demand();
            _fileHandle = UnsafeNativeMethods.CreateFile(fullPath, dwDesiredAccess, FileShare.ReadWrite, IntPtr.Zero, 4, 0x80, IntPtr.Zero);
            if (_fileHandle == UnsafeNativeMethods.InvalidHandleValue)
            {
                CachingError.WinIOError(fileName);
            }
            if (UnsafeNativeMethods.GetLengthOfFile(_fileHandle) == 0L)
            {
                if (length == 0L)
                {
                    UnsafeNativeMethods.CloseHandle(_fileHandle);
                    _fileHandle = UnsafeNativeMethods.InvalidHandleValue;
                    throw new ArgumentException(MemoryMappedFileLengthZero);
                }
                int hi = (int)(length >> 0x20);
                if (!UnsafeNativeMethods.SetFilePointer(_fileHandle, (int)length, ref hi, SeekOrigin.Begin))
                {
                    CachingError.WinIOError(fileName);
                }
                if (!UnsafeNativeMethods.SetEndOfFile(_fileHandle))
                {
                    CachingError.WinIOError(fileName);
                }
                hi = 0;
                UnsafeNativeMethods.SetFilePointer(_fileHandle, 0, ref hi, SeekOrigin.Begin);
            }
            _mappingHandle = UnsafeNativeMethods.CreateFileMapping(_fileHandle, IntPtr.Zero, dwProtect, (int)(length >> 0x20), (int)length, optionalMappingName);
            if (_mappingHandle == IntPtr.Zero)
            {
                CachingError.WinIOError(fileName);
            }
            long lengthOfFile = UnsafeNativeMethods.GetLengthOfFile(_fileHandle);
            long capacity = lengthOfFile - offsetIntoFile;
            if (length == 0L)
            {
                length = lengthOfFile - offsetIntoFile;
                if (length < 0L)
                {
                    throw new ArgumentOutOfRangeException("length", NeedNonnegativeNumber);
                }
            }
            byte* mem = UnsafeNativeMethods.MapViewOfFile(_mappingHandle, (int)access, (int)(offsetIntoFile >> 0x20), (int)offsetIntoFile, (int)length);
            if (mem == null)
            {
                CachingError.WinIOError(fileName);
            }
            Initialize(mem, length, capacity, FileAccess.ReadWrite);
        }

        protected override void Dispose(bool disposing)
        {
            if (_fileHandle != UnsafeNativeMethods.InvalidHandleValue)
            {
                if (_mappingHandle != IntPtr.Zero)
                {
                    if (!UnsafeNativeMethods.CloseHandle(_mappingHandle))
                    {
                        CachingError.WinIOError();
                    }
                    _mappingHandle = IntPtr.Zero;
                }
                if (!UnsafeNativeMethods.CloseHandle(_fileHandle))
                {
                    CachingError.WinIOError();
                }
                _fileHandle = UnsafeNativeMethods.InvalidHandleValue;
            }
            base.Dispose(disposing);
        }

        ~MemoryMappedFileStream()
        {
            Dispose(false);
        }
    }
}