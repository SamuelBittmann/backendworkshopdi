using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BullshitApi.Business;
using Microsoft.AspNetCore.Mvc;

namespace BullshitApi.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BullshitController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<BullshitModel>> Get()
        {
            return Ok(new BullshitService().GetAll());
        }

        [HttpPost]
        public void Post([FromBody] BullshitModel model)
        {
            new BullshitService().Add(model);
        }
    }
}
