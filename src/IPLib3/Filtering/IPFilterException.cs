namespace IPLib3.Filtering; 

public class IPFilterException : Exception {

    public IPFilterException() { }

    public IPFilterException(string message) : base(message) { }

    public IPFilterException(string message, Exception inner) : base(message, inner) { }

}