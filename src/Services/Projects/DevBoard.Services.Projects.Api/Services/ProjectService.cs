// ============================================================================
// FILE: Services/Projects/DevBoard.Services.Projects.Api/Services/ProjectService.cs
// ============================================================================
using DevBoard.Services.Projects.Core.Entities;
using DevBoard.Services.Projects.Infrastructure.Data;
using DevBoard.Shared.Common;
using DevBoard.Shared.Contracts.Events;
using DevBoard.Shared.Contracts.Projects;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Services.Projects.Api.Services;

public class ProjectService
{
    private readonly ProjectDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        ProjectDbContext context,
        ITenantProvider tenantProvider,
        IPublishEndpoint publishEndpoint,
        ILogger<ProjectService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProjectDto>>> GetAllAsync()
    {
        try
        {
            var projects = await _context.Projects
                .AsNoTracking()
                .Select(p => new ProjectDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.TenantId
                ))
                .ToListAsync();

            return Result<IEnumerable<ProjectDto>>.Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return Result<IEnumerable<ProjectDto>>.Failure("Failed to retrieve projects");
        }
    }

    public async Task<Result<ProjectDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProjectDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.TenantId
                ))
                .FirstOrDefaultAsync();

            if (project == null)
                return Result<ProjectDto>.Failure($"Project {id} not found");

            return Result<ProjectDto>.Success(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
            return Result<ProjectDto>.Failure("Failed to retrieve project");
        }
    }

    public async Task<Result<ProjectDto>> CreateAsync(CreateProjectRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty)
                return Result<ProjectDto>.Failure("Tenant ID is required");

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                TenantId = tenantId
            };

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            // Publish event
            await _publishEndpoint.Publish(new ProjectCreatedEvent
            {
                TenantId = tenantId,
                ProjectId = project.Id,
                Name = project.Name
            });

            _logger.LogInformation("Project {ProjectId} created for tenant {TenantId}",
                project.Id, tenantId);

            return Result<ProjectDto>.Success(new ProjectDto(
                project.Id,
                project.Name,
                project.Description,
                project.TenantId
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return Result<ProjectDto>.Failure("Failed to create project");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Boards)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return Result.Failure($"Project {id} not found");

            var tenantId = _tenantProvider.GetTenantId();

            // Publish BoardDeleted events for cascade handling in Task Service
            foreach (var board in project.Boards)
            {
                await _publishEndpoint.Publish(new BoardDeletedEvent
                {
                    TenantId = tenantId,
                    BoardId = board.Id
                });
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Project {ProjectId} deleted", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return Result.Failure("Failed to delete project");
        }
    }
}