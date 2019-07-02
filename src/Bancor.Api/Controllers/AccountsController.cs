﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Bancor.Core.Exceptions;
using Bancor.Core.Grains.Interfaces;
using Bancor.Core.Grains.Interfaces.Grains;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Bancor.Api.Controllers
{
    [Route("api/customers/{customerId}/accounts")]
    public class AccountsController : Controller
    {
        private readonly IClusterClient _clusterClient;

        public AccountsController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid customerId)
        {
            var customers = _clusterClient.GetGrain<ICustomerGrain>(customerId);

            return Ok((await customers.GetAccounts()).Select(a => new { a.Id, a.Name}));
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid customerId, [FromBody]CreateAccountRequest request)
        {
            var customer = _clusterClient.GetGrain<ICustomerGrain>(customerId);

            try
            {
                await customer.CreateAccount(request.Name);
            }
            catch (GrainDoesNotExistException e)
            {
                return NotFound(customerId);
            }

            return Ok();
        }

        [HttpPost("/{accountId}")]
        public async Task<IActionResult> Post(Guid customerId, Guid accountId, TransactionModel transaction)
        {
            var account = _clusterClient.GetGrain<IJournaledAccountGrain>(accountId);

            return Ok();
        }
    }

    public class TransactionModel
    {
        public decimal Amount { get; set; }
        public DateTime BookingDate { get; set; }
    }

    public class CreateAccountRequest
    {
        public string Name { get; set; }
    }
}
