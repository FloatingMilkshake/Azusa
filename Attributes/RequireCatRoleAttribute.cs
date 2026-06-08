namespace Azusa.Attributes;

internal class RequireCatRoleAttribute : ContextCheckAttribute;

internal class RequireCatRoleContextCheck : IContextCheck<RequireCatRoleAttribute>
{
    public ValueTask<string> ExecuteCheckAsync(RequireCatRoleAttribute _, CommandContext ctx) =>
        ValueTask.FromResult(ctx.Member.Roles.Any(r => r.Name == "🐱")
            ? null
            : "You do not have permission to execute this command.");
}
