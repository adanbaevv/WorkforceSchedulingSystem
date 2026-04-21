namespace Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? CurrentUserId { get; }

        Guid? CurrentTenantId { get; }

        /// <summary>
        /// The role of the current viewer — "Manager" or "Worker". Used for role-aware UI routing and navigation.
        /// </summary>
        string CurrentUserRole { get; }

        /// <summary>
        /// The full name of the current viewer. Used in the navbar greeting and the role switcher label.
        /// </summary>
        string CurrentUserName { get; }
    }
}
