using System;

using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevHabit.Api.Middleware;

public class ValidationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                          Exception exception,
                                          CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return new ValueTask<bool>(false);
        }

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Internal Server error",
                Detail = "An error occured processing your request, please try again",
                Status = StatusCodes.Status400BadRequest

            }
        };

        var errors = validationException
            .Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        context.ProblemDetails.Extensions.Add("errors", errors);

        return problemDetailsService.TryWriteAsync(context);

    }
}
