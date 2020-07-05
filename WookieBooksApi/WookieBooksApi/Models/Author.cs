using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WookieBooksApi.Models
{
    public class Author
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthorId { get; set; }
        public string AuthorPseudonym { get; set; }
        public string AuthorName { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}