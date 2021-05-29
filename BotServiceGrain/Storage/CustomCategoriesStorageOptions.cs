using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrain.Storage
{
    public class CustomCategoriesStorageOptions
    {
        public const string DEFAULT_TABLE_NAME = "CustomCategories";

        public CustomCategoriesStorageOptions()
        {
            TableName = DEFAULT_TABLE_NAME;
        }

        public string TableName { get; set; }
        public string ConnectionString { get; set; }
    }
}
