namespace AquaServer.StorageModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Linq;

    public class AquaStorage : DbContext
    {
        public AquaStorage(): base("name=AquaStorage")
        {

            //Database.SetInitializer<AquaStorage>(new DropCreateDatabaseAlways<AquaStorage>());
            Database.SetInitializer<AquaStorage>(new DropCreateDatabaseIfModelChanges<AquaStorage>());
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Account> Accounts { get; set; }        
        public DbSet<BillNoGenerator> BillNos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<DefaultValue> DefaultValues { get; set; }
        public DbSet<Session> Sessions { get; set; }        
    }
    
    public class Session
    {
        [Key]
        public long Id { get; set; }        
        [ForeignKey("User")]        
        public long UserId { get; set; }
        public User User { get; set; }
        public int UserType { get; set; }
        [Required]
        public string SessionId { get; set; }
        [Required]
        public int DeviceType { get; set; }
    }

    public class Company
    {
        [Key]
        public long Id { get; set; }
        [Required, MinLength(3)]
        public string CompanyCode { get; set; }
        [Required,MinLength(3)]
        public string Name { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class User
    {
        [Key]
        public long Id { get; set; }
        [Required, MinLength(3)]
        public string Name { get; set; }
        [Required, MinLength(3)]
        public string Username { get; set; }
        [Required, MinLength(3)]
        public string Password { get; set; }
        public int UserType { get; set; }
        public int Visibility { get; set; }

        [ForeignKey("Company")]
        public long CompanyId { get; set; }
        public Company Company { get; set; }

    }

    public class Account
    {
        [Key]
        public long Id { get; set; }
        [Required,MinLength(3)]
        public string Name { get; set; }
        [Required, MinLength(1),MaxLength(10)]
        public string ShortName { get; set; }
        public string Details1 { get; set; }
        public string Details2 { get; set; }
        public string Details3 { get; set; }
        [Required]
        public int MainGroup { get; set; }
        [Required]
        public long ParentGroupId { get; set; }
        [Required]
        public int AccountType{ get; set; }
        public int Visibility { get; set; }
        public bool IsConcrete { get; set; }

        [ForeignKey("Company")]
        public long CompanyId { get; set; }
        public Company Company { get; set; }
    }
    
    public class BillNoGenerator
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long BillNo { get; set; }
        [Required]
        public int BillType { get; set; }

        [ForeignKey("Company")]
        public long CompanyId { get; set; }
        public Company Company { get; set; }
        [Index]
        [Required]
        public long FinancialCode { get; set; }

    }

    public class Transaction
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long BillNo { get; set; }
        [Required]
        public DateTime BillDate { get; set; }
        [Required]
        public int BillType { get; set; }
        [Required]
        public int SubBillType { get; set; }
        [Required]
        public int SerialNo { get; set; }

        [Index]
        [ForeignKey("Account")]
        public long AccountId { get; set; }
        public Account Account { get; set; }
        [Required, MinLength(3)]
        public string AccountName { get; set; }
        [Required]
        public int MainGroup { get; set; }
        [Required]
        public long ParentGroupId { get; set; }
        [Required]
        public int AccountType { get; set; }

        [Required]
        public double Debit { get; set; }
        [Required]
        public double Credit { get; set; }
        public string Narration { get; set; }

        [Index]
        [Required]
        public long FinancialCode { get; set; }

        [Required]
        public long AddedUserId { get; set; }
        [Index]
        [ForeignKey("Company")]
        public long CompanyId { get; set; }
        public Company Company { get; set; }
    }
  
    public class DefaultValue
    {
        [Key]
        public long Id { get; set; }
        public double DoubleValue { get; set; }
        public int IntValue { get; set; }
        public string StringValue { get; set; }
        [Required]
        public string Name { get; set; }
        
        [ForeignKey("Company")]
        public long CompanyId { get; set; }
        public Company Company { get; set; }
    }
}