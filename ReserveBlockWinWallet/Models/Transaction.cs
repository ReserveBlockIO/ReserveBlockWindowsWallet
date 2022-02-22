using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveBlockWinWallet.Models
{
    public class Transaction
    {
        public string Hash { get; set; }
        public string ToAddress { get; set; }
        public string FromAddress { get; set; }
        public decimal Amount { get; set; }
        public long Nonce { get; set; }
        public decimal Fee { get; set; }
        public long Timestamp { get; set; }
        public string? NFTData { get; set; }
        public string Signature { get; set; }
        public long Height { get; set; }
    }
}
