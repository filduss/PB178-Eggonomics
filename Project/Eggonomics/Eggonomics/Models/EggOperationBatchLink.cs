namespace Eggonomics.Models
{
    public class EggOperationBatchLink
    {
        public int Id { get; set; }
        public int EggOperationId { get; set; }
        public EggOperation EggOperation { get; set; } = null!;
        public int? EggTrayId { get; set; }
        public EggTray? EggTray { get; set; }
        public int QuantityTaken { get; set; }

        public EggOperationBatchLink() { }

        public EggOperationBatchLink(EggOperation eggOperation, EggTray eggTray, int quantityTaken)
        {
            EggOperation = eggOperation;
            EggOperationId = eggOperation.Id;
            EggTray = eggTray;
            EggTrayId = eggTray.Id;
            QuantityTaken = quantityTaken;
        }
    }
}
