namespace OddDotNet;

[Serializable]
public class OddException : Exception
{
    public OddException()
    {
    }

    public OddException(string message) : base(message)
    {
    }

    public OddException(string message, Exception innerException) : base(message, innerException)
    {
    }
}