using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class UploadViewModel
    {
        public string ImageUrl { get; set; }
        public string FileId { get; set; }
        public List<ImageInfo> ImageList { get; set; }
    }
}