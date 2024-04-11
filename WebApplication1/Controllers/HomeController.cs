// Controllers/HomeController.cs
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

public class HomeController : Controller
{
    private readonly string credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData", "client_secret.json");
    public ActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public ActionResult UploadFile(HttpPostedFileBase file)
    {
        if (file != null && file.ContentLength > 0)
        {
            try
            {
                var fileModel = new FileModel
                {
                    FileName = Path.GetFileName(file.FileName),
                    Data = new byte[file.ContentLength],
                };

                file.InputStream.Read(fileModel.Data, 0, file.ContentLength);

                UploadFileToDrive(fileModel);

                ViewBag.Message = "File uploaded successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }
        }
        else
        {
            ViewBag.Message = "Please select a file to upload.";
        }

        return View("Index");
    }

    //private void UploadFileToDrive(FileModel fileModel)
    //{
    //    UserCredential credential;

    //    using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
    //    {
    //        string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    //        credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

    //        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
    //            GoogleClientSecrets.Load(stream).Secrets,
    //            new[] { DriveService.Scope.DriveFile },
    //            "user",
    //            CancellationToken.None).Result;
    //    }

    //    var service = new DriveService(new BaseClientService.Initializer()
    //    {
    //        HttpClientInitializer = credential,
    //        ApplicationName = "YourAppName",
    //    });

    //    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
    //    {
    //        Name = fileModel.FileName,
    //    };

    //    using (var stream = new MemoryStream(fileModel.Data))
    //    {
    //        var request = service.Files.Create(fileMetadata, stream, "image/jpeg");
    //        request.Upload();
    //    }
    //}
    private void UploadFileToDrive(FileModel fileModel)
    {
        UserCredential credential;

        using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                new[] { DriveService.Scope.DriveFile },
                "user",
                CancellationToken.None).Result;
        }

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "YourAppName",
        });

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = fileModel.FileName,
        };

        using (var stream = new MemoryStream(fileModel.Data))
        {
            var request = service.Files.Create(fileMetadata, stream, "image/jpeg");
            request.Upload();
        }
    }
}
