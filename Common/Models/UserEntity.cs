using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Models
{
    /// <summary>
    /// Represents a user in the identity system.
    /// </summary>
    public class UserEntity : TableEntity
    {
        public UserEntity(string username, string id)
        {
            PartitionKey = username;
            RowKey = id;
        }

        public UserEntity() { }

        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Salt { get; set; }

        public string Password { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
