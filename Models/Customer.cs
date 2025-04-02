using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;

namespace seachdemo.Models
{
    // 客户模型
    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Hobby { get; set; }
        public string Desc { get; set; }

        //// 父子关系字段
        [JsonPropertyName("customer_order")]
        public JoinField CustomerOrder { get; set; } = JoinField.Root("customer");
    }
}
