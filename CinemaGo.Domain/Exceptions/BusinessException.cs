using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaGo.Domain.Exceptions
{
    public sealed class BusinessException(string message, int statusCode = 400) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}
