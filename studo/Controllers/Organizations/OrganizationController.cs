using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using studo.Models.Requests.Organization;
using studo.Models.Responses.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Controllers.Organizations
{
    [Produces("application/json")]
    [Route("api/organization")]
    public class OrganizationController : Controller
    {
        private readonly IMapper mapper;
        private readonly ILogger<OrganizationController> logger;

        public OrganizationController(IMapper mapper, ILogger<OrganizationController> logger)
        {
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationView>> CreateOrganizationAsync([FromBody] OrganizationCreateRequest organizationCreateRequest)
        {

        }
    }
}
