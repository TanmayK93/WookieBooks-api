using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WookieBooksApi.Models;

namespace WookieBooksAPI.UnitTest.MockData
{
    public class BookTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new Books { BookId = 3, Title  = "",
                AuthorId = 1, Price = 12.2m,
                CoverImage = "data/base64",
                BookPublished = true, Description = "This is Book 1" } };

            yield return new object[] { new Books {  BookId = 4, Title  = " ",
                AuthorId = 1, Price = 12.2m,
                CoverImage = "data/base64",
                BookPublished = true, Description = "This is Book 2" } };

            yield return new object[] { new Books {  BookId = 5, Title  = "Book 3",
                AuthorId = 1, Price = 0,
                CoverImage = "data/base64",
                BookPublished = true, Description = "This is Book 3" } };

            yield return new object[] { new Books {  BookId = 6, Title  = "Book 4",
                AuthorId = 1, Price = -1,
                CoverImage = "data/base64",
                BookPublished = true, Description = "This is Book 4" } };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
