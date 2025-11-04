public interface ICarrier
{
    int Capacity { get; }
    int Count { get; }

    bool TryPickupFrom(IStorage storage, int amount = 1);
    bool TryDropTo(IStorage storage, int amount = 1);
}
