#nullable disable
using System; // Unity needs this
using System.ComponentModel.DataAnnotations.Schema;
#if UNITY_5_3_OR_NEWER
using Newtonsoft.Json;
#else
using System.Text.Json.Serialization;
#endif
using MessagePack;

namespace YourGameServer.Models // Unity cannot accpect 'namespace YourProjectName.Models;' yet
{
    [MessagePackObject]
    public record PlayerOwnedServiceTicket
    {
        [Key(0)]
        public ulong Id { get; init; }
        [Key(1)]
        public ulong OwnerId { get; set; }
        [IgnoreMember]
        [JsonIgnore]
        [ForeignKey("OwnerId")]
        public PlayerAccount Owner { get; init; }
        [IgnoreMember]
        [JsonIgnore]
        public ulong ServiceTicketId { get; set; }
        [Key(2)]
        [ForeignKey("ServiceTicketId")]
        public ServiceTicket ServiceTicket { get; init; }
        [Key(3)]
        public ConsumableOrigin Origin { get; set; }
        [Key(4)]
        public ConsumableStatus Status { get; set; }
        [Key(5)]
        public DateTime? Period { get; set; }
        [Key(6)]
        public DateTime? UsedDate { get; set; }
        [Key(7)]
        public DateTime? InvalidateDate { get; set; }
        [Key(8)]
        public DateTime? ExpireDate { get; set; }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(OwnerId);
            hash.Add(ServiceTicketId);
            hash.Add(Origin);
            hash.Add(Status);
            hash.Add(Period);
            hash.Add(UsedDate);
            hash.Add(InvalidateDate);
            hash.Add(ExpireDate);
            return hash.ToHashCode();
        }

        public override string ToString() => $"{{{nameof(Id)}={Id}, {nameof(OwnerId)}={OwnerId}, {nameof(ServiceTicketId)}={ServiceTicketId}, {nameof(Origin)}={Origin}, {nameof(Status)}={Status}, {nameof(Period)}={Period}, {nameof(UsedDate)}={UsedDate}, {nameof(InvalidateDate)}={InvalidateDate}, {nameof(ExpireDate)}={ExpireDate}}}";
    }
}
