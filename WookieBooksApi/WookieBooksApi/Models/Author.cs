using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WookieBooksApi.Models
{
    public class Author
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid AuthorId { get; set; }
        public string AuthorPseudonym { get; set; }
        public string AuthorName { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}