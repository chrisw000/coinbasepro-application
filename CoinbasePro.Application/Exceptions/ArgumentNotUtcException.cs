using System;
using System.Runtime.Serialization;

namespace CoinbasePro.Application.Exceptions
{
    [Serializable]
    public class ArgumentNotUtcException : ArgumentException
    {
        public DateTime DateTime { get; }
        private DateTimeKind _kind;

        public ArgumentNotUtcException(string paramName, DateTime dateTime) 
            : base($"DateTime needs to be in UTC", paramName)
        {
            DateTime = dateTime;
            _kind = dateTime.Kind;
        }

        /// <summary>Initializes a new instance of the <see cref="T:CoinbasePro.Application.Exceptions.ArgumentException"></see> class with serialized data.</summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ArgumentNotUtcException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _kind = (DateTimeKind)info.GetValue("Kind", typeof(DateTimeKind));
            DateTime = new DateTime(info.GetDateTime("DateTime").Ticks, _kind);
        }
        
        public override string Message
        {
            get
            {
                var s = base.Message;

                var msg = $"received: {DateTime.Kind} on {DateTime:O}";

                return s + Environment.NewLine + msg;
            }
        }

        /// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> object with the parameter name and additional exception information.</summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info">info</paramref> object is a null reference (Nothing in Visual Basic).</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("DateTime", DateTime, typeof(DateTime));
            info.AddValue("Kind", _kind, typeof(DateTimeKind));
        }
    }
}
