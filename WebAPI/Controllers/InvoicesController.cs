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

		private readonly string _connString = "Server=localhost\\SQLEXPRESS;Database=MyCompany;Trusted_Connection=True;";

		public InvoicesController(ILogger<InvoicesController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		[Route("get/invoices/{company}")]
		public Response<GetInvoicesResponse> GetInvoices(string company)
		{
			try
			{
				_logger.LogInformation("Received GetInvoices request for company: " + company);

				List<Invoice> invoices = new List<Invoice>();

				using SqlConnection sqlConnection = new SqlConnection(_connString);
				sqlConnection.Open();

				using SqlCommand command = sqlConnection.CreateCommand();

				command.CommandText = $@"
					SELECT *
					FROM invoice AS inv 
					JOIN customer AS c ON inv.CustomerId = c.CustomerId 
					JOIN InvoiceItem AS ii ON ii.InvoiceId = inv.InvoiceId
					JOIN Item AS i ON ii.ItemId = i.ItemId
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

					Item item = new Item
					{
						ItemId = reader.GetGuid(15),
						Name = reader.GetString(16),
						Price = reader.GetDecimal(17),
						Created = reader.GetDateTime(18)
					};

					Invoice invoice = invoices.FirstOrDefault(i => i.InvoiceId == reader.GetGuid(0));
					if (invoice == null)
					{
						invoice = new Invoice
						{
							InvoiceId = reader.GetGuid(0),
							Customer = customer,
							Reference = reader.GetString(2),
							Total = reader.GetDecimal(3),
							Created = reader.GetDateTime(4)
						};
						invoices.Add(invoice);
					}

					InvoiceItem invoiceItem = new InvoiceItem
					{
						InvoiceItemId = reader.GetGuid(10),
						InvoiceId = invoice.InvoiceId,
						Item = item,
						Quantity = reader.GetInt32(13),
						Created = reader.GetDateTime(14)
					};

					invoice.Items.Add(invoiceItem);
				}

				decimal customerInvoiceTotal = invoices.Sum(invoice => invoice.Total);

				_logger.LogInformation("Successful GetInvoices request for company: " + company);

				return new Response<GetInvoicesResponse>(responseCode: "200", new GetInvoicesResponse()
				{
					Invoices = invoices,
					InvoicesTotal = customerInvoiceTotal
				});
			}
			catch(Exception e)
			{
				_logger.LogError("Error for GetInvoices request. Company: " + company + " Exception: " + e);

				return new Response<GetInvoicesResponse>(responseCode: "500", new GetInvoicesResponse()
				{
					//would only surface exception here if this is an internal tool
					Error = "Exception in GetInvoices: " + e
				});
			}
		}
	}
}
