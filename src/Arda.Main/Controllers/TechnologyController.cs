﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arda.Common.Utils;
using System.Net.Http;
using Arda.Main.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Arda.Main.Controllers
{
    [Authorize]
    public class TechnologyController : Controller
    {
        [HttpGet]
        public async Task<JsonResult> GetTechnologies()
        {
            // Getting uniqueName
            var uniqueName = HttpContext.User.Claims.First(claim => claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;

            // Getting the response of remote service
            var technologies = await Util.ConnectToRemoteService<List<TechnologyViewModel>>(HttpMethod.Get, Util.KanbanURL + "api/technology/list", uniqueName, "");

            return Json(technologies);
        }
    }
}
