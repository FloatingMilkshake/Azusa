namespace Azusa.Attributes;

internal class SecretAttribute : ContextCheckAttribute;

internal class SecretContextCheck : IContextCheck<SecretAttribute>
{
    public ValueTask<string> ExecuteCheckAsync(SecretAttribute _, CommandContext ctx) =>
        ValueTask.FromResult(ctx.Member.Roles.Any(r => r.Name == "㊙️")
            ? null
            : "You do not have permission to execute this command.");
}
