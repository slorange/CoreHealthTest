using WebAPI.Controllers;

namespace WebAPI.Models
{
	public class Invoice
	{
		public Guid InvoiceId { get; set; }
		public Customer Customer { get; set; }
		public string Reference { get; set; }
		public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
		public decimal Total { get; set; }
		public DateTime Created { get; set; }
	}
}
