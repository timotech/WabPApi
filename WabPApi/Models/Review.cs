using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WabPApi.Models
{
    [Table("wabpReviews")]
    public class Review
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Username { get; set; }
        public string Rating { get; set; }
        public string BookId { get; set; }
        public virtual Book Book { get; set; }
    }
}
