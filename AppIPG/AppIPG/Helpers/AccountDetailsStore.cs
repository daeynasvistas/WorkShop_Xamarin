using System;
using System.Collections.Generic;
using System.Text;

namespace AppIPG.Helpers
{
    public sealed class AccountDetailsStore
    {
        private AccountDetailsStore() { }

        public static AccountDetailsStore Instance { get; } = new AccountDetailsStore();

        public string Token { get; set; }
    }
}
