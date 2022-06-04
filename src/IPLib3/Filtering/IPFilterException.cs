using System.Runtime.Serialization;

namespace IPLib3.Filtering; 

[ Serializable ]
public class IPFilterException : Exception {

    public IPFilterException() { }

    public IPFilterException(string message) : base(message) { }

    public IPFilterException(string message, Exception inner) : base(message, inner) { }

    protected IPFilterException(SerializationInfo info, StreamingContext context) : base(info, context) { }

}