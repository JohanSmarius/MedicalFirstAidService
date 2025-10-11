using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Staff.DomainServices;

internal class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string? message) : base(message)
    {
    }

    public DomainException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
