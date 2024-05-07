using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WabPApi.Models
{
    [Table("wabpBooks")]
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [Display(Name = "Price")]
        public decimal Price { get; set; }
        [Display(Name = "Special Price")]
        public decimal SpecialPrice { get; set; }
        [Display(Name="Full Information")]
        public string FullInfo { get; set; }
        [ForeignKey("Categories")]
        public int CategoryId { get; set; }
        public Categories Categories { get; set; }
        [NotMapped]
        public IFormFile fleUploadImage { get; set; }
        public string PicPath { get; set; }
        public DateTime DateAdded { get; set; }
        public string Genre { get; set; }
        public string Author { get; set; }
        [NotMapped]
        public IFormFile fleUploadEbook { get; set; }
        public string EbookPath { get; set; }
        [StringLength(int.MaxValue)]
        [MaxLength]
        public string Base64Code { get; set; }
    }

    public class BookDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public decimal SpecialPrice { get; set; }
        public string PicPath { get; set; }
        public string Genre { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string FullInfo { get; set; }
        public string EbookPath { get; set; }
        public int CategoryId { get; set; }
        public string Base64Code { get; set; }
        public long FileSize { get; set; }
        public int NumberOfDownloads { get; set; }
    }

    public class MyArray
    {
        public BookDTO book { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
    }
}
