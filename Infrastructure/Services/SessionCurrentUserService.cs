using Application.Common.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    /// <summary>
    /// Session-backed current-user service for the demo. Reads a chosen employee id from the
    /// ASP.NET session ("currentEmployeeId"). Falls back to the seeded Manager if the session
    /// value is missing or invalid. Caches the resolved employee for the lifetime of the scope
    /// <em>keyed to the stored session id</em>, so a mid-request session change (e.g. right after
    /// a POST to /SwitchUser that does CommitAsync before redirect) causes the cache to miss and
    /// re-resolve fresh instead of returning a stale viewer.
    /// </summary>
    public class SessionCurrentUserService : ICurrentUserService
    {
        private const string SessionKey = "currentEmployeeId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmployeeRepository _employeeRepository;

        private Employee? _cachedEmployee;
        private Guid? _cachedForSessionEmployeeId;
        private bool _cachedForFallback;

        public SessionCurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IEmployeeRepository employeeRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _employeeRepository = employeeRepository;
        }

        public Guid? CurrentUserId => Resolve().Id;

        public Guid? CurrentTenantId => null;

        public string CurrentUserRole => Resolve().Role == EmployeeRole.Manager ? "Manager" : "Worker";

        public string CurrentUserName => Resolve().FullName;

        private Employee Resolve()
        {
            var sessionEmployeeId = ReadSessionEmployeeId();

            // Cache key = the session-stored id (or the "fallback" sentinel). If it changes, re-resolve.
            if (_cachedEmployee != null)
            {
                if (sessionEmployeeId.HasValue && _cachedForSessionEmployeeId == sessionEmployeeId)
                {
                    return _cachedEmployee;
                }

                if (!sessionEmployeeId.HasValue && _cachedForFallback)
                {
                    return _cachedEmployee;
                }
            }

            Employee? employee = null;
            if (sessionEmployeeId.HasValue)
            {
                employee = _employeeRepository.GetByIdAsync(sessionEmployeeId.Value).GetAwaiter().GetResult();
            }

            if (employee == null)
            {
                var all = _employeeRepository.GetAllAsync().GetAwaiter().GetResult();
                employee = all.FirstOrDefault(e => e.Role == EmployeeRole.Manager && e.IsActive)
                          ?? all.FirstOrDefault(e => e.IsActive);

                if (employee == null)
                {
                    throw new InvalidOperationException(
                        "No active employees exist — seed data is broken. Run DbInitializer.");
                }

                _cachedEmployee = employee;
                _cachedForSessionEmployeeId = null;
                _cachedForFallback = true;
                return employee;
            }

            _cachedEmployee = employee;
            _cachedForSessionEmployeeId = sessionEmployeeId;
            _cachedForFallback = false;
            return employee;
        }

        private Guid? ReadSessionEmployeeId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return null;
            }

            try
            {
                var stored = session.GetString(SessionKey);
                if (!string.IsNullOrWhiteSpace(stored) && Guid.TryParse(stored, out var parsed))
                {
                    return parsed;
                }
            }
            catch (InvalidOperationException)
            {
                // Session not loaded yet on this request (e.g. a pipeline stage before UseSession).
            }

            return null;
        }
    }
}
