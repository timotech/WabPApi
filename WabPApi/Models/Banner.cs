using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WabPApi.Models
{
    public class Banner
    {
        public int ID { get; set; }
        public string Title { get; set; }
        [NotMapped]
        public IFormFile fleUploadImage { get; set; }
        public string PicPath { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
