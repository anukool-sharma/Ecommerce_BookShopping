using EOnego.DataAccess.Data;
using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace EOnego.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            category = new CategoryRepository(context);
            coverType = new CoverTypeRepository(context);
            Product = new ProductRepository(context);
            Company = new CompanyRepository(context);
            ApplicationUser = new ApplicationUserRepository(context);
            ShoppingCart = new ShoppingCartRepository(context);
            OrderHeader = new OrderHeaderRepository(context);
            OrderDetails = new OrderDetailsRepository(context);


        }

        public ICategoryRepository category { private set; get; }

        public ICoverTypeRepository coverType { private set; get; }
        public IProductRepository Product  { private set; get; }
        public ICompanyRepository Company { private set; get; }
        public IApplicationUserRepository ApplicationUser { private set; get; }
        public IShoppingCartRepository ShoppingCart { private set; get; }
        public IOrderHeaderRepository OrderHeader { private set; get; }
        public IOrderDetailsRepository OrderDetails { private set; get; }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
