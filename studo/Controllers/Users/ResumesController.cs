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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompactResumeView>>> GetAllResumesAsync()
        {
            if (await dbContext.Resumes.CountAsync() == 0)
                return Ok(new List<CompactResumeView>());

            return Ok(await dbContext.Resumes
                .ProjectTo<CompactResumeView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        [HttpGet("{resumeId:guid}")]
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

        [HttpGet("user/{userId:guid}")]
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

        [HttpPost]
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

        [HttpPut]
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

        [HttpDelete("{resumeId:guid}")]
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
