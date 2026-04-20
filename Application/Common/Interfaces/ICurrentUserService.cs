namespace Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? CurrentUserId { get; }

        Guid? CurrentTenantId { get; }
    }
}
