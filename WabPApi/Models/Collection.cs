using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WabPApi.Models
{
    [Table("wabpCollections")]
    public class Collection
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public string Status { get; set; }
        public int NumberOfDownloads { get; set; } = 0;
    }
}
