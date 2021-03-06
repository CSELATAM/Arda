﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arda.Common.Utils;
using Arda.Common.ViewModels.Reports;
using System.Net.Http;
using System.Linq;
using Arda.Main.Utils;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Extensions.Caching.Distributed;
using ArdaSDK.Kanban;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Arda.Main.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {

        private IDistributedCache _cache = null;

        public ReportController(IDistributedCache cache)
        {
            _cache = cache;
        }

        //public IActionResult TimeConsuming()
        //{
        //    var uniqueName = HttpContext.User.Claims.First(claim => claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;

        //    UsageTelemetry.Track(uniqueName, ArdaUsage.Report_Show);

        //    return View();
        //}

        public async Task<IActionResult> TimeConsuming()
        {
            string userObjectID = HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var tokenCache = new NaiveSessionCache(userObjectID, NaiveSessionCacheResource.PowerBi, _cache);
            //ClientCredential credential = new ClientCredential(Startup.ClientId, Startup.ClientSecret);

            AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, tokenCache);

            var authenticationResult = await authContext.AcquireTokenSilentAsync(Startup.PowerBIResourceId, Startup.ClientId);

            var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

            using (var client = new PowerBIClient(new Uri(Startup.PowerBIApiUrl), tokenCredentials))
            {
                // You will need to provide the GroupID where the dashboard resides.
                ODataResponseListReport reports = await client.Reports.GetReportsInGroupAsync(Startup.PowerBIGroupId);

                // Get the first report in the group.
                Report report = reports.Value.FirstOrDefault();

                // Generate Embed Token.
                var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                EmbedToken tokenResponse = client.Reports.GenerateTokenInGroup(Startup.PowerBIGroupId, report.Id, generateTokenRequestParameters);

                // Generate Embed Configuration.
                var embedConfig = new EmbedConfig()
                {
                    EmbedToken = tokenResponse,
                    EmbedUrl = report.EmbedUrl,
                    Id = report.Id
                };

                return View("PowerBI", embedConfig);
            }
        }


        public async Task<JsonResult> GetActivityConsumingBubbleData(DateTime startDate, DateTime endDate, string user = "All")
        {
            var data = await Util.ConnectToRemoteService<D3BubbleViewModel>(HttpMethod.Get, Util.ReportsURL + "api/Activity/bubble?startDate=" + startDate + "&endDate=" + endDate + "&user=" + user, "", "");
            return Json(data);
        }

        public async Task<JsonResult> GetActivityConsumingTableData(DateTime startDate, DateTime endDate, string user = "All")
        {
            var data = await Util.ConnectToRemoteService<IEnumerable<ActivityConsumingViewModel>>(HttpMethod.Get, Util.ReportsURL + "api/Activity/tabledata?startDate=" + startDate + "&endDate=" + endDate + "&user=" + user, "", "");
            return Json(data);
        }

        public async Task<JsonResult> GetExpertiseConsumingBubbleData(DateTime startDate, DateTime endDate, string user = "All")
        {
            var data = await Util.ConnectToRemoteService<D3BubbleViewModel>(HttpMethod.Get, Util.ReportsURL + "api/Expertise/bubble?startDate=" + startDate + "&endDate=" + endDate + "&user=" + user, "", "");
            return Json(data);
        }

        public async Task<JsonResult> GetExpertiseConsumingTableData(DateTime startDate, DateTime endDate, string user = "All")
        {
            var data = await Util.ConnectToRemoteService<IEnumerable<ExpertiseConsumingViewModel>>(HttpMethod.Get, Util.ReportsURL + "api/Expertise/tabledata?startDate=" + startDate + "&endDate=" + endDate + "&user=" + user, "", "");
            return Json(data);
        }

        public async Task<JsonResult> GetMetricConsumingTableData(DateTime startDate, DateTime endDate, string user = "All")
        {
            var data = await Util.ConnectToRemoteService<IEnumerable<MetricConsumingViewModel>>(HttpMethod.Get, Util.ReportsURL + "api/Metric/tabledata?startDate=" + startDate + "&endDate=" + endDate + "&user=" + user, "", "");
            return Json(data);
        }
    }
}
