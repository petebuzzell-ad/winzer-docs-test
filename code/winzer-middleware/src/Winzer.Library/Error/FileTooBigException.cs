namespace Winzer.Library.Error;

/// <summary>
/// An exception thrown when a file is too big for processing
/// </summary>
public class FileTooBigException : Exception
{
    /// Default Exception Constructors
    public FileTooBigException()
        : base() { }
    public FileTooBigException(string? message)
        : base(message) { }
    public FileTooBigException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
