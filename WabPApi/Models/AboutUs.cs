using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WabPApi.Models
{
    [Table("wabpAboutUs")]
    public class AboutUs
    {
        public int Id { get; set; }
        [AllowHtml]
        public string Description { get; set; }
    }
}
