public interface IStorage
{
    bool CanAccept(ICarryable item);
    bool TryStore(ICarryable item);       // depoya ürün koy
    bool TryTake(out ICarryable item);    // depodan ürün al
    int Capacity { get; }
    int Count { get; }
}
