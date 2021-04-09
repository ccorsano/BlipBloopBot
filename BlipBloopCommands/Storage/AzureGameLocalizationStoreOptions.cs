using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopCommands.Storage
{
    public class AzureGameLocalizationStoreOptions
    {
        public string TableName { get; set; }
        public string StorageConnectionString { get; set; }
    }
}
