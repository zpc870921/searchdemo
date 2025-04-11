using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using seachdemo.Data;
using seachdemo.Mappers;
using seachdemo.Models;

namespace seachdemo.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MapsterController : ControllerBase
    {
        private readonly MyDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public MapsterController(MyDbContext dbContext,IMapper mapper,IMemoryCache memoryCache)
        {
            this._dbContext = dbContext;
            this._mapper = mapper;
            this._memoryCache = memoryCache;
        }

        [HttpPost]
        public async Task<IActionResult> test2()
        {

            return Ok();
        }

        [HttpGet]
        [ResponseCache(Duration =20)]
        public async Task<IActionResult> cachedemo()
        {
            return Ok(DateTime.Now);
        }

        [HttpGet]
        public async Task<IActionResult> memcache(int id)
        {
            Console.WriteLine("开始执行memcache");
            //null也缓存-可以解决缓存穿透问题
            var customer =await this._memoryCache.GetOrCreateAsync($"customer:{id}", async e =>
            {
                //绝对过期和滑动过期
                //e.AbsoluteExpiration= DateTime.Now;
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                e.SlidingExpiration = TimeSpan.FromSeconds(10);
                Console.WriteLine("从数据库查询");

                return await _dbContext.Customer.FirstOrDefaultAsync(c => c.Id == id);
            });
            Console.WriteLine("结果：");

            return Ok(customer);
        }

        [HttpGet]
        public async Task<IActionResult> demo()
        {

            var customer = new Customer
            {
                Id = 1,
                Name = "张二二"
            };
            var entry= _dbContext.Entry<Customer>(customer);
            entry.Property(c => c.Name).IsModified=true;
            await _dbContext.SaveChangesAsync();



            var t1 = new Teacher {
             Name="张三"
            };

            var t2 = new Teacher
            {
                Name = "李四"
            };

            var t3 = new Teacher
            {
                Name = "王五"
            };

            var s1 = new Student
            {
                Name = "学生1"
            };
            var s2 = new Student
            {
                Name = "学生2"
            };

            var s3 = new Student
            {
                Name = "学生3"
            };
            t1.Students.Add(s1);
            t1.Students.Add(s2);

            t2.Students.Add(s2);
            t2.Students.Add(s3);

            t3.Students.Add(s1);
            t3.Students.Add(s2);
            t3.Students.Add(s3);

            await _dbContext.Teachers.AddRangeAsync(t1,t2,t3);
            await _dbContext.SaveChangesAsync();

            var customers = _dbContext.Customer.Where(c=>c.Desc.Contains("香港"));
            foreach (var item in customers)
            {
                Console.WriteLine(item.Name);
            }

            //var customers = await _dbContext.Customer.Where(c => c.Biz_Orders.Any(o => o.SerialCode.Contains("code"))).ToListAsync();

            //var customer = new Customer
            //{
            //    Name = "test66",
            //    Email = "test@test.com",
            //    Hobby = "hobby1;hobby2",
            //    C_time = DateTime.Now,
            //    Desc = "test desc",
            //    Update_time = DateTime.Now,
            //};
            //var order1 = new Biz_Order2
            //{
            //    Amount = 200,
            //    C_time = DateTime.Now,
            //    SerialCode = "1serial code",
            //    Status = 1,
            //    Customer = customer
            //};
            //var order2 = new Biz_Order2
            //{
            //    Amount = 300,
            //    C_time = DateTime.Now,
            //    CustomerId = 1,
            //    SerialCode = "2serial code",
            //    Status = 1,
            //    Customer = customer
            //};

            //customer.Biz_Orders.Add(order1);
            //customer.Biz_Orders.Add(order2);
            //_dbContext.Customer.Add(customer);
            //_dbContext.Biz_Order.Add(order1);
            //_dbContext.Biz_Order.Add(order2);
            //await _dbContext.SaveChangesAsync();

            //var customer = await _dbContext.Customer
            //  .Include(c=>c.Biz_Orders).FirstOrDefaultAsync(c=>c.Id==2);

            //decimal i = 123.Adapt<decimal>();
            //var e = "Read, Write, Delete".Adapt<FileShare>();

            //var point = new { X = 2, Y = 3 };
            //var dict = point.Adapt<Dictionary<string, int>>();

            //var src = new { Name = "Mapster", Email = "443813032@qq.com" };
            //var target = src.Adapt<Customer>();

            var user = new User
            {
                Id = 1,
                FirstName = "first name",
                LastName = "last name"
            };
            var userDto = user.Adapt<UserDto>();    

            var customer2=await _dbContext.Customer.FirstOrDefaultAsync();
            customer2.Hobby = null;

            var target = new CustomerDto2 {
             Name="test"
            };
            //var customer2=await _dbContext.Customer.ProjectToType<CustomerDto2>().FirstOrDefaultAsync();
            //var customerDto = customer2.BuildAdapter()
            //  .AddParameters("UserDesc", customer2.Desc)
            //  .AdaptToType<CustomerDto2>();
           // var customerDto = _mapper.Map<CustomerDto2>(customer2);

            //var customerDto = customer2.Adapt<CustomerDto2>();
            var customerDto = customer2.Adapt(target);

            return Ok(customerDto);
            //var customerDto = new CustomerDto2()
            //{
            //     Email="443813032@qq.com"
            //};
            //customer.Adapt(customerDto);
            //customer.Adapt<CustomerDto2>();
            //return Ok(customerDto);
        }
    }
}
