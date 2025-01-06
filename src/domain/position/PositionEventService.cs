using LanguageExt;

namespace domain.position
{
    public class PositionEventService(IPositionEventRepository positionEventRepository) : IPositionEventService
    {
        public Task<Fin<Unit>> SaveEvent(PositionEvent positionEvent)
        {
            return positionEventRepository.AddAsync(positionEvent);
        }
        public Task<Fin<PositionEvent>> GetLatestEvent(Guid positionId)
        {
            return positionEventRepository.GetLatestEvent(positionId);
        }
    }
}
