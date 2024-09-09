
namespace TransactionsProcessor.Models
{
    public class TransactionRequest
    {
        public string RequestReference { get; set; }
        public decimal Amount { get; set; }
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }
        public string TransactionType { get; set; }
        public string Initiator { get; set; }
        public string Narration { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime ValueDate { get; set; }
    }

}
