using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CoinbasePro.Application.Exceptions;
using NUnit.Framework;

namespace CoinbasePro.Application.Test.Exceptions
{
    [TestFixture]
    public class ArgumentNotUtcExceptionTest
    {
        private ArgumentNotUtcException _subject;
        private readonly DateTime _d = new DateTime(2019, 4, 3, 13, 09, 15, DateTimeKind.Local);
        private const string _pName = "pName to test for";

        [SetUp]
        public void SetUp()
        {
            _subject = new ArgumentNotUtcException(_pName, _d);
        }

        [Test]
        public void EnsureDateTime()
        {
            Assert.That(_subject.DateTime, Is.EqualTo(_d));
        }

        [Test]
        public void EnsureParamName()
        {
            Assert.That(_subject.ParamName, Is.EqualTo(_pName));
        }

        [Test]
        public void EnsureMessage()
        {
            Assert.That(_subject.Message, Is.EqualTo("DateTime needs to be in UTC\r\nParameter name: pName to test for\r\nreceived: Local on 2019-04-03T13:09:15.0000000+01:00"));
        }

        [Test]
        public void EnsureSerialize()
        {
            var mem = new MemoryStream();
            var b = new BinaryFormatter();

            b.Serialize(mem, _subject);
            mem.Position = 0;

            var copy = (ArgumentNotUtcException)b.Deserialize(mem);

            Assert.That(_subject.DateTime, Is.EqualTo(copy.DateTime));            
            Assert.That(_subject.Message, Is.EqualTo(copy.Message));
        }
    }
}
