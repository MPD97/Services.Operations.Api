﻿using System;
using System.Net;
using Convey.WebApi.Exceptions;

namespace Thesis.Services.Operations.Api.Infrastructure
{
    internal sealed class ExceptionToResponseMapper : IExceptionToResponseMapper
    {
        public ExceptionResponse Map(Exception exception)
            => exception switch
            {
                _ => new ExceptionResponse(new {code = "error", reason = "There was an error."},
                    HttpStatusCode.BadRequest)
            };
    }
}