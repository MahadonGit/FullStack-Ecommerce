namespace Ecommerce.Models
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string? DeletedBy { get; set; }
    }
}
