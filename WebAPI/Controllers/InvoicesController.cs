using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using WebAPI.Models;

namespace WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InvoicesController : ControllerBase
	{
		private readonly ILogger<InvoicesController> _logger;

		public InvoicesController(ILogger<InvoicesController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		[Route("get/invoices/{company}")]
		public Response<GetInvoicesResponse> GetInvoices(string company)
		{
			List<Invoice> invoices = new List<Invoice>();

			using (SqlConnection sqlConnection = new SqlConnection("Server=localhost\\SQLEXPRESS;Database=MyCompany;Trusted_Connection=True;"))
			{
				sqlConnection.Open();
				using (SqlCommand command = sqlConnection.CreateCommand())
				{
					command.CommandText = $@"
						SELECT * FROM invoice AS inv 
						JOIN customer AS c ON inv.CustomerId = c.CustomerId 
						WHERE c.Company = @Company";

					command.Parameters.AddWithValue("@Company", company);

					SqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						Customer customer = new Customer
						{
							CustomerId = reader.GetGuid(5),
							FirstName = reader.GetString(6),
							LastName = reader.GetString(7),
							Company = reader.GetString(8),
							Created = reader.GetDateTime(9)
						};
						List<InvoiceItem> items = new List<InvoiceItem>();

						Invoice invoice = new Invoice(invoiceId: reader.GetGuid(0), customer: customer, reference: reader.GetString(2), total: reader.GetDecimal(3));
						invoice.Items.AddRange(items);
						invoices.Add(invoice);
					}
				}
			}

			foreach(Invoice invoice in invoices)
			{
				using (SqlConnection sqlConnection = new SqlConnection("Server=localhost\\SQLEXPRESS;Database=MyCompany;Trusted_Connection=True;"))
				{
					using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
					{
						sqlCommand.CommandText = $@"
							SELECT ii.*, i.*, inv.* 
							FROM InvoiceItem AS ii 
							JOIN invoice AS inv ON ii.InvoiceId = inv.InvoiceId 
							JOIN Item AS i ON ii.ItemId = i.ItemId 
							WHERE inv.InvoiceId = @Invoice";

						sqlCommand.Parameters.AddWithValue("@Invoice", invoice.InvoiceId);

						sqlConnection.Open();
						SqlDataReader reader = sqlCommand.ExecuteReader();
						while (reader.Read())
						{
							Item item = new Item(
								itemId: reader.GetGuid(5),
								name: reader.GetString(6),
								price: reader.GetDecimal(7),
								created: reader.GetDateTime(8)
								);

							InvoiceItem invoiceItem = new InvoiceItem
							{
								Item = item,
								InvoiceId = invoice.InvoiceId,
								InvoiceItemId = reader.GetGuid(0)
							};

							invoice.Items.Add(invoiceItem);
						}
					}
				}
			}

			decimal customerInvoiceTotal = invoices.Sum(invoice => invoice.Total);

			return new Response<GetInvoicesResponse>(responseCode: "200", new GetInvoicesResponse()
			{
				Invoices = invoices,
				InvoicesTotal = customerInvoiceTotal
			});
		}
	}
}
