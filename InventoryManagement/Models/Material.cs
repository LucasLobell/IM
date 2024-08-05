namespace InventoryManagement.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Material
    {
        public int Id { get; set; }
        public string? Code { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }


        [Required]
        public int Quantity { get; set; }



        public void Enter(int amount)
        {
            if (amount > 0)
            {
                Quantity += amount;
            }
        }

        public void Exit(int amount)
        {
            if (amount > 0 && amount <= Quantity)
            {
                Quantity -= amount;
            }
        }
    }
}
