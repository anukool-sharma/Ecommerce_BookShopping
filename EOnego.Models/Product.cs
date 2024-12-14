using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EOnego.Models
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
        public double ListPrice { get; set; } // 600

        [Required]
        [Range(1, 1000)]
        public double Price { get; set; } // 550

        [Required]
        [Range(1, 1000)]
        public double Price50 { get; set; } // 500

        [Required]
        [Range(1, 1000)]
        public double Price100 { get; set; } // 450

        [Display(Name = "ImageUrl")]
        public string ImageUrl { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Required]
        [Display(Name = "CoverType")]
        public int CoverTypeId { get; set; }
        public CoverType CoverType { get; set; }
    }
}
