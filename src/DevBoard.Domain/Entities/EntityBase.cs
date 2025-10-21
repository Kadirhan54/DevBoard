using DevBoard.Domain.Common;

namespace DevBoard.Domain.Entities
{
    public abstract class EntityBase<TKey> : IEntityBase<TKey> , ICreatedByEntity
    {
        public virtual TKey Id { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }

    }
}
