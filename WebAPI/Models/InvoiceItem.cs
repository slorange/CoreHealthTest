namespace WebAPI.Models
{
	public class InvoiceItem
	{
		public Guid InvoiceItemId { get; set; }
		public Guid InvoiceId { get; set; }
		public Item Item { get; set; }
		public int Quantity { get; set; }
		public DateTime Created { get; set; }
	}
}