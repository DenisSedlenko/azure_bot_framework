using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Interfaces
{
    public interface IRepository
    {
        Task<CloudTable> RetriveOrCreateTableAsync(CloudTableClient cloudTableClient, string tableName);

        Task<T> InsertOrMergeEntityAsync<T>(CloudTable table, T entity) where T : TableEntity;

        Task<IEnumerable<T>> RetrieveEntityUsingPointQueryAsync<T>(CloudTable table, string partitionKey, string rowKey) where T : ITableEntity, new();

        Task DeleteEntityAsync<T>(CloudTable table, T deleteEntity) where T : TableEntity;
    }
}
