using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public MapsterController(MyDbContext dbContext,IMapper mapper)
        {
            this._dbContext = dbContext;
            this._mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> demo()
        {
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
