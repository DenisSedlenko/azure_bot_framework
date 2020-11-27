using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace TeamsAuth.Interfaces.Service
{
    public interface IStorageService
    {
        Task<T> InsertOrMergeEntityAsync<T>(string tableName, T entity) where T : TableEntity;

        Task<IEnumerable<T>> RetrieveEntityUsingPointQueryAsync<T>(string tableName, string partitionKey, string rowKey) where T : ITableEntity, new();

        Task DeleteEntityAsync<T>(string tableName, T entity) where T : TableEntity;
    }
}
