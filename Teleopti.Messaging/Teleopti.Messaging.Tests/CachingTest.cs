using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Teleopti.Caching.Core;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Tests
{

    [TestFixture]
    public class CachingTest
    {

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void CachingItemAddTests()
        {
            ICache cache = new Cache("Teleopti.Messaging.Caching1");
            Hashtable ht = new Hashtable { { "Peter", "Ankarlou" } };
            cache.Add("Names", ht);
            cache.Dispose();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CacheNameIsStringEmtpyTests()
        {
            Assert.Throws(typeof(ArgumentNullException), delegate
                                                             {
                                                                 ICache cache = new Cache(String.Empty);
                                                                 Hashtable ht = new Hashtable { { "Peter", "Ankarlou" } };
                                                                 cache.Add("Names", ht);
                                                                 cache.Dispose();
                                                             });
        }

        [Test, ExpectedException("System.ArgumentNullException")]
        public void CachingAddNullTests()
        {
            Assert.Throws(typeof(ArgumentNullException), delegate
                                                 {

                                                     ICache cache = new Cache("Teleopti.Messaging.Caching2");
                                                     Hashtable ht = new Hashtable { { "Peter", "Ankarlou" } };
                                                     cache.Add(null, ht);
                                                     cache.Dispose();
                                                 });
        }


        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void CachingAddNullObjectTests()
        {
            Assert.Throws(typeof(ArgumentNullException), delegate
                                     {

                                         ICache cache = new Cache("Teleopti.Messaging.Caching3");
                                         cache.Add("Peter", null);
                                         cache.Dispose();
                                     });
        }


        [Test, ExpectedException(typeof(ArgumentException))]
        public void CachingAddNonHashtableTests()
        {
            Assert.Throws(typeof(ArgumentException), delegate
                                     {

                                         ICache cache = new Cache("Teleopti.Messaging.Caching4");
                                         cache.Add("Peter", new object());
                                         cache.Dispose();
                                     });
        }

        [Test]
        public void CachingItemFetchTests()
        {
            Cache cache = new Cache("Teleopti.Messaging.Caching5");
            Hashtable ht = new Hashtable { { "Peter", "Ankarlou" } };
            cache.Add("Names", ht);
            Hashtable htNew = (Hashtable)cache["Names"];
            //Console.WriteLine(htNew["Peter"].ToString());
        }

        [Test]
        public void CachingItemRemoveTests()
        {
            ICache cache = new Cache("Teleopti.Messaging.Caching6");
            cache.Remove("Names");
            //Console.WriteLine("Successfully removed names from cache ...");
        }

        [Test]
        public void CachingItemIsNullTests()
        {
            ICache cache = new Cache("Teleopti.Messaging.Caching");
            Hashtable ht = (Hashtable)cache["Peppe"];
            Assert.IsNull(ht);
        }


        [Test, Ignore("This is a stress test. Run this test manually. ( < 1 Hour )")]
        public void CachingSizeTests()
        {
            ICache cache = new Cache("Teleopti.Messaging.Caching");
            Hashtable ht = (Hashtable)cache.Items("Names");
            for (int count = 0; count < Cache.MegaByte * 5; count++)
            {
                cache.Add(String.Format(CultureInfo.InvariantCulture, "Names{0}", count), ht);
                if (SerializeObject(ht).Capacity > Cache.MegaByte)
                {
                    Debug.WriteLine(SerializeObject(ht).Capacity.ToString());
                    break;
                }
            }
            //Console.WriteLine("Successfully tested size of cache ...");
        }

        public MemoryStream SerializeObject(object o)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            // serialize entire object graph into stream (memoryStream)
            formatter.Serialize(stream, o);
            // position beginning of stream
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        [Test]
        public void CachingItemAddToBrandNewFileTests()
        {
            ICache cache = new Cache(Guid.NewGuid().ToString().Replace("-", ""));
            Hashtable ht = new Hashtable { { "Peter", "Ankarlou" } };
            cache.Add("Names", ht);
        }


        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void MemoryMappedFileNameIsNullTests()
        {
            Assert.Throws(typeof(ArgumentNullException), delegate
                                                              {
                                                                  MemoryMappedFileStream mmf =
                                                                      new MemoryMappedFileStream(null,
                                                                                                 FileAccess.
                                                                                                     ReadWrite,
                                                                                                 0, 255,
                                                                                                 "Peter");
                                                              });
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MemoryMappedFileLengthIsNegativeTests()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), delegate
            {
                MemoryMappedFileStream mmf = new MemoryMappedFileStream("Peppe", FileAccess.ReadWrite, 0, -1, "Peter");
            });
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MemoryMappedFileLengthIsTooLongTests()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), delegate
            {
                MemoryMappedFileStream mmf = new MemoryMappedFileStream("Peppe", FileAccess.ReadWrite, 0, long.MaxValue, "Peter");
            });
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MemoryMappedOffsetIsNegativeTests()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), delegate
            {
                MemoryMappedFileStream mmf = new MemoryMappedFileStream("Peppe", FileAccess.ReadWrite, -1, 255, "Peter");
            });
        }

        [Test, ExpectedException(typeof(IOException))]
        public void MemoryMappedOffsetGreaterThanLengthTests()
        {
            Assert.Throws(typeof(IOException), delegate
            {
                MemoryMappedFileStream mmf = new MemoryMappedFileStream("Peppe", FileAccess.ReadWrite, 255, 25, "Peter");
            });
        }

        [Test, ExpectedException(typeof(IOException))]
        public void MemoryMappedOffsetEqualToLengthTests()
        {
            Assert.Throws(typeof(IOException), delegate
             {
                 MemoryMappedFileStream mmf = new MemoryMappedFileStream("Peppe", FileAccess.ReadWrite, 255, 255, "Peter");
             });
        }

        [Test, ExpectedException(typeof(IOException))]
        public void CachingExceptionTests()
        {
            Assert.Throws(typeof(IOException), CachingError.WinIOError);
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void CachingExceptionErrorCodeTwoTests()
        {
            Assert.Throws(typeof(FileNotFoundException), delegate
            {
                CachingError.WinIOError("Peppe", 2);
            });
        }

        [Test, ExpectedException(typeof(DirectoryNotFoundException))]
        public void CachingExceptionErrorCodeThreeTests()
        {
            Assert.Throws(typeof(DirectoryNotFoundException), delegate { CachingError.WinIOError("Peppe", 3); });
        }

        [TearDown]
        public void TearDown()
        {
        }

    }

}
