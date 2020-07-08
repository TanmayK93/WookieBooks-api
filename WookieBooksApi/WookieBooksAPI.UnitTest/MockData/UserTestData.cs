using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WookieBooksApi.Models;

namespace WookieBooksAPI.UnitTest.MockData
{
    public class UserTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new User { UserName = "" ,
                Password =  "123456", Name = "Annie" } };

            yield return new object[] { new User { UserName = "ronney" ,
                Password =  "", Name = "Ronney" } };

            yield return new object[] { new User { UserName = "" ,
                Password =  "", Name = "" } };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}