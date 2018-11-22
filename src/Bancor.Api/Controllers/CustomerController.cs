﻿using System.Threading.Tasks;
using Bancor.Core.Grains.Interfaces.Grains;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Bancor.Api.Controllers
{
    [Route("api/customers")]
    public class CustomerController : Controller
    {
        private readonly IClusterClient _clusterClient;

        public CustomerController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        // POST

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateCutomerRequest request)
        {
            var customerManager = _clusterClient.GetGrain<ICustomerManagerGrain>(0);

            var customer = await customerManager.Create(request.Name);

            var x = _clusterClient.GetGrain<IAccoutCreatedObserverGrain>(0);
            await x.StartSubscribe();
            return Ok(customer.GetPrimaryKeyLong());
        }

        // GET
        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            var customer = _clusterClient.GetGrain<ICustomerGrain>(id);

            return Ok(customer.GetPrimaryKey());
        }
    }

    public class CreateCutomerRequest
    {
        public string Name { get; set; }
    }
}