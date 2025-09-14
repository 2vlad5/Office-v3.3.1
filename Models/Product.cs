namespace officeApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Volume { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public string Group { get; set; }
        public bool CountInTotal { get; set; } = true;
        public string AdditionalInfo { get; set; }
        public string Type { get; set; }
        public bool MsgSend { get; set; } = false;
    }
}