// ============================================================================
// FILE 1: Infrastructure/Services/ProjectService.cs
// ============================================================================
using DevBoard.Application.Common;
using DevBoard.Application.Dtos;
using DevBoard.Application.Events;
using DevBoard.Application.Interfaces;
using DevBoard.Application.Services;
using DevBoard.Domain.Entities;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IEventPublisher eventPublisher,
            ILogger<ProjectService> logger)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _eventPublisher = eventPublisher;
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
                        p.TenantId.ToString()
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
                        p.TenantId.ToString()
                    ))
                    .FirstOrDefaultAsync();

                if (project is null)
                    return Result<ProjectDto>.Failure($"Project with ID {id} not found");

                return Result<ProjectDto>.Success(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
                return Result<ProjectDto>.Failure("Failed to retrieve project");
            }
        }

        public async Task<Result<ProjectDto>> CreateAsync(CreateProjectDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tenantId = _tenantProvider.GetTenantId();

                if (tenantId == Guid.Empty)
                    return Result<ProjectDto>.Failure("Tenant ID is required");

                var projectId = Guid.NewGuid();
                var project = new Project
                {
                    Id = projectId,
                    Name = dto.Name,
                    Description = dto.Description,
                    TenantId = Guid.Parse(dto.TenantId)
                };

                await _context.Projects.AddAsync(project);
                await _context.SaveChangesAsync();

                // Publish event (fire-and-forget)
                await _eventPublisher.PublishAsync(new ProjectCreatedEvent
                {
                    ProjectId = project.Id,
                    Name = project.Name,
                    TenantId = project.TenantId
                });

                await transaction.CommitAsync();

                var result = new ProjectDto(
                    project.Id,
                    project.Name,
                    project.Description,
                    project.TenantId.ToString()
                );

                _logger.LogInformation("Project {ProjectId} created for tenant {TenantId}", projectId, tenantId);

                return Result<ProjectDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return Result<ProjectDto>.Failure("Failed to create project");
            }
        }

        public async Task<Result<ProjectWithBoardsDto>> GetProjectWithBoardsAsync(Guid projectId)
        {
            try
            {
                var project = await _context.Projects
                    .AsNoTracking()
                    .Where(p => p.Id == projectId)
                    .Select(p => new ProjectWithBoardsDto(
                        p.Id,
                        p.Name,
                        p.Description,
                        p.TenantId.ToString(),
                        p.Boards.Select(b => new BoardWithTasksDto(
                            b.Id,
                            b.Name,
                            b.Description,
                            b.TenantId,
                            b.Tasks.Select(t => new TaskItemResponseDto(
                                t.Id,
                                t.Name,
                                t.Description,
                                (int)t.Status,
                                t.DueDate,
                                t.BoardId,
                                t.TenantId
                            ))
                        ))
                    ))
                    .FirstOrDefaultAsync();

                if (project is null)
                    return Result<ProjectWithBoardsDto>.Failure($"Project with ID {projectId} not found");

                return Result<ProjectWithBoardsDto>.Success(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with boards {ProjectId}", projectId);
                return Result<ProjectWithBoardsDto>.Failure("Failed to retrieve project with boards");
            }
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);

                if (project is null)
                    return Result.Failure($"Project with ID {id} not found");

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
}