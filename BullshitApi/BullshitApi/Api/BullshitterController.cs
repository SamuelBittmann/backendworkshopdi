using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BullshitApi.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BullshitApi.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BullshitterController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok(new BullshitService().GetBullshitters());
        }
    }
}