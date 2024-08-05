namespace InventoryManagement.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class History
    {
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public string ActionType { get; set; } // "Enter" or "Exit"
        public int Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public Material Material { get; set; }
    }
}
