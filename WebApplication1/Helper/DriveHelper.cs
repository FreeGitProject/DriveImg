using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

public static class DriveHelper
{
    public static Google.Apis.Drive.v3.Data.File UploadFile(DriveService driveService, HttpPostedFileBase file, string folderId)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = Path.GetFileName(file.FileName),
            Parents = new[] { folderId }
        };

        using (var stream = new MemoryStream())
        {
            file.InputStream.CopyTo(stream);
            stream.Position = 0;
            var request = driveService.Files.Create(fileMetadata, stream, file.ContentType);
            request.Upload();
            return request.ResponseBody;
        }
    }

    public static Google.Apis.Drive.v3.Data.File GetFolder(DriveService driveService, string folderName)
    {
        var listRequest = driveService.Files.List();
        listRequest.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}'";
        var result = listRequest.Execute();
        var folder = result.Files.FirstOrDefault();

        if (folder == null)
        {
            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };
            folder = driveService.Files.Create(folderMetadata).Execute();
        }

        return folder;
    }

    public static Stream DownloadFile(DriveService driveService, string fileId)
    {
        var request = driveService.Files.Get(fileId);
        var stream = new MemoryStream();
        request.Download(stream);
        stream.Position = 0;
        return stream;
    }

    public static void DeleteFile(DriveService driveService, string fileId)
    {
        driveService.Files.Delete(fileId).Execute();
    }

    public static IList<Google.Apis.Drive.v3.Data.File> SearchFiles(DriveService driveService, string query)
    {
        var listRequest = driveService.Files.List();
        listRequest.Q = query;
        var result = listRequest.Execute();
        return result.Files;
    }
    public static IList<Google.Apis.Drive.v3.Data.File> GetImagesFromDrive(DriveService driveService, string folderId)
    {
        var listRequest = driveService.Files.List();
        listRequest.Q = $"'{folderId}' in parents and mimeType contains 'image/'";
        var result = listRequest.Execute();
        return result.Files;
    }
}
