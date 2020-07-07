using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using WookieBooksApi.Models;

namespace WookieBooksAPI.UnitTest.Models
{
    class DbContextMocker
    {
        #region IDisposable Support  
        private bool disposedValue = false; // To detect redundant calls  

        public static AppDbContext WookieBooksImportersDbContext()
        {

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var option = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

            var dbContext = new AppDbContext(option);

            if (dbContext != null)
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            }
            // Add entities in memory
            dbContext.Seed();

            return dbContext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion  
    }
}
