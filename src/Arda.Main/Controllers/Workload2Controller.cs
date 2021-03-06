﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Arda.Common.Utils;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Microsoft.Net.Http.Headers;
using System.Net;
using Arda.Main.ViewModels;

using ArdaSDK.Kanban;
using ArdaSDK.Kanban.Models;

namespace Arda.Main.Controllers
{
    [Authorize]
    public class Workload2Controller : Controller
    {
        [HttpGet]
        public async Task<JsonResult> ListBacklogsByUser([FromQuery] string User)
        {
            var loggedUser = this.GetCurrentUser();

            string filtered_user = (User == null || User == "") ? loggedUser : User;

            var workloads = new List<string>();
            var existentWorkloads = await Util.ConnectToRemoteService<List<WorkloadsByUserViewModel>>(HttpMethod.Get, Util.KanbanURL + "api/workload/listworkloadbyuser", filtered_user, "");

            var dados = existentWorkloads.Where(x => x._WorkloadIsWorkload == false)
                         .Select(x => new {
                             id = x._WorkloadID,
                             title = x._WorkloadTitle,
                             start = x._WorkloadStartDate.ToString("dd/MM/yyyy"),
                             end = x._WorkloadEndDate.ToString("dd/MM/yyyy"),
                             hours = x._WorkloadHours,
                             attachments = x._WorkloadAttachments,
                             tag = x._WorkloadExpertise,
                             status = x._WorkloadStatus,
                             users = x._WorkloadUsers,
                             textual = x._WorkloadTitle + " (Started in " + x._WorkloadStartDate.ToString("dd/MM/yyyy") + " and Ending in " + x._WorkloadEndDate.ToString("dd/MM/yyyy") + ", with  " + x._WorkloadHours + " hours spent on this."
                         })
                         .Distinct()
                         .ToList();

            return Json(dados);
        }

        [HttpGet]
        public async Task<JsonResult> ListWorkloadsByUser([FromQuery] string User, [FromQuery] string Tag)
        {
            if( Tag != null && Tag != "" )
            {
                return await ListWorkloadsByWorkspace(Tag);
            }
            return await ListWorkloads(User);
        }

        private async Task<JsonResult> ListWorkloads(string User)
        {
            var loggedUser = this.GetCurrentUser();

            var workloads = new List<string>();

            string filtered_user = (User == null || User == "") ? loggedUser : User;

            var existentWorkloads = await Util.ConnectToRemoteService<List<WorkloadsByUserViewModel>>(HttpMethod.Get, Util.KanbanURL + "api/workload/listworkloadbyuser", filtered_user, "");

            var dados = existentWorkloads.Where(x => x._WorkloadIsWorkload == true)
                         .Select(x => new {
                             id = x._WorkloadID,
                             title = x._WorkloadTitle,
                             start = x._WorkloadStartDate.ToString("dd/MM/yyyy"),
                             end = x._WorkloadEndDate.ToString("dd/MM/yyyy"),
                             hours = x._WorkloadHours,
                             attachments = x._WorkloadAttachments,
                             tag = x._WorkloadExpertise,
                             status = x._WorkloadStatus,
                             users = x._WorkloadUsers,
                             textual = x._WorkloadTitle + " (Started in " + x._WorkloadStartDate.ToString("dd/MM/yyyy") + " and Ending in " + x._WorkloadEndDate.ToString("dd/MM/yyyy") + ", with  " + x._WorkloadHours + " hours spent on this."
                         })
                         .Distinct()
                         .ToList();

            Util.KanbanClient.WorkspaceFoldersService.ListItems(filtered_user);
            var w = new WorkspaceItem();
            var r = new
            {
                id = w.Id,
                title = w.Title,
                start = w.StartDate.Value.ToString("dd/MM/yyyy"),
                end = w.EndDate.Value.ToString("dd/MM/yyyy"),
                status = w.ItemState.Value,
                hours = 0,
                attachments = 0,
                tag = "",
                users = "", //x._WorkloadUsers,
                textual = w.Summary
            };

            return Json(dados);
        }
        private async Task<JsonResult> ListWorkloadsByWorkspace(string workspace)
        {
            var workloads = new List<string>();

            // var existentWorkloads = await Util.ConnectToRemoteService<List<WorkloadsByUserViewModel>>(HttpMethod.Get, Util.KanbanURL + "api/workload2/listtag?tag=" + workspace,"","");
            var existentWorkloads = await Util.ConnectToRemoteService<List<WorkloadStatusCompatViewModel>>(HttpMethod.Get, Util.KanbanURL + "api/workload2/liststatus/" + workspace, "", "");
            
            var dados = existentWorkloads //.Where(x => x._WorkloadIsWorkload == true)
                         .Select(x => new {
                             id = x._WorkloadID,
                             title = x._WorkloadTitle,
                             start = x._WorkloadStartDate.ToString("dd/MM/yyyy"),
                             end = x._WorkloadEndDate.ToString("dd/MM/yyyy"),
                             hours = x._WorkloadHours,
                             attachments = x._WorkloadAttachments,
                             tag = x._WorkloadExpertise,
                             status = x._WorkloadStatus,
                             users = x._WorkloadUsers,
                             textual = GetFirstLine(InsertLineBreak(x.StatusText))
                             //x._WorkloadTitle + " (Started in " + x._WorkloadStartDate.ToString("dd/MM/yyyy") + " and Ending in " + x._WorkloadEndDate.ToString("dd/MM/yyyy") + ", with  " + x._WorkloadHours + " hours spent on this."
                         })
                         //.Distinct()
                         .ToList();

            return Json(dados);
        }

        string InsertLineBreak(string text)
        {
            if (text == null)
                return null;

            return text.Replace("</p>", "</p>\n");
        }
        string GetFirstLine(string text)
        {
            if (text == null)
                return null;

            int endline = text.IndexOfAny( new char[] { '\n', '\r'});

            return (endline == -1) ? text : text.Substring(0, endline - 1);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add(ICollection<IFormFile> WBFiles, WorkloadViewModel2 workload)
        {
            //Owner:
            var uniqueName = this.GetCurrentUser();

            //Complete WB fields:
            workload.WBCreatedBy = uniqueName;
            workload.WBCreatedDate = DateTime.Now;
            //Iterate over files:
            if (WBFiles.Count > 0)
            {
                List<Tuple<Guid, string, string>> fileList = await UploadNewFiles(WBFiles);
                //Adds the file lists to the workload object:
                workload.WBFilesList = fileList;
            }            

            // Sometimes /Workload/Guid fails or takes a long time to return
            if(workload.WBID == Guid.Empty)
            {
                workload.WBID = Guid.NewGuid();
            }

            var response = await Util.ConnectToRemoteService(HttpMethod.Post, Util.KanbanURL + "api/workload/add", uniqueName, "", workload);

            UsageTelemetry.Track(uniqueName, ArdaUsage.Workload_Add);

            if (!response.IsSuccessStatusCode)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            await Assign(workload.WBID, workload.Tag, uniqueName);
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        async Task Assign(Guid WBID, string tags, string uniqueName)
        {
            string wbid = WBID.ToString();

            if (tags == null)
                return;

            foreach(string tag in tags.Split(';'))
            {
                if (tag != null && tag != "")
                {
                    var strresp = await Util.ConnectToRemoteServiceString(HttpMethod.Post, Util.KanbanURL + $"api/workload2/{tag}/assign/{wbid}", uniqueName, "");
                }
            }
        }

        [HttpPost]
        public async Task<WorkloadViewModel> AddSimple(ICollection<IFormFile> WBFiles, WorkloadViewModel2 workload)
        {
            //Owner:
            var uniqueName = this.GetCurrentUser();

            workload.WBActivity = Guid.Empty;
            workload.WBComplexity = 0;
            workload.WBExpertise = 0;
            
            //Complete WB fields:
            workload.WBCreatedBy = uniqueName;
            workload.WBCreatedDate = DateTime.Now;

            var now = DateTime.Now;
            var today = new DateTime(now.Year, now.Month, now.Day);

            workload.WBStartDate = today;
            workload.WBEndDate = today;

            // Myself
            workload.WBUsers = new string[] { uniqueName };

            // Sometimes /Workload/Guid fails or takes a long time to return
            if (workload.WBID == Guid.Empty)
            {
                workload.WBID = Guid.NewGuid();
            }

            var response = await Util.ConnectToRemoteService(HttpMethod.Post, Util.KanbanURL + "api/workload/add", uniqueName, "", workload);

            await Assign(workload.WBID, workload.Tag, uniqueName);

            UsageTelemetry.Track(uniqueName, ArdaUsage.Workload_Add);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"ConnectToRemote(Kanban/api/workload/add) failed with HTTP ${response.StatusCode}");

            return workload;
        }

        public class WorkloadSimpleViewModel
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int? Status { get; set; }            
        }
        [HttpPost]
        public async Task<WorkloadViewModel> AddSimpleV3([FromBody]WorkloadSimpleViewModel work)
        {
            if (work == null)
                throw new ArgumentOutOfRangeException(nameof(work));

            if (work.Title == null || work.Title == "")
                throw new ArgumentOutOfRangeException(nameof(work.Title));

            var uniqueName = this.GetCurrentUser();
            var now = DateTime.Now;
            var today = new DateTime(now.Year, now.Month, now.Day);

            WorkloadViewModel workload = new WorkloadViewModel2()
            {
                WBID = (work.Id == Guid.Empty) ? Guid.NewGuid() : work.Id,
                
                WBTitle = work.Title,
                WBDescription = work.Description,
                WBStatus = (work.Status.HasValue) ? work.Status.Value : 0,

                WBCreatedBy = uniqueName,
                WBCreatedDate = DateTime.Now,

                WBStartDate = today,
                WBEndDate = today,
                WBUsers = new string[] { uniqueName },
                
                WBIsWorkload = true,

                WBActivity = Guid.Empty,
                WBComplexity = 0,
                WBExpertise = 0,
                WBFilesList = null,
                WBMetrics = null,
                WBTechnologies = null
            };
            
            var response = await Util.ConnectToRemoteService(HttpMethod.Post, Util.KanbanURL + "api/workload/add", uniqueName, "", workload);

            UsageTelemetry.Track(uniqueName, ArdaUsage.Workload_Add);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"ConnectToRemote(Kanban/api/workload/add) failed with HTTP ${response.StatusCode}");

            return workload;
        }
        [HttpPut]
        public async Task<IActionResult> UpdateSimpleV3([FromBody]WorkloadSimpleViewModel work)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));

            if (work.Id == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(work));

            if (work.Title == null || work.Title == "")
                throw new ArgumentOutOfRangeException(nameof(work.Title));

            var uniqueName = this.GetCurrentUser();

            var workload = await Util.ConnectToRemoteService<WorkloadViewModel>(HttpMethod.Get, Util.KanbanURL + "api/workload/details?=" + work.Id, uniqueName, "");

            workload.WBTitle = work.Title;
            workload.WBDescription = work.Description;
            workload.WBStatus = (work.Status.HasValue) ? work.Status.Value : workload.WBStatus;

            var request = await Util.ConnectToRemoteService(HttpMethod.Put, Util.KanbanURL + "api/workload/edit", uniqueName, "", workload);

            if (!request.IsSuccessStatusCode)
                throw new InvalidOperationException("api/workload/edit failed");

            return Ok(workload);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update(ICollection<IFormFile> WBFiles, List<string> oldFiles, WorkloadViewModel workload)
        {
            //Owner:
            var uniqueName = this.GetCurrentUser();
            //Update WB fields:
            //workload.WBCreatedBy = uniqueName;
            //workload.WBCreatedDate = DateTime.Now;
            //Iterate over files:
            var fileList = new List<Tuple<Guid, string, string>>();
            if (WBFiles.Count > 0)
            {
                fileList = await UploadNewFiles(WBFiles);
            }
            if (oldFiles != null)
            {
                for (int i = 0; i < oldFiles.Count; i++)
                {
                    var split = oldFiles[i].Split('&');
                    var f = new Tuple<Guid, string, string>(new Guid(split[0]), split[1], split[2]);
                    fileList.Add(f);
                }
            }
            //Adds the file lists to the workload object:
            workload.WBFilesList = fileList;

            var response = await Util.ConnectToRemoteService(HttpMethod.Put, Util.KanbanURL + "api/workload/edit", uniqueName, "", workload);

            if (response.IsSuccessStatusCode)
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

        }

        [HttpGet]
        public async Task<JsonResult> GetWorkload(Guid workloadID)
        {
            var uniqueName = this.GetCurrentUser();

            var workload = await Util.ConnectToRemoteService<WorkloadViewModel>(HttpMethod.Get, Util.KanbanURL + "api/workload/details?=" + workloadID, uniqueName, "");
            return Json(workload);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdateStatus([FromQuery]string Id, [FromQuery]int Status)
        {
            var uniqueName = this.GetCurrentUser();
            //System.IO.StreamReader reader = new System.IO.StreamReader(HttpContext.Request.Body);
            //string requestFromPost = reader.ReadToEnd();

            await Util.ConnectToRemoteService<string>(HttpMethod.Put, Util.KanbanURL + "api/workload/updatestatus?id=" + Id + "&status=" + Status, uniqueName, "");
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete(Guid workloadID)
        {
            var uniqueName = this.GetCurrentUser();

            await Util.ConnectToRemoteService(HttpMethod.Delete, Util.KanbanURL + "api/workload/delete?=" + workloadID, uniqueName, "");
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public JsonResult GetGuid()
        {
            return Json(Util.GenerateNewGuid());
        }

        private static async Task<List<Tuple<Guid, string, string>>> UploadNewFiles(ICollection<IFormFile> WBFiles)
        {
            var fileList = new List<Tuple<Guid, string, string>>();
            var Configuration = new ConfigurationBuilder().AddJsonFile("secrets.json").Build();
            var connectionString = Configuration["Storage_AzureBLOB_ConnectionString"];
            var containerName = Configuration["Storage_AzureBLOB_ContainerName"];
            // Retrieve storage account information from connection string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            // Create a blob client for interacting with the blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Create a container for organizing blobs within the storage account.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            foreach (var file in WBFiles)
            {
                if (file.Length > 0)
                {
                    //Get file properties:
                    var filePath = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fileStream = file.OpenReadStream();
                    var fileName = new FileInfo(filePath).Name;
                    var fileExt = new FileInfo(filePath).Extension;
                    var fileID = Util.GenerateNewGuid();
                    var fileNameUpload = string.Concat(fileID, fileExt);
                    //Upload the file:
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileNameUpload);
                    await blockBlob.UploadFromStreamAsync(fileStream);
                    //Retrieve the url:
                    string fileURL = blockBlob.Uri.ToString();
                    //GUID, URL and Name:
                    fileList.Add(Tuple.Create(fileID, fileURL, fileName));
                }
            }

            return fileList;
        }


        [HttpGet]
        public async Task<JsonResult> ListArchiveWithFilter([FromQuery] string User)
        {
            var loggedUser = this.GetCurrentUser();

            var workloads = new List<string>();

            string filtered_user = (User == null || User == "") ? loggedUser : User;

            var existentWorkloads = await Util.ConnectToRemoteService<List<WorkloadsByUserViewModel>>(HttpMethod.Get, Util.KanbanURL + "api/workload/listarchivewithfilter?uniqueName=" + filtered_user, filtered_user, "");

            UsageTelemetry.Track(filtered_user, ArdaUsage.Archive_User);

            var dados = existentWorkloads.Where(x => x._WorkloadIsWorkload == true)
                         .Select(x => new {
                             id = x._WorkloadID,
                             title = x._WorkloadTitle,
                             start = x._WorkloadStartDate.ToString("dd/MM/yyyy"),
                             end = x._WorkloadEndDate.ToString("dd/MM/yyyy"),
                             hours = x._WorkloadHours,
                             attachments = x._WorkloadAttachments,
                             tag = x._WorkloadExpertise,
                             status = x._WorkloadStatus,
                             users = x._WorkloadUsers,
                             textual = x._WorkloadTitle + " (Started in " + x._WorkloadStartDate.ToString("dd/MM/yyyy") + " and Ending in " + x._WorkloadEndDate.ToString("dd/MM/yyyy") + ", with  " + x._WorkloadHours + " hours spent on this."
                         })
                         .Distinct()
                         .ToList();

            return Json(dados);
        }
    }
}
