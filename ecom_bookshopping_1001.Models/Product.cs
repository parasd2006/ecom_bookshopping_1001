using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ecom_bookshopping_1001.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        [Range(1, 1000)]
        public double ListPrice { get; set; }//600
        [Required]
        [Range(1, 1000)]
        public double Price { get; set; }//550 yeh discount ke stah
        [Required]
        [Range(1, 1000)]
        public double Price50 { get; set; }//500 yeh agr 50 orr 100 ke bihcme books loge
        [Required]
        [Range(1, 1000)]
        public double Price100 { get; set; }//450 yeh agr 100 se jyada books loge

        [Display(Name = "Imageurl")]
        public string Imageurl { get; set; }
        [Required]
        [Display (Name ="Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [Required]
        [Display(Name = "Cover Type")]
        public int CopyTypeId { get; set; }
        public CopyType CopyType { get; set; }

    }
}
