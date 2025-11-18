// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Api/Controllers/AttachmentsController.cs
// ============================================================================

using DevBoard.Services.Tasks.Infrastructure.Services;
using DevBoard.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Services.Tasks.Api.Controllers;

//[Authorize]
[AllowAnonymous]
[ApiController]
[Route("api/tasks/{taskId}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly AttachmentService _attachmentService;
    private readonly ILogger<AttachmentsController> _logger;
    private readonly ITenantProvider _tenantProvider;

    public AttachmentsController(
        AttachmentService attachmentService,
        ILogger<AttachmentsController> logger,
        ITenantProvider tenantProvider)
    {
        _attachmentService = attachmentService;
        _logger = logger;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Upload an attachment to a task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="file">File to upload</param>
    /// <param name="description">Optional description</param>
    /// <returns>Uploaded attachment details</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadAttachment(
        Guid taskId,
        IFormFile file,
        [FromForm] string? description = null)
    {
        var userId = _tenantProvider.GetTenantId();
        if (userId == Guid.Empty)
            return Unauthorized("User ID not found in token");

        var result = await _attachmentService.UploadTaskAttachmentAsync(
            taskId, file, description, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all attachments for a task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <returns>List of attachments</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTaskAttachments(Guid taskId)
    {
        var result = await _attachmentService.GetTaskAttachmentsAsync(taskId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete an attachment from a task
    /// </summary>
    /// <param name="taskId">Task ID (for route consistency)</param>
    /// <param name="attachmentId">Attachment ID</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAttachment(
        Guid taskId,
        Guid attachmentId)
    {
        var userId = _tenantProvider.GetTenantId();

        if (userId == Guid.Empty)
            return Unauthorized("User ID not found in token");

        var result = await _attachmentService.DeleteAttachmentAsync(
            attachmentId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Attachment deleted successfully" });
    }

    /// <summary>
    /// Get a single attachment by ID (with download URL)
    /// </summary>
    /// <param name="attachmentId">Attachment ID</param>
    /// <returns>Attachment details</returns>
    [HttpGet("{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAttachment(
        Guid attachmentId)
    {
        var result = await _attachmentService.GetAttachmentByIdAsync(attachmentId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

}