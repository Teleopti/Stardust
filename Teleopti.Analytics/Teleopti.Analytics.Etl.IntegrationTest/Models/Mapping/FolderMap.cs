using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping
{
    public class FolderMap : EntityTypeConfiguration<Folder>
    {
        public FolderMap()
        {
            // Primary Key
            this.HasKey(t => t.FolderId);

            // Properties
            this.Property(t => t.FolderId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.FolderName)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(150);

            this.Property(t => t.Description)
                .HasMaxLength(255);

            this.Property(t => t.Inherited)
                .IsFixedLength()
                .HasMaxLength(1);

            // Table & Column Mappings
            this.ToTable("Folders", "mart");
            this.Property(t => t.FolderId).HasColumnName("FolderId");
            this.Property(t => t.FolderName).HasColumnName("FolderName");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.ParentFolderId).HasColumnName("ParentFolderId");
            this.Property(t => t.CompanyId).HasColumnName("CompanyId");
            this.Property(t => t.Inherited).HasColumnName("Inherited");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.OwnerId).HasColumnName("OwnerId");
            this.Property(t => t.LastModifiedDate).HasColumnName("LastModifiedDate");
            this.Property(t => t.ModifiedId).HasColumnName("ModifiedId");
        }
    }
}
