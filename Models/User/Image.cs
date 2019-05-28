using System;

namespace Qwiz.Models
{
    public class Image
    {
        public Image() {}

        public Image(string fileName, string ip, string uploaderUsername)
        {
            Ip = ip;
            FileName = fileName;
            UploaderUsername = uploaderUsername;
        }
        
        public int Id { get; set; }
        public string Ip { get; set; }
        public string FileName { get; set; }
        public string UploaderUsername { get; set; }
        public DateTime DateUploaded { get; set; } = DateTime.Now;
    }
}