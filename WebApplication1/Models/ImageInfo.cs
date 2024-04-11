using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class ImageInfo
    {
        public string ImageUrl { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public long? FileSize { get; set; } // File size in bytes
    }
}