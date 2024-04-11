using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Runtime.Caching;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;


public class DriveController : Controller
{
    #region "Fields"
    private const string DriveServiceCacheKey = "DriveServiceCache";
    // Your GCP project credentials
    private static readonly string[] Scopes = { DriveService.Scope.Drive };
    private static readonly string ApplicationName = "Web client 2";
    // Retrieve Google API client ID and client secret from web.config
    string clientId = ConfigurationManager.AppSettings["GoogleClientId"];
    string clientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
    // string folderId = ConfigurationManager.AppSettings["folderId"];
    #endregion
    //private readonly IMemoryCache _memoryCache;
    //// Inject IMemoryCache into your controller
    //public DriveController(IMemoryCache memoryCache)
    //{
    //    _memoryCache = memoryCache;
    //}
    // GET: Drive
    public ActionResult Index()
    {
        try
        {
            // Try to retrieve DriveService from cache
            //if (!_memoryCache.TryGetValue(DriveServiceCacheKey, out DriveService driveService))
            //{
            // Try to retrieve DriveService from cache
            var driveService = MemoryCache.Default.Get(DriveServiceCacheKey) as DriveService;

            // If not in cache, create and cache the DriveService
            if (driveService == null)
            {
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("Drive.Auth.Store")
                ).Result;

                  driveService = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                // Cache the DriveService for a certain period of time (e.g., 1 hour)
                MemoryCache.Default.Add(DriveServiceCacheKey, driveService, DateTimeOffset.Now.AddHours(1));
            }

            // Cache the DriveService for a certain period of time (e.g., 1 hour)
            //    _memoryCache.Set(DriveServiceCacheKey, driveService, TimeSpan.FromHours(1));
            //}

            // Specify the Google Drive folder name
            string folderName = "product";

            // Find or create the folder
            var folder = DriveHelper.GetFolder(driveService, folderName);

            // Get all images in the "product" folder
            var imageList = DriveHelper.GetImagesFromDrive(driveService, folder.Id);

            var model = new UploadViewModel
            {
                ImageList = imageList.Select(file => new ImageInfo
                {
                    ImageUrl = Url.Action("Download", "Drive", new { fileId = file.Id }), // Use file Id to construct a download link
                    FileId = file.Id,
                    FileName = file.Name,
                    FileSize = file.Size
                }).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.Message = "Error: " + ex.Message;
            return View("Index");
        }
    }



    // POST: Drive/Upload
    [HttpPost]
    public ActionResult Upload(HttpPostedFileBase file)
    {
        if (file != null && file.ContentLength > 0)
        {
            try
            {

                //// Read client ID, client secret, and redirect URIs from the JSON file
                //var jsonPath = Server.MapPath("~/AppData/client_secret.json"); // Adjust the path accordingly
                //var jsonContent = File.ReadAllText(jsonPath);
                //var jsonSettings = JsonConvert.DeserializeObject<JsonSettings>(jsonContent);
                var driveService = MemoryCache.Default.Get(DriveServiceCacheKey) as DriveService;
                if (driveService == null)
                {
                    var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret
                        },
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore("Drive.Auth.Store")
                    ).Result;


                    driveService = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = ApplicationName,
                    });
                    // Cache the DriveService for a certain period of time (e.g., 1 hour)
                    MemoryCache.Default.Add(DriveServiceCacheKey, driveService, DateTimeOffset.Now.AddHours(1));
                }
                // Specify the Google Drive folder name
                string folderName = "product";

                // Find or create the folder
                var folder = DriveHelper.GetFolder(driveService, folderName);

                //// Upload the file to Google Drive
                //DriveHelper.UploadFile(driveService, file, folder.Id);

                // Upload the file to Google Drive and get the fileId
                var fileId = DriveHelper.UploadFile(driveService, file, folder.Id);


                ViewBag.Message = "File uploaded successfully.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
            }
        }
        else
        {
            ViewBag.Message = "Please select a file.";
        }

        return RedirectToAction("Index", "Drive");
    }
    //private class JsonSettings
    //{
    //    public string GoogleClientId { get; set; }
    //    public string GoogleClientSecret { get; set; }
    //}
    // GET: Drive/Download/ fileId
    public ActionResult Download(string fileId)
    {
        try
        {
            var driveService = MemoryCache.Default.Get(DriveServiceCacheKey) as DriveService;
            if (driveService == null)
            {
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("Drive.Auth.Store")
            ).Result;

                driveService = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                // Cache the DriveService for a certain period of time (e.g., 1 hour)
                MemoryCache.Default.Add(DriveServiceCacheKey, driveService, DateTimeOffset.Now.AddHours(1));
            }
            // Get the file metadata to retrieve the filename
            var fileRequest = driveService.Files.Get(fileId);
            var file = fileRequest.Execute();
            // Download the file by fileId
            var fileStream = DriveHelper.DownloadFile(driveService, fileId);

            // Convert fileStream to a byte array
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // Provide the file for download
            return File(fileBytes, "application/octet-stream", file.Name);
        }
        catch (Exception ex)
        {
            ViewBag.Message = "Error: " + ex.Message;
            return View("Index");
        }
    }

    // GET: Drive/Delete/ fileId
    public ActionResult Delete(string fileId)
    {
        try
        {
            var driveService = MemoryCache.Default.Get(DriveServiceCacheKey) as DriveService;
            if (driveService == null)
            {
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("Drive.Auth.Store")
            ).Result;

                 driveService = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                // Cache the DriveService for a certain period of time (e.g., 1 hour)
                MemoryCache.Default.Add(DriveServiceCacheKey, driveService, DateTimeOffset.Now.AddHours(1));
            }
            // Delete the file by fileId
            DriveHelper.DeleteFile(driveService, fileId);

            ViewBag.Message = "File deleted successfully.";
           
        }
        catch (Exception ex)
        {
            ViewBag.Message = "Error: " + ex.Message;
        }

        // return View("Index");
        return RedirectToAction("Index","Drive");
    }
}
