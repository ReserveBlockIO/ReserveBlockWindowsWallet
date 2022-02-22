using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveBlockWinWallet.Models
{
    public class Account
    {
        public long Id { get; set; }
        public string PrivateKey { set; get; }
        public string PublicKey { set; get; }
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public bool IsValidating { get; set; }
        public bool IsEncrypted { get; set; }
    }
}
