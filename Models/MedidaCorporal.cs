using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KROTraining.Models
{
    [Table("MEDIDA_CORPORAL")]
    public class MedidaCorporal
    {
        [Key]
        [Column("medida_id")]
        public int MedidaId { get; set; }

        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("peso", TypeName = "decimal(5,2)")]
        public decimal Peso { get; set; }

        [Column("estatura", TypeName = "decimal(3,2)")]
        public decimal Estatura { get; set; }

        [Column("cintura", TypeName = "decimal(5,2)")]
        public decimal? Cintura { get; set; }

        [Column("cadera", TypeName = "decimal(5,2)")]
        public decimal? Cadera { get; set; }

        [Column("brazo", TypeName = "decimal(5,2)")]
        public decimal? Brazo { get; set; }

        [ForeignKey(nameof(ClienteId))]
        public virtual Cliente Cliente { get; set; } = null!;
    }
}