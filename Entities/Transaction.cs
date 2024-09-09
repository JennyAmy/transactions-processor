
namespace TransactionsProcessor.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string RequestReference { get; set; }
        public decimal Amount { get; set; }
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }
        public string TransactionType { get; set; }
        public string Initiator { get; set; }
        public string Narration { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime ValueDate { get; set; }
        public bool Processed { get; set; }
    }

}
