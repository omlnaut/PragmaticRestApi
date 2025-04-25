using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.Services;

public class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
{
    public LinkDto CreateLink(string endpointName,
                              string rel,
                              string method,
                              object? values,
                              string? controller = null)
    {
        var href = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext!, action: endpointName, controller: controller, values: values);

        return new LinkDto()
        {
            Href = href ?? throw new ArgumentNullException(nameof(endpointName)),
            Method = method,
            Rel = rel
        };
    }
}