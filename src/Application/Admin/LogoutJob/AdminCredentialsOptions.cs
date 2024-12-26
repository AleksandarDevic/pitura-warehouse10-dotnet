namespace Application.Admin.LogoutJob;
public class AdminCredentialsOptions
{
    public const string SectionName = "Admin";

    public required string Secret { get; init; }
}
