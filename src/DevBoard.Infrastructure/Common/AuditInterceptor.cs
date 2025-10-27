using DevBoard.Domain.Common;
using DevBoard.Infrastructure.Common.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace DevBoard.Infrastructure.Common
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuditSettings _settings;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor, IOptions<AuditSettings> settings)
        {
            _httpContextAccessor = httpContextAccessor;
            _settings = settings.Value;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedId)
                ? parsedId
                : _settings.FallbackUserId;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is ICreatedByEntity);

            foreach (var entry in entries)
            {
                var entity = (ICreatedByEntity)entry.Entity;
                entity.CreatedAt ??= DateTimeOffset.UtcNow;
                entity.CreatedByUserId ??= userId;
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
