using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Models
{
    /// <summary>
    /// Represents pairs for storing keys
    /// </summary>
    public class PairEntity : TableEntity
    {
        public PairEntity(string userId, string service)
        {
            PartitionKey = userId;
            RowKey = service;
        }

        public PairEntity() { }

        public Guid UserId { get; set; }

        public string Service { get; set; }
    }
}
