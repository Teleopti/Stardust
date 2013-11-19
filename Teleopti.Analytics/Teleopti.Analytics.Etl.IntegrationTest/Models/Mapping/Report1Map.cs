using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class Report1Map : EntityTypeConfiguration<Report1>
    {
        public Report1Map()
        {
            // Primary Key
            this.HasKey(t => t.ReportId);

            // Properties
            this.Property(t => t.ReportId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.ReportName)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(150);

            this.Property(t => t.Description)
                .HasMaxLength(255);

            this.Property(t => t.Inherited)
                .IsFixedLength()
                .HasMaxLength(1);

            // Table & Column Mappings
            this.ToTable("Reports", "mart");
            this.Property(t => t.Definition).HasColumnName("Definition");
            this.Property(t => t.ReportId).HasColumnName("ReportId");
            this.Property(t => t.ReportName).HasColumnName("ReportName");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.FolderId).HasColumnName("FolderId");
            this.Property(t => t.CompanyId).HasColumnName("CompanyId");
            this.Property(t => t.Inherited).HasColumnName("Inherited");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.OwnerId).HasColumnName("OwnerId");
            this.Property(t => t.LastModifiedDate).HasColumnName("LastModifiedDate");
            this.Property(t => t.ModifiedId).HasColumnName("ModifiedId");
        }
    }
}
