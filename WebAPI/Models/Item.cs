namespace WebAPI.Models
{
	public class Item
	{
		public Guid ItemId { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public DateTime Created { get; set; }
	}
}