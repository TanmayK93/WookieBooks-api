using System;

namespace WookieBooksApi.Models
{
    internal class BooksDetailsDTO
    {
        public int bookId { get; set; }
        public string title { get; set; }
        public decimal price { get; set; }
        public string coverImage { get; set; }
        public string description { get; set; }
        public string authorName { get; set; }
        public string authorPseudonym { get; set; }
    }
}