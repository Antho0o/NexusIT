using NexusIT.Data;
using NexusIT.Models;

namespace NexusIT.Services
{
    public class ActivityLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLogService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            string action,
            string description,
            string entityType = "",
            int? entityId = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var performedBy =
                httpContext?.User?.Identity?.IsAuthenticated == true
                    ? httpContext.User.Identity.Name ?? "User"
                    : "System";

            var ipAddress =
                httpContext?.Connection.RemoteIpAddress?.ToString();

            var activity = new ActivityLog
            {
                Action = action,
                Description = description,
                EntityType = entityType,
                EntityId = entityId,
                PerformedBy = performedBy,
                CreatedDate = DateTime.Now,
                IpAddress = ipAddress
            };

            _context.ActivityLogs.Add(activity);

            await _context.SaveChangesAsync();
        }
    }
}