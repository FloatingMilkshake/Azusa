namespace Azusa.Attributes;

internal class RequireSecretRoleAttribute : ContextCheckAttribute;

internal class RequireSecretRoleContextCheck : IContextCheck<RequireSecretRoleAttribute>
{
    public ValueTask<string> ExecuteCheckAsync(RequireSecretRoleAttribute _, CommandContext ctx) =>
        ValueTask.FromResult(ctx.Member.Roles.Any(r => r.Name == "㊙️")
            ? null
            : "You do not have permission to execute this command.");
}
