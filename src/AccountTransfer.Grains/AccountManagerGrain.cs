﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountTransfer.Interfaces;
using BankOr.Core;
using Orleans;

namespace AccountTransfer.Grains
{
    public class AccountManagerGrain : Grain, IAccountManagerGrain
    {
        public Task<IAccountGrain> Create(string userId)
        {
            var bankAccountGrain = GrainFactory.GetGrain<IAccountGrain>(Guid.NewGuid());
            bankAccountGrain.Owner(userId);

            return Task.FromResult(bankAccountGrain);
        }

        public Task<IList<Account>> GetAccounts()
        {
            throw new NotImplementedException();
        }

    }
}