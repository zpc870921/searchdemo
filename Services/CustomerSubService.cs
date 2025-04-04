using DotNetCore.CAP;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using seachdemo.Data;
using seachdemo.Models;

namespace seachdemo.Services
{
    public class CustomerSubService:ICapSubscribe
    {
        private readonly MyDbContext _dbcontext;
        private readonly ElasticsearchClient _elasticsearchClient;

        public CustomerSubService(MyDbContext dbcontext,ElasticsearchClient elasticsearchClient)
        {
            this._dbcontext = dbcontext;
            this._elasticsearchClient = elasticsearchClient;
        }

        [CapSubscribe("customer.add",Group ="queue.customer.add")]
        public async Task CustomerAdd(int id)
        {
            var customer = await _dbcontext.Customer.FirstOrDefaultAsync(c=>c.Id==id);
            if(null==customer)
                return;
            var customerDoc = new CustomerDoc(customer);
            var ret = await this._elasticsearchClient.IndexAsync(customerDoc);
        }

        [CapSubscribe("customer.delete",Group = "queue.customer.delete")]
        public async Task CustomerDelete(int id)
        {
            await this._elasticsearchClient.DeleteAsync<Customer>(id.ToString());
        }
    }
}
