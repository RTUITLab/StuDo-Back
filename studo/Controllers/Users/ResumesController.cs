using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using studo.Data;
using studo.Models;
using studo.Models.Requests.Users;
using studo.Models.Responses.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Controllers.Users
{
    /// <summary>
    /// Controller for making operations with resums:
    /// Create, edit, delete, get all, get one, get user's only
    /// </summary>
    [Produces("application/json")]
    [Route("api/resumes")]
    [ApiController]
    public class ResumesController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly ILogger<ResumesController> logger;
        private readonly DatabaseContext dbContext;

        public ResumesController(UserManager<User> userManager, IMapper mapper,
            ILogger<ResumesController> logger, DatabaseContext dbContext)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Get all resumes
        /// </summary>
        /// <returns>All resumes</returns>
        /// <response code="200">All is okay</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<CompactResumeView>>> GetAllResumesAsync()
        {
            if (await dbContext.Resumes.CountAsync() == 0)
                return Ok(new List<CompactResumeView>());

            return Ok(await dbContext.Resumes
                .ProjectTo<CompactResumeView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        /// <summary>
        /// Get one resume by id
        /// </summary>
        /// <param name="resumeId"></param>
        /// <returns>One resume</returns>
        /// <response code="200">All is okay</response>
        /// <response code="404">When resume with passed id doesn't exist</response>
        [HttpGet("{resumeId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ResumeView>> GetOneResumeAsync(Guid resumeId)
        {
            ResumeView resumeView = await dbContext.Resumes
                .ProjectTo<ResumeView>(mapper.ConfigurationProvider)
                .SingleAsync(res => res.Id == resumeId);

            if (resumeView == null)
            {
                logger.LogDebug($"Can't find resume {resumeId}");
                return NotFound("Can't find resume");
            }

            return Ok(resumeView);
        }

        /// <summary>
        /// Get all user's resumes
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>All user's resumes</returns>
        /// <response code="200">All is okay</response>
        /// <response code="404">User with passed id doesn't exist</response>
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<CompactResumeView>>> GetAllUsersResumesAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                logger.LogDebug($"Can't find user {userId}");
                return NotFound("Can't find user");
            }

            var resumes = await dbContext.Resumes
                .Where(res => res.UserId == userId)
                .ProjectTo<CompactResumeView>(mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(resumes);
        }

        /// <summary>
        /// Create resume
        /// </summary>
        /// <param name="createResumeRequest"></param>
        /// <returns>Created resume</returns>
        /// <response code="200">Resume is created</response>
        /// <response code="400">Access token isn't correct</response>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ResumeView>> CreateResumeAsync([FromBody] CreateResumeRequest createResumeRequest)
        {
            Guid currentUserId = GetCurrentUserId();
            var currentUser = await userManager.FindByIdAsync(currentUserId.ToString());

            if (currentUser == null)
            {
                logger.LogDebug($"Current user {currentUserId} doesn't exist in database");
                return BadRequest("Current user doesn't exist in database");
            }

            var newResume = mapper.Map<Resume>(createResumeRequest);
            newResume.UserId = currentUserId;
            newResume.User = currentUser;

            await dbContext.Resumes.AddAsync(newResume);
            await dbContext.SaveChangesAsync();

            return Ok(await dbContext.Resumes
                .ProjectTo<ResumeView>(mapper.ConfigurationProvider)
                .SingleAsync(res => res.Id == newResume.Id));
        }

        /// <summary>
        /// Edit resume
        /// </summary>
        /// <param name="editResumeRequest"></param>
        /// <returns>Edited reume</returns>
        /// <response code="200">Resume was successfully edited</response>
        /// <response code="400">Access token isn't correct</response>
        /// <response code="403">Current user can't edit passed resume</response>
        /// <response code="404">Can't find resume with passed id</response>
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ResumeView>> EditResumeAsync([FromBody] EditResumeRequest editResumeRequest)
        {
            Guid currentUserId = GetCurrentUserId();
            var currentUser = await userManager.FindByIdAsync(currentUserId.ToString());

            if (currentUser == null)
            {
                logger.LogDebug($"Current user {currentUserId} doesn't exist in database");
                return BadRequest("Current user doesn't exist in database");
            }

            var resumeToEdit = await dbContext.Resumes.FirstOrDefaultAsync(res => res.Id == editResumeRequest.Id);
            if (resumeToEdit == null)
            {
                logger.LogDebug($"Can't find resume {editResumeRequest.Id}");
                return NotFound("Can't find resume");
            }

            if (resumeToEdit.UserId != currentUserId || resumeToEdit.User != currentUser)
            {
                logger.LogDebug($"Current user {currentUserId} can't edit resume {editResumeRequest.Id}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }

            mapper.Map(editResumeRequest, resumeToEdit);
            await dbContext.SaveChangesAsync();

            return Ok(await dbContext.Resumes
                .ProjectTo<ResumeView>(mapper.ConfigurationProvider)
                .SingleAsync(res => res.Id == resumeToEdit.Id));
        }

        /// <summary>
        /// Delete resume
        /// </summary>
        /// <param name="resumeId"></param>
        /// <returns>Guid of deleted resume</returns>
        /// <response code="200">Resume was successfully deleted</response>
        /// <response code="400">Access token isn't correct</response>
        /// <response code="403">Current user can't delete passed resume</response>
        /// <response code="404">Can't find resume with passed id</response>
        [HttpDelete("{resumeId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Guid>> DeleteResumeAsync(Guid resumeId)
        {
            Guid currentUserId = GetCurrentUserId();
            var currentUser = await userManager.FindByIdAsync(currentUserId.ToString());

            if (currentUser == null)
            {
                logger.LogDebug($"Current user {currentUserId} doesn't exist in database");
                return BadRequest("Current user doesn't exist in database");
            }

            var resumeToDelete = await dbContext.Resumes.FirstOrDefaultAsync(res => res.Id == resumeId);
            if (resumeToDelete == null)
            {
                logger.LogDebug($"Can't find resume {resumeId}");
                return NotFound("Can't find resume");
            }

            if (resumeToDelete.UserId != currentUserId || resumeToDelete.User != currentUser)
            {
                logger.LogDebug($"Current user {currentUserId} can't delete reume {resumeId}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }

            dbContext.Resumes.Remove(resumeToDelete);
            await dbContext.SaveChangesAsync();

            return Ok(resumeId);
        }

        private Guid GetCurrentUserId()
            => Guid.Parse(userManager.GetUserId(User));
    }
}
