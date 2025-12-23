using Microsoft.AspNetCore.Mvc;

namespace FullstackProject.Security;

public sealed class RequireApiKeyAttribute : TypeFilterAttribute
{
    // permission = "Read" or "ReadWrite"
    public RequireApiKeyAttribute(string permission) : base(typeof(ApiKeyAuthFilter))
    {
        Arguments = new object[] { permission };
    }
}
