﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountTransfer.Interfaces.Grains;
using BankOr.Core;
using BankOr.Core.Exceptions;
using BankOr.Core.Models;
using Orleans;
using Orleans.Providers;

namespace AccountTransfer.Grains
{

    [StorageProvider(ProviderName = "CustomerStorageProvider")]
    public class CustomerGrain : Grain<CustomerGrainState>, ICustomerGrain
    {
        public async Task HasNewName(string name)
        {

            State.Name = name;
            await WriteStateAsync();
        }

        public async Task<IList<AccountModel>> GetAccounts()
        {
            EnsureCreated();

            var accountNames = new List<AccountModel>();
            foreach (var account in State.AccountGrains)
            {
                accountNames.Add(new AccountModel{
                   Name = await account.GetName(),
                    Id = account.GetPrimaryKeyLong(),
                    Balance = await account.GetBalance()
                });
            }

            return accountNames;
        }


        public async Task CreateAccount(string name)
        {
            EnsureCreated();

            var item = Math.Abs(Guid.NewGuid().GetHashCode());  
            var account = GrainFactory.GetGrain<IAccountGrain>(item);

            await account.HasNewName(name);
            State.AccountGrains.Add(account);

            await WriteStateAsync();

        }

        public async Task TryInit(string name)
        {
            await HasNewName(name);
        }

        private void EnsureCreated()
        {
            if (!State.Created)
                throw new GrainDoesNotExistException($"Customer with id '{this.GetPrimaryKeyLong()}' does not exist");
        }
    }
}