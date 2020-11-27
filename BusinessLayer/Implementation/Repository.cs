using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace BusinessLayer.Implementation
{
    public class Repository : IRepository
    {
        public async Task<CloudTable> RetriveOrCreateTableAsync(CloudTableClient cloudTableClient, string tableName)
        {
            var table = cloudTableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }

        public async Task<T> InsertOrMergeEntityAsync<T>(CloudTable table, T entity) where T : TableEntity
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table is not specified");
            }

            if (entity == null)
            {
                throw new ArgumentNullException("Entity cannot be null");
            }

            try
            {
                var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                var result = await table.ExecuteAsync(insertOrMergeOperation);
                var inserted = result.Result as T;


                return inserted;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> RetrieveEntityUsingPointQueryAsync<T>(CloudTable table, string partitionKey = null, string rowKey = null) where T : ITableEntity, new()
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table is not specified");
            }

            try
            {
                string filterPk = null;
                if (!string.IsNullOrEmpty(partitionKey)) {
                    filterPk = TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, partitionKey);
                }

                string filterRk = null;
                if (!string.IsNullOrEmpty(rowKey)) {
                    filterRk = TableQuery.GenerateFilterCondition(nameof(TableEntity.RowKey), QueryComparisons.Equal, rowKey);
                }

                var query = new TableQuery<T>().Where(filterPk != null && filterRk != null ? TableQuery.CombineFilters(filterPk, TableOperators.And, filterRk) : filterPk ?? filterRk);

                /*var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);*/
                var entities = new List<T>();
                TableContinuationToken continuationToken = null;
                do
                {
                    var page = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                    continuationToken = page.ContinuationToken;
                    entities.AddRange(page.Results as IEnumerable<T>);
                }
                while (continuationToken != null);


                return entities;
            }
            catch 
            {
                throw;
            }
        }

        public async Task DeleteEntityAsync<T>(CloudTable table, T deleteEntity) where T : TableEntity
        {
            if (table == null)
            {
                throw new ArgumentNullException("Table is not specified");
            }

            try
            {
                if (deleteEntity == null)
                {
                    throw new ArgumentNullException("deleteEntity");
                }

                var deleteOperation = TableOperation.Delete(deleteEntity);
                var result = await table.ExecuteAsync(deleteOperation);
            }
            catch
            {
                throw;
            }
        }
    }
}
