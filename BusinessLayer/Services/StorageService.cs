using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TeamsAuth.Interfaces.Service;

namespace TeamsAuth.Services
{
    public class StorageService : IStorageService
    {
        private readonly CloudTableClient _cloudTableClient;

        private readonly IRepository _repository;

        public StorageService(IRepository repository)
        {
            var storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials("storeapp", "iA8Q2+C8hVAimrHZbQyOEcSKDDKknyCBKZ3qDkuENRbg1f2n8EL0YHo4bV3SL5byUVBfU4dslIVcX8/WkNc6Yg=="), true);
            _cloudTableClient = storageAccount.CreateCloudTableClient();

            _repository = repository;
        }

        public async Task<T> InsertOrMergeEntityAsync<T>(string tableName, T entity) where T : TableEntity
        {
            var table = await _repository.RetriveOrCreateTableAsync(_cloudTableClient, tableName);

            return await _repository.InsertOrMergeEntityAsync(table, entity);
        }


        public async Task<IEnumerable<T>> RetrieveEntityUsingPointQueryAsync<T>(string tableName, string partitionKey, string rowKey) where T : ITableEntity, new()
        {
            var table = await _repository.RetriveOrCreateTableAsync(_cloudTableClient, tableName);

            return await _repository.RetrieveEntityUsingPointQueryAsync<T>(table, partitionKey, rowKey) as IEnumerable<T> ?? new List<T>();
        }


        public async Task DeleteEntityAsync<T>(string tableName, T entity) where T : TableEntity
        {
            var table = await _repository.RetriveOrCreateTableAsync(_cloudTableClient, tableName);

            await _repository.DeleteEntityAsync(table, entity);
        }
    }
}
