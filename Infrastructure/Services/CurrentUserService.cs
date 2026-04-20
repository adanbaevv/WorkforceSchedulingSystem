using Application.Common.Interfaces;

namespace Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private static readonly Guid AdminUserId = new("11111111-1111-1111-1111-111111111111");
        private static readonly Guid AdminTenantId = new("22222222-2222-2222-2222-222222222222");

        public Guid? CurrentUserId => AdminUserId;

        public Guid? CurrentTenantId => AdminTenantId;
    }
}
